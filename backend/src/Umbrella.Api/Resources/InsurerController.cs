using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbrella.Api.Dtos;
using Umbrella.Api.Entities.Enums;
using Umbrella.Api.Services;

namespace Umbrella.Api.Resources;

[ApiController]
[Route("/insurers")]
[Authorize("contributor")]
public class InsurerController : ControllerBase
{
    private readonly InsurerService _service;

    public InsurerController(InsurerService service)
    {
        _service = service;
    }

    [HttpGet]
    public IActionResult FindAll([FromQuery] ProposalType? type = null)
    {
        var dtos = type == null ? _service.FindAll() : _service.FindAllWithParameters(type.Value);
        return Ok(dtos);
    }

    [HttpGet("{id:int}")]
    public IActionResult FindById(int id, [FromQuery] ProposalType? type = null)
    {
        InsurerDTO dto = type == null ? _service.FindById(id) : _service.FindByIdWithParameters(id, type.Value);
        return Ok(dto);
    }

    [HttpPut("{id:int}"), Authorize("admin")]
    public IActionResult UpdateParameters(int id, [FromBody] ParametersDTO data)
    {
        _service.UpdateParameters(id, data);
        return Ok();
    }
}