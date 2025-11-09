using App.Application.Interfaces;
using App.Application.Dto;
using App.Infrastructure.Settings;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using MailKit.Net.Smtp;
using MimeKit;
using MailKit.Security;

namespace App.Infrastructure.Services;

public class GmailEmailService : IEmailService
{
    private readonly ILogger<GmailEmailService> _logger;
    private readonly EmailSettings _emailSettings;

    public GmailEmailService(
        IOptions<EmailSettings> emailSettings, 
        ILogger<GmailEmailService> logger)
    {
        _emailSettings = emailSettings.Value;
        _logger = logger;
        
        if (string.IsNullOrWhiteSpace(_emailSettings.SenderEmail))
        {
            throw new InvalidOperationException("EmailSettings.SenderEmail is not configured");
        }
        
        if (string.IsNullOrWhiteSpace(_emailSettings.Password))
        {
            throw new InvalidOperationException("EmailSettings.Password is not configured");
        }
        
        _logger.LogInformation("Gmail email service initialized with sender: {SenderEmail}", 
            _emailSettings.SenderEmail);
    }

    public async Task<bool> SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true)
    {
        var request = new EmailRequest
        {
            ToEmail = toEmail,
            Subject = subject,
            Body = body,
            IsHtml = isHtml
        };

        return await SendEmailAsync(request);
    }

    public async Task<bool> SendEmailAsync(EmailRequest request)
    {
        try
        {
            _logger.LogInformation("Preparing to send email via Gmail to: {To}", request.ToEmail);

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
            message.To.Add(new MailboxAddress(request.ToName ?? "", request.ToEmail));
            message.Subject = request.Subject;

            var bodyBuilder = new BodyBuilder();
            if (request.IsHtml)
            {
                bodyBuilder.HtmlBody = request.Body;
            }
            else
            {
                bodyBuilder.TextBody = request.Body;
            }
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            
            _logger.LogInformation("Connecting to Gmail SMTP server...");
            await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, SecureSocketOptions.StartTls);
            
            _logger.LogInformation("Authenticating with Gmail...");
            await client.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);
            
            _logger.LogInformation("Sending email...");
            await client.SendAsync(message);
            
            await client.DisconnectAsync(true);

            _logger.LogInformation("Email sent successfully via Gmail to: {To}", request.ToEmail);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email via Gmail to: {To}", request.ToEmail);
            return false;
        }
    }
}