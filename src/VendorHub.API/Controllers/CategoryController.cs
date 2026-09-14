using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VendorHub.AppService.Interfaces;
using VendorHub.Contracts.Catalog;
using VendorHub.Contracts.Common;

namespace VendorHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        var categories = await _categoryService.GetAllAsync();
        return Ok(ApiResponse<List<CategoryDto>>.SuccessResponse(categories, "Categories retrieved successfully"));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(CategoryCreateDto request)
    {
        try
        {
            var category = await _categoryService.CreateAsync(request);
            return Ok(ApiResponse<CategoryDto>.SuccessResponse(category, "Category created successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<CategoryDto>.FailureResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<CategoryDto>.FailureResponse(ex.Message));
        }
    }
}
