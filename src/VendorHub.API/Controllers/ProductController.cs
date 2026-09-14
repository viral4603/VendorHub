using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VendorHub.AppService.Interfaces;
using VendorHub.Contracts.Catalog;
using VendorHub.Contracts.Common;

namespace VendorHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll([FromQuery] ProductFilterDto filter)
    {
        try
        {
            var products = await _productService.GetAllAsync(filter);
            return Ok(ApiResponse<List<ProductResponseDto>>.SuccessResponse(products, "Products retrieved successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<List<ProductResponseDto>>.FailureResponse(ex.Message));
        }
    }

    [HttpGet("my-products")]
    [Authorize(Roles = "Vendor")]
    public async Task<IActionResult> GetMyProducts()
    {
        try
        {
            var products = await _productService.GetMyProductsAsync(GetCurrentUserId());
            return Ok(ApiResponse<List<ProductResponseDto>>.SuccessResponse(products, "Products retrieved successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<List<ProductResponseDto>>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<List<ProductResponseDto>>.FailureResponse(ex.Message));
        }
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var product = await _productService.GetByIdAsync(id);
            return Ok(ApiResponse<ProductResponseDto>.SuccessResponse(product, "Product retrieved successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<ProductResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpPost]
    [Authorize(Roles = "Vendor")]
    public async Task<IActionResult> Create(ProductCreateDto request)
    {
        try
        {
            var product = await _productService.CreateAsync(GetCurrentUserId(), request);
            return Ok(ApiResponse<ProductResponseDto>.SuccessResponse(product, "Product created successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<ProductResponseDto>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<ProductResponseDto>.FailureResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<ProductResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Vendor")]
    public async Task<IActionResult> Update(int id, ProductUpdateDto request)
    {
        try
        {
            var product = await _productService.UpdateAsync(GetCurrentUserId(), id, request);
            return Ok(ApiResponse<ProductResponseDto>.SuccessResponse(product, "Product updated successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<ProductResponseDto>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<ProductResponseDto>.FailureResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<ProductResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Vendor")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _productService.DeleteAsync(GetCurrentUserId(), id);
            return Ok(ApiResponse.SuccessResponse("Product deleted successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse.FailureResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse.FailureResponse(ex.Message));
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
