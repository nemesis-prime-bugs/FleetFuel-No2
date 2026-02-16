using FleetFuel.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace FleetFuel.Api.Controllers;

/// <summary>
/// API controller for Trip CRUD operations.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class TripsController : ControllerBase
{
    private readonly ITripService _service;

    public TripsController(ITripService service)
    {
        _service = service;
    }

    /// <summary>
    /// GET /api/v1/trips
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var trips = await _service.GetAllAsync(userId.Value);
        return Ok(trips);
    }

    /// <summary>
    /// GET /api/v1/trips/{id}
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var trip = await _service.GetByIdAsync(id, userId.Value);
        if (trip == null) return NotFound();

        return Ok(trip);
    }

    /// <summary>
    /// GET /api/v1/trips/vehicle/{vehicleId}
    /// </summary>
    [HttpGet("vehicle/{vehicleId:guid}")]
    public async Task<IActionResult> GetByVehicleId(Guid vehicleId)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        try
        {
            var trips = await _service.GetByVehicleIdAsync(vehicleId, userId.Value);
            return Ok(trips);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// POST /api/v1/trips
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTripRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        try
        {
            var trip = await _service.CreateAsync(request, userId.Value);
            return CreatedAtAction(nameof(GetById), new { id = trip.Id }, trip);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// PUT /api/v1/trips/{id}
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTripRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        try
        {
            var trip = await _service.UpdateAsync(id, request, userId.Value);
            if (trip == null) return NotFound();

            return Ok(trip);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// DELETE /api/v1/trips/{id}
    /// </summary>
    [HttpDelete("{id:guid}")]
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
