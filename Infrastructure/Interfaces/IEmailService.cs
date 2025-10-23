using Domain.DTOs.EmailDto;

namespace Infrastructure.Interfaces;

public interface IEmailService
{
   Task SendEmail(SendEmail dto);
}