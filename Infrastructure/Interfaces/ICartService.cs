using Domain.DTOs.CartItemDto;
using Domain.Responces;

namespace Infrastructure.Interfaces;

public interface ICartService
{
    Task<Responce<string>> AddToCart(CreateCartItemDto create, int userId);
    Task<Responce<string>> UpdateCart(UpdateCartItemDto update, int userId);
    Task<Responce<string>> DeleteCart(int id, int userId);
    Task<Responce<List<GetCartItemDto>>> GetCartItem(int userId);
}