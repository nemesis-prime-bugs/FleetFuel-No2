using FleetFuel.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace FleetFuel.Api.Controllers;

/// <summary>
/// API controller for Summary/Reporting operations.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class SummaryController : ControllerBase
{
    private readonly ISummaryService _service;

    public SummaryController(ISummaryService service)
    {
        _service = service;
    }

    /// <summary>
    /// GET /api/v1/summary?year=2024
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetYearlySummary([FromQuery] int year)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        if (year < 2000 || year > 2100)
        {
            return BadRequest(new { error = "Invalid year" });
        }

        try
        {
            var summary = await _service.GetYearlySummaryAsync(userId.Value, year);
            return Ok(summary);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// GET /api/v1/summary/export?year=2024
    /// </summary>
    [HttpGet("export")]
    public async Task<IActionResult> ExportCsv([FromQuery] int year)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        if (year < 2000 || year > 2100)
        {
            return BadRequest(new { error = "Invalid year" });
        }

        try
        {
            var csvBytes = await _service.ExportYearlySummaryCsvAsync(userId.Value, year);
            return File(csvBytes, "text/csv", $"fleetfuel-summary-{year}.csv");
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    private Guid? GetUserId()
    {
        var userId = HttpContext.Items["UserId"] as string;
        return userId != null ? Guid.Parse(userId) : null;
    }
}