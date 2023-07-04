using System.Globalization;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Quartz;
using RiskAnalysis;
using Umbrella.Api.Entities;
using Umbrella.Api.Resources.Exceptions;
using Umbrella.Api.Services;
using Umbrella.Api.Utils;
using Umbrella.RabbitMQ;
using Umbrella.RabbitMQ.Bus;
using Umbrella.RabbitMQ.Bus.Routers;
using Umbrella.RabbitMQ.Configuration;
using Umbrella.RabbitMQ.Serialization;
using Route = Umbrella.RabbitMQ.Bus.Routers.Route;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

builder.Services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddAuthentication("jwt")
       .AddJwtBearer("jwt", config =>
                            {
                                config.TokenValidationParameters = new TokenValidationParameters
                                                                   {ValidateAudience = false, ValidateIssuer = false};
                                config.MapInboundClaims = false;

                                string secret = Environment.GetEnvironmentVariable("JWT_SECRET") ??
                                                "fedaf7d8863b48e197b9287d492b708e";

                                config.Configuration = new OpenIdConnectConfiguration
                                                       {
                                                           SigningKeys =
                                                           {
                                                               new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret))
                                                           }
                                                       };
                            });

builder.Services.AddAuthorization(config =>
                                  {
                                      config.AddPolicy("contributor", pb =>
                                                                      {
                                                                          pb.RequireAuthenticatedUser()
                                                                            .AddAuthenticationSchemes("jwt")
                                                                            .RequireClaim("roles", "contributor");
                                                                      });

                                      config.AddPolicy("admin", pb =>
                                                                {
                                                                    pb.RequireAuthenticatedUser()
                                                                      .AddAuthenticationSchemes("jwt")
                                                                      .RequireClaim("roles", "admin");
                                                                });
                                  });

builder.Services.AddQuartz(o =>
{
    // 0 6 * * 1-5 * ?
    o.UseMicrosoftDependencyInjectionJobFactory();
    o.AddJob<UpdateJob>(opts => opts.WithIdentity("SendUpdateJob"));
    o.AddTrigger(trigger => { trigger.ForJob("SendUpdateJob").WithIdentity("update-trigger").WithCronSchedule("0 0 6 ? * MON-FRI"); });
}); 

builder.Services.AddQuartzHostedService(o => o.WaitForJobsToComplete = true);

builder.Services.AddCosmosRepository(o =>
{
    o.DatabaseId = "gcb_main";
    o.ContainerBuilder.Configure<MetricRecord>(opt =>
    {
        opt.WithContainer("gcb_kpis_records");
        opt.WithPartitionKey("/id");
    });

});

builder.Services.AddControllers(options =>
                                {
                                    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
                                })
       .AddJsonOptions(options =>
                       {
                           options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                           options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                       });

builder.Services.AddRabbitMQ(cfg => cfg.WithSerializer<NewtonsoftAmqpSerializer>()
                                       .WithConfigurationPrefix("RabbitMQ"));

builder.Services.AddSingleton<IRouteResolver>(_ => new TypeAndFunctionBasedRouter()
                                                  .AddRoute<EnrollPayload>(_ => new Route
                                                                               {
                                                                                   ExchangeName = "eventos",
                                                                                   RoutingKey =
                                                                                       "insurers-integration"
                                                                               })
                                                  .AddRoute<InsPayload>(_ => new Route
                                                                             {
                                                                                 ExchangeName = "eventos",
                                                                                 RoutingKey = "insurers-update"
                                                                             }));

builder.Services.RegisterBus();

builder.Services.AddSingleton<ExchangerSender>();
builder.Services.AddSingleton<ClientService>();
builder.Services.AddSingleton<InsurerService>();
builder.Services.AddSingleton<IssueService>();
builder.Services.AddSingleton<UserService>();
builder.Services.AddSingleton<EnrollmentService>();
builder.Services.AddSingleton<IRiskAnalysisDataProvider, RiskAnalysisProvider>();
builder.Services.AddSingleton<RiskAnalysisService>();
builder.Services.AddSingleton<RiskAnalysisDataService>();

WebApplication app = builder.Build();

app.ConfigureRabbitMQ();

app.UseCors(options => options.AllowAnyOrigin()
                              .AllowAnyMethod()
                              .AllowAnyHeader());

app.UseMiddleware<ExceptionMiddleware>();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseEndpoints(config => config.MapControllers());
app.MapGet("/", context => context.Response.WriteAsync($"API ON {Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}: TEST 0.3.2"));
app.Run();