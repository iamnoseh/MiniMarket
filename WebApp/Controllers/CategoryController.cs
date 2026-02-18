using Domain.DTOs.CategoryDto;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Extensions;
 
namespace WebApp.Controllers;
[ApiController]
[Route("api/[controller]")]
public class CategoryController(ICategoryService  service):ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateCategory(CreateCategoryDto category)
    {
        var  res = await service.CreateCategory(category);
        return this.ToActionResult(res);
    }

    [HttpPut]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateCategory(UpdateCategoryDto category)
    {
        var  res = await service.UpdateCategory(category);
        return this.ToActionResult(res);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var  res = await service.DeleteCategory(id);
        return this.ToActionResult(res);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetCategories()
    {
        var res = await service.GetCategory();
        return this.ToActionResult(res);
    }
}