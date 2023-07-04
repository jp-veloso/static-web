using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbrella.Api.Services;
using Umbrella.Api.Utils;

namespace Umbrella.Api.Resources;


[ApiController]
[Route("/analysis")]
[Authorize("admin")]
public class RiskAnalysisController : ControllerBase
{

    private readonly RiskAnalysisService _service;

    public RiskAnalysisController(RiskAnalysisService service)
    {
        _service = service;
    }

    [HttpGet("table")]
    public IActionResult GetTable()
    {
        return Ok(_service.GetAnalysisTable());
    }

    [HttpGet("ratings")]
    public IActionResult GetRatings([FromQuery] string cnpj)
    {
        return Ok(_service.GetRatingFromClient(TextUtil.UnformatCNPJ(cnpj)));
    }

    [HttpGet("predict")]
    public IActionResult ExecutePrediction([FromQuery] string rating, [FromQuery] bool useCompanyRating = false)
    {
        return Ok(_service.Predict(rating, useCompanyRating));
    }
}