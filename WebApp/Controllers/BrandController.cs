using Domain.DTOs.BrandDto;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;


[ApiController]
[Route("api/[controller]")]
public class BrandController(IBrandService service) :  ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Post(CreateBrandDto dto)
    {
        var res =  await service.CreateBrand(dto);
        return Ok(res);
    }

    [HttpPut]
    public async Task<IActionResult> Put(UpdateBrandDto dto)
    {
        var res = await service.UpdateBrand(dto);
        return Ok(res);
    }

    [HttpDelete]
    public async Task<IActionResult> Delete(int id)
    {
        var res = await service.DeleteBrand(id);
        return Ok(res);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var res = await service.GetBrand(id);
        return Ok(res);
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var res = await service.GetBrands();
        return Ok(res);
    }
}