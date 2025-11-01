using Domain.DTOs.BrandDto;
using Domain.Responces;

namespace Infrastructure.Interfaces;

public interface IBrandService
{
    Task<Responce<string>> CreateBrand(CreateBrandDto dto);
    Task<Responce<string>> UpdateBrand(UpdateBrandDto dto);
    Task<Responce<string>> DeleteBrand(int id);
    Task<Responce<GetBrandDto>> GetBrand(int id);
    Task<Responce<List<GetBrandDto>>> GetBrands();
}