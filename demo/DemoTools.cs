using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace McpPaywallDemo;

/// <summary>
/// Simple demo MCP tools that require payment to access
/// </summary>
[McpServerToolType]
public static class DemoTools
{
    [McpServerTool, Description("Get current weather information for any city worldwide.")]
    public static async Task<string> GetWeather(string city,
        [FromServices] IHttpContextAccessor httpContextAccessor)
    {
        var context = httpContextAccessor.HttpContext;
        var userId = context?.Items["McpPaywall.UserId"]?.ToString() ?? "unknown";

        Console.WriteLine($"[DEMO] Paid user {userId} requesting weather for: {city}");

        await Task.Delay(100); // Simulate API call delay
        var random = new Random();
        var temperature = random.Next(-10, 35);
        var conditions = new[] { "sunny", "cloudy", "rainy", "snowy", "windy" };
        var condition = conditions[random.Next(conditions.Length)];

        return JsonSerializer.Serialize(new
        {
            city = city,
            temperature_celsius = temperature,
            condition = condition,
            description = $"Currently {condition} with {temperature}°C",
            timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC"),
            user_id = userId,
            note = "This is simulated weather data for demo purposes."
        }, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool, Description("Generate a secure password with customizable length.")]
    public static async Task<string> GeneratePassword(
        int length = 12,
        [FromServices] IHttpContextAccessor httpContextAccessor = null!)
    {
        var context = httpContextAccessor.HttpContext;
        var userId = context?.Items["McpPaywall.UserId"]?.ToString() ?? "unknown";
        
        Console.WriteLine($"[DEMO] Paid user {userId} generating password");

        await Task.Delay(100);

        var characters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*";
        var random = new Random();
        var password = new string(Enumerable.Repeat(characters, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());

        return JsonSerializer.Serialize(new
        {
            password = password,
            length = length,
            generated_at = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC"),
            user_id = userId
        }, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool, Description("Calculate hash value for input text.")]
    public static async Task<string> CalculateHash(
        string input,
        [FromServices] IHttpContextAccessor httpContextAccessor = null!)
    {
        var context = httpContextAccessor.HttpContext;
        var userId = context?.Items["McpPaywall.UserId"]?.ToString() ?? "unknown";
        
        Console.WriteLine($"[DEMO] Paid user {userId} calculating hash");

        await Task.Delay(100);

        var inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hash = Convert.ToHexString(sha256.ComputeHash(inputBytes)).ToLower();

        return JsonSerializer.Serialize(new
        {
            input_text = input,
            sha256_hash = hash,
            calculated_at = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC"),
            user_id = userId
        }, new JsonSerializerOptions { WriteIndented = true });
    }
}