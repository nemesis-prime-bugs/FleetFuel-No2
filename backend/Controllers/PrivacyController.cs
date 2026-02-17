using FleetFuel.Api.Models;
using FleetFuel.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace FleetFuel.Api.Controllers;

/// <summary>
/// API controller for privacy and data management operations.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class PrivacyController : ControllerBase
{
    private readonly IPrivacyService _privacyService;

    public PrivacyController(IPrivacyService privacyService)
    {
        _privacyService = privacyService;
    }

    /// <summary>
    /// GET /api/v1/privacy/settings
    /// </summary>
    [HttpGet("settings")]
    [ProducesResponseType(typeof(ApiResponse<PrivacySettings>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetPrivacySettings()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized(new ApiResponse<PrivacySettings> { Error = "Unauthorized" });

        var settings = await _privacyService.GetPrivacySettingsAsync(userId.Value);
        if (settings == null) return NotFound(new ApiResponse<PrivacySettings> { Error = "User not found" });

        return Ok(new ApiResponse<PrivacySettings> { Success = true, Data = settings });
    }

    /// <summary>
    /// PUT /api/v1/privacy/settings
    /// </summary>
    [HttpPut("settings")]
    [ProducesResponseType(typeof(ApiResponse<PrivacySettings>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdatePrivacySettings([FromBody] UpdatePrivacyRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized(new ApiResponse<PrivacySettings> { Error = "Unauthorized" });

        var settings = await _privacyService.UpdatePrivacySettingsAsync(userId.Value, request);
        if (settings == null) return NotFound(new ApiResponse<PrivacySettings> { Error = "User not found" });

        return Ok(new ApiResponse<PrivacySettings> { Success = true, Data = settings });
    }

    /// <summary>
    /// POST /api/v1/privacy/export
    /// </summary>
    [HttpPost("export")]
    [ProducesResponseType(typeof(ApiResponse<DataExportStatus>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RequestDataExport()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized(new ApiResponse<DataExportStatus> { Error = "Unauthorized" });

        var result = await _privacyService.RequestDataExportAsync(userId.Value);
        
        if (!result.Success)
        {
            return BadRequest(new ApiResponse<DataExportStatus>
            {
                Success = false,
                Error = result.Message
            });
        }

        return Ok(new ApiResponse<DataExportStatus> { Success = true, Data = result });
    }

    /// <summary>
    /// POST /api/v1/privacy/delete
    /// </summary>
    [HttpPost("delete")]
    [ProducesResponseType(typeof(ApiResponse<DeleteAccountResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RequestAccountDeletion([FromBody] DeleteAccountRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized(new ApiResponse<DeleteAccountResponse> { Error = "Unauthorized" });

        var result = await _privacyService.RequestAccountDeletionAsync(userId.Value, request);
        
        if (!result.Success)
        {
            return BadRequest(new ApiResponse<DeleteAccountResponse>
            {
                Success = false,
                Error = result.Message
            });
        }

        return Ok(new ApiResponse<DeleteAccountResponse> { Success = true, Data = result });
    }

    /// <summary>
    /// POST /api/v1/privacy/delete/cancel
    /// </summary>
    [HttpPost("delete/cancel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CancelAccountDeletion()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var success = await _privacyService.CancelAccountDeletionAsync(userId.Value);
        
        if (!success)
        {
            return BadRequest(new { error = "Could not cancel deletion" });
        }

        return Ok(new { success = true, message = "Account deletion cancelled" });
    }

    private Guid? GetUserId()
    {
        var userId = HttpContext.Items["UserId"] as string;
        return userId != null ? Guid.Parse(userId) : null;
    }
}