using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VendorHub.AppService.Interfaces;
using VendorHub.Contracts.Common;
using VendorHub.Contracts.Vendor;

namespace VendorHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VendorController : ControllerBase
{
    private readonly IVendorService _vendorService;

    public VendorController(IVendorService vendorService)
    {
        _vendorService = vendorService;
    }

    [HttpPost("apply")]
    [Authorize(Roles = "Vendor")]
    public async Task<IActionResult> Apply(VendorApplyDto request)
    {
        try
        {
            var vendor = await _vendorService.ApplyAsync(GetCurrentUserId(), request);
            return Ok(ApiResponse<VendorResponseDto>.SuccessResponse(vendor, "Vendor application submitted successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<VendorResponseDto>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<VendorResponseDto>.FailureResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<VendorResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpGet("my-status")]
    [Authorize(Roles = "Vendor")]
    public async Task<IActionResult> GetMyStatus()
    {
        try
        {
            var vendor = await _vendorService.GetMyStatusAsync(GetCurrentUserId());
            return Ok(ApiResponse<VendorResponseDto>.SuccessResponse(vendor, "Vendor status retrieved successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<VendorResponseDto>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<VendorResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll()
    {
        var vendors = await _vendorService.GetAllAsync();
        return Ok(ApiResponse<List<VendorResponseDto>>.SuccessResponse(vendors, "Vendors retrieved successfully"));
    }

    [HttpPut("{id:int}/review")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Review(int id, ReviewVendorRequestDto request)
    {
        try
        {
            var vendor = await _vendorService.ReviewAsync(id, request);
            return Ok(ApiResponse<VendorResponseDto>.SuccessResponse(vendor, $"Vendor {vendor.Status.ToLower()} successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<VendorResponseDto>.FailureResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<VendorResponseDto>.FailureResponse(ex.Message));
        }
    }

    private int GetCurrentUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userId, out var parsedUserId))
            throw new UnauthorizedAccessException("The current user could not be identified from the token.");

        return parsedUserId;
    }
}
