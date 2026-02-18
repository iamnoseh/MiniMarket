using Domain.DTOs.UserDto;
using Domain.Entities;

namespace Infrastructure.Mapping;

public static class UserMapping
{
    public static GetUserDto ToDto(this User user)
    {
        return new GetUserDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Age = user.Age,
            Address = user.Address,
            PhoneNumber = user.PhoneNumber,
            AvatarUrl = user.AvatarUrl,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }
}
