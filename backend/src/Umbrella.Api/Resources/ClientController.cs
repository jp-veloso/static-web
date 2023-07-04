using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbrella.Api.Dtos;
using Umbrella.Api.ProposalResources;
using Umbrella.Api.Services;
using Umbrella.Api.Utils.Pagination;

namespace Umbrella.Api.Resources;

[ApiController]
[Route("/clients")]
[Authorize("contributor")]
public class ClientController : ControllerBase
{
    private readonly ClientService _clService;
    private readonly EnrollmentService _enService;

    public ClientController(ClientService clService, EnrollmentService enService)
    {
        _clService = clService;
        _enService = enService;
    }

    [HttpGet]
    public IActionResult FindAllPaged([FromQuery] Pageable pageable, [FromQuery] string? filter)
    {
        Page<ClientDTO> items = _clService.FindAllPaged(pageable, filter);
        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public IActionResult FindById(int id)
    {
        ClientDTO dto = _clService.FindById(id);
        return Ok(dto);
    }

    [HttpPost("byCnpj", Name = "byCnpj")]
    public IActionResult CreateByCnpj([FromBody] JsonDocument document)
    {
        string name = document.RootElement.GetProperty("name").ToString();
        string cnpj = document.RootElement.GetProperty("cnpj").ToString();
        int companyId = document.RootElement.GetProperty("companyId").GetInt32();

        ClientDTO dto = _clService.CreateByCnpj(cnpj, name, companyId);

        return Ok(dto);
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        _clService.DeleteById(id);
        return NoContent();
    }

    [HttpPost]
    public IActionResult Insert([FromBody] ClientDTO payload, [FromQuery] bool withEnrollments = false)
    {
        ClientDTO dto = _clService.Insert(payload, withEnrollments);
        return CreatedAtAction(nameof(FindById), new {dto.Id}, dto);
    }

    [HttpPost("{id:int}/enrollments/reload")]
    public IActionResult ReloadEnrollments(int id, [FromQuery] string insurers = "")
    {
        int[] ints = string.IsNullOrEmpty(insurers)
                         ? Array.Empty<int>()
                         : insurers.Split(",")
                                   .Select(int.Parse)
                                   .ToArray();

        _clService.ReloadEnrollments(id, ints);
        return NoContent();
    }

    [HttpGet("{id:int}/enrollments/appointment")]
    public IActionResult GenerateAppointment(int id, [FromServices] IWebHostEnvironment env,
                                             [FromQuery] string insurers = "")
    {
        int[] ints = string.IsNullOrEmpty(insurers)
                         ? Array.Empty<int>()
                         : insurers.Split(",")
                                   .Select(int.Parse)
                                   .ToArray();

        byte[] pdfBytes = _clService.GenerateAppointment(id, ints, env.ContentRootPath);

        return File(pdfBytes, "application/pdf", "carta_de_nomeacao.pdf");
    }

    // Enrollments
    // Se não for POST o body não é desserializado corretamente
    [HttpPost("{id:int}/enrollments/proposals")]
    public IActionResult GenerateProposals(int id, [FromBody] ProposalInfo info)
    {
        List<Proposal> result = _enService.GenerateProposals(id, info);
        return Ok(result);
    }

    [HttpGet("{clientId:int}/enrollments")]
    public IActionResult FindEnrollments(int clientId)
    {
        IEnumerable<EnrollmentDTO> enrollments = _enService.FindAll(clientId);
        return Ok(new {data = enrollments});
    }

    [HttpPost("{clientId:int}/enrollments")]
    public IActionResult InsertEnrollment(int clientId, [FromBody] EnrollmentDTO payload)
    {
        EnrollmentDTO result = _enService.Insert(clientId, payload);
        return CreatedAtAction(nameof(FindEnrollments), new {result.ClientId}, result);
    }

    [HttpPut("{clientId:int}/enrollments/{insurerId:int}")]
    public IActionResult UpdateEnrollment(int clientId, int insurerId, [FromBody] EnrollmentDTO payload)
    {
        EnrollmentDTO result = _enService.Update(clientId, insurerId, payload);
        return Ok(result);
    }

    [HttpDelete("{clientId:int}/enrollments/{insurerId:int}")]
    public IActionResult DeleteEnrollment(int clientId, int insurerId)
    {
        _enService.Delete(clientId, insurerId);
        return NoContent();
    }
}