using FleetFuel.Api.Services;
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
    public async Task<IActionResult> GetAll()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var vehicles = await _service.GetAllAsync(userId.Value);
        return Ok(vehicles);
    }

    /// <summary>
    /// GET /api/v1/vehicles/{id}
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var vehicle = await _service.GetByIdAsync(id, userId.Value);
        if (vehicle == null) return NotFound();

        return Ok(vehicle);
    }

    /// <summary>
    /// POST /api/v1/vehicles
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateVehicleRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var vehicle = await _service.CreateAsync(request, userId.Value);
        return CreatedAtAction(nameof(GetById), new { id = vehicle.Id }, vehicle);
    }

    /// <summary>
    /// PUT /api/v1/vehicles/{id}
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateVehicleRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var vehicle = await _service.UpdateAsync(id, request, userId.Value);
        if (vehicle == null) return NotFound();

        return Ok(vehicle);
    }

    /// <summary>
    /// DELETE /api/v1/vehicles/{id}
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