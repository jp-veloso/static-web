using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Umbrella.Api.Contexts;
using Umbrella.Api.Dtos;
using Umbrella.Api.Entities;
using Umbrella.Api.Services;
using JsonClaimValueTypes = Microsoft.IdentityModel.JsonWebTokens.JsonClaimValueTypes;

namespace Umbrella.Api.Resources;

[ApiController]
[Route("/auth")]
public class UserController : ControllerBase
{
    [HttpPost("register")]
    [Authorize("admin")]
    public IActionResult Register([FromBody] UserDTO body, [FromServices] IPasswordHasher<User> hasher)
    {
        User user = new(body.Username, body.Name ?? "", body.Roles ?? new[] {"contributor"});
        user.Password = hasher.HashPassword(user, body.Password);

        using RepositoryContext db = new();

        db.Users.Add(user);
        db.SaveChanges();

        return Ok("User created");
    }

    [HttpGet("me")]
    [Authorize("contributor")]
    public IActionResult Me([FromServices] UserService service)
    {
        int userId = int.Parse(HttpContext.User.FindFirst("id")!.Value);
        return Ok(new {data = service.Me(userId)});
    }

    [HttpGet("kpis")]
    [AllowAnonymous]
    public IActionResult Me([FromQuery(Name = "token")] string secret, [FromServices] UserService service)
    {
        if (secret != "eyJuYW1lIjoiVmljdG9yIEJhbWJvenppIiwicm9sZSI6WyJBRE1JTiJdfQ")
        {
            return Unauthorized("Acesso negado");
        }

        return Ok(new {data = service.MePublic()});
    }
    
    [HttpGet("v2/kpis")]
    [AllowAnonymous]
    public IActionResult KpisRecord([FromQuery(Name = "token")] string secret, [FromQuery(Name = "date")] DateTime? date, [FromServices] UserService service)
    {
        if (secret != "eyJuYW1lIjoiVmljdG9yIEJhbWJvenppIiwicm9sZSI6WyJBRE1JTiJdfQ")
        {
            return Unauthorized("Acesso negado");
        }

        date ??= DateTime.UtcNow;

        return Ok(service.GetKpisRecordsAsync(date.Value).Result);
    }

    [HttpPost("google")]
    [Consumes("application/x-www-form-urlencoded")]
    [AllowAnonymous]
    public async Task<IActionResult> GoogleAuth([FromForm] GoogleDTO auth, [FromServices] IConfiguration configuration)
    {
        if (!auth.Hd.Equals("grantoseguros.com"))
        {
            return Unauthorized();
        }

        (string email, string name, string? picture) = await ExchangeCodeAsync(auth, configuration);

        await using RepositoryContext db = new();

        User? user = db.Users.SingleOrDefault(x => x.Username.Equals(email));

        if (user == null)
        {
            user = new User(email, name, new[] {"contributor"});
            await db.Users.AddAsync(user);
        }

        user.LastLogin = DateTime.UtcNow;
        await db.SaveChangesAsync();

        string token = GenerateToken(user);

        return Ok(new {name, picture, token});
    }

    private static string GenerateToken(User user)
    {
        JsonWebTokenHandler handler = new();

        string secret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? "fedaf7d8863b48e197b9287d492b708e";

        string token = handler.CreateToken(new SecurityTokenDescriptor
                                           {
                                               Subject = new ClaimsIdentity(new[]
                                                                            {
                                                                                new Claim("username",
                                                                                 user.Username),
                                                                                new Claim("id",
                                                                                 $"{user.Id}"),
                                                                                new Claim("roles",
                                                                                 JsonConvert
                                                                                    .SerializeObject(user
                                                                                        .Authorities),
                                                                                 JsonClaimValueTypes
                                                                                    .JsonArray),
                                                                                new Claim("dep",
                                                                                 user.Department ??
                                                                                 "none")
                                                                            }),
                                               Expires = DateTime.UtcNow.AddHours(2),
                                               SigningCredentials =
                                                   new
                                                       SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret)),
                                                                          SecurityAlgorithms
                                                                             .HmacSha256Signature)
                                           });
        return token;
    }

    private async Task<(string email, string name, string? picture)> ExchangeCodeAsync(
        GoogleDTO auth, IConfiguration configuration)
    {
        string redirect = Request.Headers["origin"]
                                 .ToString();

        Console.WriteLine("REDIRECT TO: " + redirect);

        Dictionary<string, string> tokenRequestParameters = new()
                                                            {
                                                                {
                                                                    "client_id",
                                                                    configuration.GetSection("Google")["ClientId"]
                                                                    !
                                                                },
                                                                {
                                                                    "client_secret",
                                                                    configuration.GetSection("Google")["Secret"]!
                                                                },
                                                                {"code", auth.Code},
                                                                {
                                                                    "redirect_uri",
                                                                    string.IsNullOrEmpty(redirect)
                                                                        ? configuration.GetSection("Google")
                                                                            ["Redirect"]!
                                                                        : redirect
                                                                },
                                                                {"grant_type", "authorization_code"}
                                                            };

        using HttpClient client = new();

        HttpRequestMessage requestMessage = new(HttpMethod.Post, "https://oauth2.googleapis.com/token");
        requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        requestMessage.Content = new FormUrlEncodedContent(tokenRequestParameters);
        requestMessage.Version = HttpVersion.Version11;
        HttpResponseMessage response = await client.SendAsync(requestMessage);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException("ID Token not found", null, response.StatusCode);
        }

        JsonDocument document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        JwtPayload payload = JwtPayload.Base64UrlDeserialize(document.RootElement.GetString("id_token")!.Split(".")[1]);

        string? picture = payload.ContainsKey("picture")
                              ? payload["picture"]
                                 .ToString()!
                              : null;

        return (payload["email"]
                   .ToString()!, payload["given_name"]
                   .ToString()!, picture);
    }

    [HttpPost("login")]
    [Consumes("application/x-www-form-urlencoded")]
    [AllowAnonymous]
    public IActionResult Login([FromForm] UserDTO body, [FromServices] IPasswordHasher<User> hasher)
    {
        using RepositoryContext db = new();

        User user = db.Users.Single(x => x.Username.Equals(body.Username));

        PasswordVerificationResult result = hasher.VerifyHashedPassword(user, user.Password, body.Password);

        if (result == PasswordVerificationResult.Failed)
        {
            return Unauthorized();
        }

        user.LastLogin = DateTime.UtcNow;
        db.SaveChanges();

        string token = GenerateToken(user);

        return Ok(token);
    }

    [HttpPost("npe"), AllowAnonymous] 
    public IActionResult Npe()
    {
        string path = @"C:\Users\dti-m\Desktop\CNPJS.txt";

        string[] lines = System.IO.File.ReadAllLines(path);

        using var db = new RepositoryContext();
        
        foreach (var line in lines)
        {
            Console.WriteLine("LINE: " + line);
            
            string[] values = line.Split(";");

            if (values.Length < 2)
            {
                continue;
            }

            string code = values[1];
            string cnpj = values[0];

            Client client = db.Clients.Single(x => x.Cnpj.Equals(cnpj));
            client.Pipe = code;

            db.Clients.Update(client);
        }

        db.SaveChanges();

        return Ok();
    }
    
}