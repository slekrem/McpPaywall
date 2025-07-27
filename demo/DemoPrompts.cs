using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace McpPaywallDemo;

/// <summary>
/// Simple demo MCP prompts that require payment to access
/// </summary>
[McpServerPromptType]
public static class DemoPrompts
{
    [McpServerPrompt, Description("Generate a simple email template.")]
    public static async Task<string> EmailTemplate(
        string recipient = "[Recipient Name]",
        string subject = "[Subject]",
        [FromServices] IHttpContextAccessor httpContextAccessor = null!)
    {
        var context = httpContextAccessor.HttpContext;
        var userId = context?.Items["McpPaywall.UserId"]?.ToString() ?? "unknown";
        
        Console.WriteLine($"[DEMO] Paid user {userId} requesting email template");

        await Task.Delay(100);

        var template = $@"Subject: {subject}

Dear {recipient},

I hope this email finds you well.

[Your message content here]

Best regards,
[Your Name]";
        
        return JsonSerializer.Serialize(new
        {
            template = template,
            recipient = recipient,
            subject = subject,
            generated_at = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC"),
            user_id = userId
        }, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerPrompt, Description("Generate a simple meeting agenda.")]
    public static async Task<string> MeetingAgenda(
        string title = "Team Meeting",
        int duration = 60,
        [FromServices] IHttpContextAccessor httpContextAccessor = null!)
    {
        var context = httpContextAccessor.HttpContext;
        var userId = context?.Items["McpPaywall.UserId"]?.ToString() ?? "unknown";
        
        Console.WriteLine($"[DEMO] Paid user {userId} requesting meeting agenda");

        await Task.Delay(100);

        var agenda = new[]
        {
            "Welcome & Introductions (5 min)",
            "Project Updates (20 min)",
            "Discussion Topics (25 min)",
            "Action Items & Next Steps (10 min)"
        };

        return JsonSerializer.Serialize(new
        {
            meeting_title = title,
            duration_minutes = duration,
            agenda_items = agenda,
            generated_at = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC"),
            user_id = userId
        }, new JsonSerializerOptions { WriteIndented = true });
    }
}