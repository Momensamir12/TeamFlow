namespace App.Application.Dto;

public class EmailRequest
{
    public required string ToEmail { get; set; }
    public string? ToName { get; set; }
    public required string Subject { get; set; }
    public required string Body { get; set; }
    public bool IsHtml { get; set; } = true;
    public List<string>? CcEmails { get; set; }
    public List<string>? BccEmails { get; set; }
}