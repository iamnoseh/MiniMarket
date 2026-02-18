using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Domain.DTOs.OrderDto;

public class CreateOrderDto
{
    public Status Status{get;set;}
    [Required]
    public required string Address{get;set;}
    public PaymentMethod PaymentMethod {get;set;}
}