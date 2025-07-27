using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace McpPaywallDemo;

/// <summary>
/// Simple demo MCP resources that require payment to access
/// </summary>
[McpServerResourceType]
public static class DemoResources
{
    [McpServerResource, Description("Access to productivity tips.")]
    public static async Task<string> ProductivityTips(
        [FromServices] IHttpContextAccessor httpContextAccessor)
    {
        var context = httpContextAccessor.HttpContext;
        var userId = context?.Items["McpPaywall.UserId"]?.ToString() ?? "unknown";
        
        Console.WriteLine($"[DEMO] Paid user {userId} accessing productivity tips");

        await Task.Delay(100);

        var tips = new[]
        {
            "Use the Pomodoro Technique: 25-minute focused work sessions",
            "Prioritize tasks using a simple to-do list",
            "Take regular breaks to maintain focus",
            "Eliminate distractions during work hours",
            "Set clear daily goals"
        };

        return JsonSerializer.Serialize(new
        {
            title = "Productivity Tips",
            tips = tips,
            user_id = userId,
            accessed_at = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC")
        }, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerResource, Description("List of useful development tools.")]
    public static async Task<string> DevTools(
        [FromServices] IHttpContextAccessor httpContextAccessor)
    {
        var context = httpContextAccessor.HttpContext;
        var userId = context?.Items["McpPaywall.UserId"]?.ToString() ?? "unknown";
        
        Console.WriteLine($"[DEMO] Paid user {userId} accessing dev tools");

        await Task.Delay(100);

        var tools = new[]
        {
            new { name = "Visual Studio Code", category = "Editor", url = "https://code.visualstudio.com" },
            new { name = "Git", category = "Version Control", url = "https://git-scm.com" },
            new { name = "Postman", category = "API Testing", url = "https://postman.com" },
            new { name = "Docker", category = "Containerization", url = "https://docker.com" }
        };

        return JsonSerializer.Serialize(new
        {
            title = "Development Tools",
            tools = tools,
            user_id = userId,
            accessed_at = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC")
        }, new JsonSerializerOptions { WriteIndented = true });
    }
}