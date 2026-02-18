using Domain.DTOs.BrandDto;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Extensions;

namespace WebApp.Controllers;


[ApiController]
[Route("api/[controller]")]
public class BrandController(IBrandService service) :  ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateBrand(CreateBrandDto dto)
    {
        var res =  await service.CreateBrand(dto);
        return this.ToActionResult(res);
    }

    [HttpPut]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateBrand(UpdateBrandDto dto)
    {
        var res = await service.UpdateBrand(dto);
        return this.ToActionResult(res);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteBrand(int id)
    {
        var res = await service.DeleteBrand(id);
        return this.ToActionResult(res);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetBrand(int id)
    {
        var res = await service.GetBrand(id);
        return this.ToActionResult(res);
    }

    [HttpGet]
    public async Task<IActionResult> GetBrands()
    {
        var res = await service.GetBrands();
        return this.ToActionResult(res);
    }
}