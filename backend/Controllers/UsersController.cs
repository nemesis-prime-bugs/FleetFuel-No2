using FleetFuel.Api.Models;
using FleetFuel.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace FleetFuel.Api.Controllers;

/// <summary>
/// API controller for user profile and preferences management.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// GET /api/v1/users/profile
    /// </summary>
    [HttpGet("profile")]
    [ProducesResponseType(typeof(ApiResponse<UserProfileResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProfile()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized(new ApiResponse<UserProfileResponse> { Error = "Unauthorized" });

        var profile = await _userService.GetProfileAsync(userId.Value);
        if (profile == null) return NotFound(new ApiResponse<UserProfileResponse> { Error = "User not found" });

        return Ok(new ApiResponse<UserProfileResponse> { Success = true, Data = profile });
    }

    /// <summary>
    /// PUT /api/v1/users/profile
    /// </summary>
    [HttpPut("profile")]
    [ProducesResponseType(typeof(ApiResponse<UserProfileResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized(new ApiResponse<UserProfileResponse> { Error = "Unauthorized" });

        var profile = await _userService.UpdateProfileAsync(userId.Value, request);
        if (profile == null) return NotFound(new ApiResponse<UserProfileResponse> { Error = "User not found" });

        return Ok(new ApiResponse<UserProfileResponse> { Success = true, Data = profile });
    }

    /// <summary>
    /// POST /api/v1/users/avatar
    /// </summary>
    [HttpPost("avatar")]
    [ProducesResponseType(typeof(ApiResponse<UserProfileResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadAvatar([FromBody] UploadAvatarRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized(new ApiResponse<UserProfileResponse> { Error = "Unauthorized" });

        try
        {
            var profile = await _userService.UploadAvatarAsync(userId.Value, request);
            if (profile == null) return NotFound(new ApiResponse<UserProfileResponse> { Error = "User not found" });

            return Ok(new ApiResponse<UserProfileResponse> { Success = true, Data = profile });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ApiResponse<UserProfileResponse> { Error = ex.Message });
        }
    }

    /// <summary>
    /// DELETE /api/v1/users/avatar
    /// </summary>
    [HttpDelete("avatar")]
    [ProducesResponseType(typeof(ApiResponse<UserProfileResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveAvatar()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized(new ApiResponse<UserProfileResponse> { Error = "Unauthorized" });

        var profile = await _userService.RemoveAvatarAsync(userId.Value);
        if (profile == null) return NotFound(new ApiResponse<UserProfileResponse> { Error = "User not found" });

        return Ok(new ApiResponse<UserProfileResponse> { Success = true, Data = profile });
    }

    /// <summary>
    /// GET /api/v1/users/preferences
    /// </summary>
    [HttpGet("preferences")]
    [ProducesResponseType(typeof(ApiResponse<UserPreferences>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPreferences()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized(new ApiResponse<UserPreferences> { Error = "Unauthorized" });

        var preferences = await _userService.GetPreferencesAsync(userId.Value);
        if (preferences == null) return NotFound(new ApiResponse<UserPreferences> { Error = "User not found" });

        return Ok(new ApiResponse<UserPreferences> { Success = true, Data = preferences });
    }

    /// <summary>
    /// PUT /api/v1/users/preferences
    /// </summary>
    [HttpPut("preferences")]
    [ProducesResponseType(typeof(ApiResponse<UserPreferences>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePreferences([FromBody] UpdatePreferencesRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized(new ApiResponse<UserPreferences> { Error = "Unauthorized" });

        var preferences = await _userService.UpdatePreferencesAsync(userId.Value, request);
        if (preferences == null) return NotFound(new ApiResponse<UserPreferences> { Error = "User not found" });

        return Ok(new ApiResponse<UserPreferences> { Success = true, Data = preferences });
    }

    private Guid? GetUserId()
    {
        var userId = HttpContext.Items["UserId"] as string;
        return userId != null ? Guid.Parse(userId) : null;
    }
}