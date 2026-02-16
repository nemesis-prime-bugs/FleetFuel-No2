using FleetFuel.Api.Models;
using FleetFuel.Api.Services;
using FleetFuel.Data;
using Microsoft.AspNetCore.Mvc;

namespace FleetFuel.Api.Controllers;

/// <summary>
/// API controller for Vehicle CRUD operations.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class VehiclesController : ControllerBase
{
    private readonly IVehicleService _service;

    public VehiclesController(IVehicleService service)
    {
        _service = service;
    }

    /// <summary>
    /// GET /api/v1/vehicles
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<Vehicle>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized(new ApiResponse<Vehicle> { Error = "Unauthorized" });

        var vehicles = await _service.GetAllAsync(userId.Value);
        
        var totalItems = vehicles.Count();
        var pagedItems = vehicles
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var pagination = new PaginationMetadata
        {
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
            HasNextPage = page < (int)Math.Ceiling(totalItems / (double)pageSize),
            HasPreviousPage = page > 1
        };

        return Ok(new ApiResponse<List<Vehicle>>
        {
            Success = true,
            Data = pagedItems,
            Pagination = pagination
        });
    }

    /// <summary>
    /// GET /api/v1/vehicles/{id}
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<Vehicle>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized(new ApiResponse<Vehicle> { Error = "Unauthorized" });

        var vehicle = await _service.GetByIdAsync(id, userId.Value);
        if (vehicle == null) return NotFound(new ApiResponse<Vehicle> { Error = "Vehicle not found" });

        return Ok(new ApiResponse<Vehicle> { Success = true, Data = vehicle });
    }

    /// <summary>
    /// POST /api/v1/vehicles
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<Vehicle>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateVehicleRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized(new ApiResponse<Vehicle> { Error = "Unauthorized" });

        try
        {
            var vehicle = await _service.CreateAsync(request, userId.Value);
            return CreatedAtAction(nameof(GetById), new { id = vehicle.Id }, 
                new ApiResponse<Vehicle> { Success = true, Data = vehicle });
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiResponse<Vehicle> { Error = ex.Message });
        }
    }

    /// <summary>
    /// PUT /api/v1/vehicles/{id}
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<Vehicle>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateVehicleRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized(new ApiResponse<Vehicle> { Error = "Unauthorized" });

        var vehicle = await _service.UpdateAsync(id, request, userId.Value);
        if (vehicle == null) return NotFound(new ApiResponse<Vehicle> { Error = "Vehicle not found" });

        return Ok(new ApiResponse<Vehicle> { Success = true, Data = vehicle });
    }

    /// <summary>
    /// DELETE /api/v1/vehicles/{id}
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var deleted = await _service.DeleteAsync(id, userId.Value);
        if (!deleted) return NotFound();

        return NoContent();
    }

    private Guid? GetUserId()
    {
        var userId = HttpContext.Items["UserId"] as string;
        return userId != null ? Guid.Parse(userId) : null;
    }
}
