using App.Application.Dto;

namespace App.Application.Interfaces;

public interface IEmailService
{
    Task<bool> SendEmailAsync(EmailRequest request);
    Task<bool> SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true);
}