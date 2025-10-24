using Domain.DTOs.CartItemDto;
using Domain.Responces;

namespace Infrastructure.Interfaces;

public interface ICartService
{
    Task<Responce<string>> AddToCart(CreateCartItemDto create);
    Task<Responce<string>> UpdateCart(UpdateCartItemDto update);
    Task<Responce<string>> DeleteCart();
    Task<Responce<List<GetCartItemDto>>> GetCartItem();
}