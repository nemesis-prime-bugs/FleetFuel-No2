using FleetFuel.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace FleetFuel.Api.Controllers;

/// <summary>
/// API controller for Receipt CRUD operations.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class ReceiptsController : ControllerBase
{
    private readonly IReceiptService _service;

    public ReceiptsController(IReceiptService service)
    {
        _service = service;
    }

    /// <summary>
    /// GET /api/v1/receipts
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var receipts = await _service.GetAllAsync(userId.Value);
        return Ok(receipts);
    }

    /// <summary>
    /// GET /api/v1/receipts/{id}
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var receipt = await _service.GetByIdAsync(id, userId.Value);
        if (receipt == null) return NotFound();

        return Ok(receipt);
    }

    /// <summary>
    /// GET /api/v1/receipts/vehicle/{vehicleId}
    /// </summary>
    [HttpGet("vehicle/{vehicleId:guid}")]
    public async Task<IActionResult> GetByVehicleId(Guid vehicleId)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        try
        {
            var receipts = await _service.GetByVehicleIdAsync(vehicleId, userId.Value);
            return Ok(receipts);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// POST /api/v1/receipts
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateReceiptRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        try
        {
            var receipt = await _service.CreateAsync(request, userId.Value);
            return CreatedAtAction(nameof(GetById), new { id = receipt.Id }, receipt);
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
    /// POST /api/v1/receipts/{id}/upload
    /// Upload a receipt image for an existing receipt.
    /// </summary>
    [HttpPost("{id:guid}/upload")]
    public async Task<IActionResult> UploadImage(Guid id, IFormFile file)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        if (file == null || file.Length == 0)
        {
            return BadRequest(new { error = "No file uploaded" });
        }

        // Validate file type
        var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp", "application/pdf" };
        if (!allowedTypes.Contains(file.ContentType))
        {
            return BadRequest(new { error = "Invalid file type. Allowed: JPEG, PNG, WebP, PDF" });
        }

        // Validate file size (max 5MB)
        if (file.Length > 5 * 1024 * 1024)
        {
            return BadRequest(new { error = "File size must be less than 5MB" });
        }

        try
        {
            var environment = HttpContext.RequestServices.GetService<IWebHostEnvironment>();
            var uploadsPath = Path.Combine(environment?.ContentRootPath ?? "uploads", "receipts");
            
            // Ensure uploads directory exists
            Directory.CreateDirectory(uploadsPath);

            // Generate unique filename
            var extension = Path.GetExtension(file.FileName);
            var filename = $"{id}_{Guid.NewGuid():N}{extension}";
            var filePath = Path.Combine(uploadsPath, filename);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Update receipt with image path (relative path)
            var relativePath = $"/uploads/receipts/{filename}";
            var receipt = await _service.UpdateImagePathAsync(id, relativePath, userId.Value);

            if (receipt == null)
            {
                // Clean up file if receipt not found
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
                return NotFound(new { error = "Receipt not found" });
            }

            return Ok(new { imagePath = relativePath, filename });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to upload file", details = ex.Message });
        }
    }

    /// <summary>
    /// PUT /api/v1/receipts/{id}
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateReceiptRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        try
        {
            var receipt = await _service.UpdateAsync(id, request, userId.Value);
            if (receipt == null) return NotFound();

            return Ok(receipt);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// DELETE /api/v1/receipts/{id}
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