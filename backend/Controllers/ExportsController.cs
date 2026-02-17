using FleetFuel.Api.Models;
using FleetFuel.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace FleetFuel.Api.Controllers;

/// <summary>
/// API controller for data export operations.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class ExportsController : ControllerBase
{
    private readonly IExportService _exportService;

    public ExportsController(IExportService exportService)
    {
        _exportService = exportService;
    }

    /// <summary>
    /// POST /api/v1/exports
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ExportStatusResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateExport([FromBody] ExportRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized(new ApiResponse<ExportStatusResponse> { Error = "Unauthorized" });

        var result = await _exportService.GenerateExportAsync(userId.Value, request);
        
        if (!result.Success)
        {
            return BadRequest(new ApiResponse<ExportStatusResponse>
            {
                Success = false,
                Error = result.Message
            });
        }

        return Ok(new ApiResponse<ExportStatusResponse> { Success = true, Data = result });
    }

    /// <summary>
    /// GET /api/v1/exports
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<ExportMetadata>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ListExports()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized(new ApiResponse<List<ExportMetadata>> { Error = "Unauthorized" });

        var exports = await _exportService.ListExportsAsync(userId.Value);
        return Ok(new ApiResponse<List<ExportMetadata>> { Success = true, Data = exports });
    }

    /// <summary>
    /// GET /api/v1/exports/{id}/download
    /// </summary>
    [HttpGet("{id:guid}/download")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadExport(Guid id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var fileContent = await _exportService.GetExportFileAsync(userId.Value, id);
        if (fileContent == null)
        {
            return NotFound(new { error = "Export not found or expired" });
        }

        var extension = id.ToString().Contains('.') ? 
            $".{id.ToString().Split('.').Last()}" : ".json";

        return File(fileContent, "application/octet-stream", $"fleetfuel_export_{id}{extension}");
    }

    /// <summary>
    /// DELETE /api/v1/exports/{id}
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteExport(Guid id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var success = await _exportService.DeleteExportAsync(userId.Value, id);
        if (!success) return NotFound();

        return NoContent();
    }

    private Guid? GetUserId()
    {
        var userId = HttpContext.Items["UserId"] as string;
        return userId != null ? Guid.Parse(userId) : null;
    }
}