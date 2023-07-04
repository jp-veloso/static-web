using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbrella.Api.Contexts;
using Umbrella.Api.Dtos;
using Umbrella.Api.Entities;
using Umbrella.Api.Services;
using Umbrella.Api.Utils;

namespace Umbrella.Api.Resources;

[ApiController]
[Route("/issues")]
[Authorize("contributor")]
public class IssuedController : ControllerBase
{
    private readonly IssueService _isService;

    public IssuedController(IssueService isService)
    {
        _isService = isService;
    }

    [HttpPost]
    public IActionResult InsertIssue([FromBody] List<InsertIssueDTO> payload)
    {
        List<IssueDTO> results = payload.Select(item => _isService.Insert(item))
                                        .Where(dto => dto != null)
                                        .ToList()!;
        return new ObjectResult(results) {StatusCode = StatusCodes.Status201Created};
    }

    [HttpGet("{id:int}")]
    public IActionResult FindById(int id)
    {
        IssueDTO dto = _isService.FindById(id);
        return Ok(dto);
    }

    [HttpGet("sync-clients")]
    public IActionResult SyncClients()
    {
        using StreamReader reader =
            new(@"C:\Users\Pedro\Desktop\Granto\backend\src\GCB_Portal\Assets\Base-de-Clientes.csv");

        using RepositoryContext db = new();

        List<string> dates = new();
        while (!reader.EndOfStream)
        {
            string[] line = reader.ReadLine()!.Split(",");

            string cnpj = TextUtil.IsCnpj(line[0]) ? TextUtil.UnformatCNPJ(line[0]) : "";

            Console.WriteLine(line[0]);

            if (string.IsNullOrEmpty(cnpj))
            {
                Console.WriteLine("INVALIDO: " + line[0]);
            }

            Client? client = db.Clients.SingleOrDefault(x => x.Cnpj.Equals(cnpj));

            if (client == null)
            {
                dates.Add(line[0] + " :: " + line[1]);
            }
        }

        return Ok(new
                  {
                      amount = dates.Count,
                      values = dates
                  });
    }
}