using FleetFuel.Api.Models;
using FleetFuel.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace FleetFuel.Api.Controllers;

/// <summary>
/// API controller for account security operations.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class SecurityController : ControllerBase
{
    private readonly ISecurityService _securityService;

    public SecurityController(ISecurityService securityService)
    {
        _securityService = securityService;
    }

    /// <summary>
    /// POST /api/v1/security/change-password
    /// </summary>
    [HttpPost("change-password")]
    [ProducesResponseType(typeof(ApiResponse<SuccessResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized(new ApiResponse<SuccessResponse> { Error = "Unauthorized" });

        var result = await _securityService.ChangePasswordAsync(userId.Value, request);
        
        if (!result.Success)
        {
            return BadRequest(new ApiResponse<SuccessResponse> 
            { 
                Success = false, 
                Error = result.Message 
            });
        }

        return Ok(new ApiResponse<SuccessResponse> { Success = true, Data = result });
    }

    /// <summary>
    /// GET /api/v1/security/info
    /// </summary>
    [HttpGet("info")]
    [ProducesResponseType(typeof(ApiResponse<SecurityInfoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSecurityInfo()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized(new ApiResponse<SecurityInfoResponse> { Error = "Unauthorized" });

        var info = await _securityService.GetSecurityInfoAsync(userId.Value);
        if (info == null) return NotFound(new ApiResponse<SecurityInfoResponse> { Error = "User not found" });

        return Ok(new ApiResponse<SecurityInfoResponse> { Success = true, Data = info });
    }

    /// <summary>
    /// GET /api/v1/security/sessions
    /// </summary>
    [HttpGet("sessions")]
    [ProducesResponseType(typeof(ApiResponse<List<SessionInfo>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetSessions()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized(new ApiResponse<List<SessionInfo>> { Error = "Unauthorized" });

        var sessions = await _securityService.GetSessionsAsync(userId.Value);
        return Ok(new ApiResponse<List<SessionInfo>> { Success = true, Data = sessions });
    }

    /// <summary>
    /// DELETE /api/v1/security/sessions/{sessionId}
    /// </summary>
    [HttpDelete("sessions/{sessionId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RevokeSession(string sessionId)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var success = await _securityService.RevokeSessionAsync(userId.Value, sessionId);
        if (!success) return BadRequest(new { error = "Failed to revoke session" });

        return NoContent();
    }

    /// <summary>
    /// DELETE /api/v1/security/sessions
    /// </summary>
    [HttpDelete("sessions")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RevokeAllSessions()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var success = await _securityService.RevokeAllSessionsAsync(userId.Value);
        if (!success) return BadRequest(new { error = "Failed to revoke sessions" });

        return NoContent();
    }

    private Guid? GetUserId()
    {
        var userId = HttpContext.Items["UserId"] as string;
        return userId != null ? Guid.Parse(userId) : null;
    }
}