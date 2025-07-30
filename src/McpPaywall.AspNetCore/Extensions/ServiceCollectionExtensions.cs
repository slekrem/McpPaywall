using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using McpPaywall.AspNetCore.Data;
using McpPaywall.AspNetCore.Models;
using McpPaywall.AspNetCore.Services;
using McpPaywall.AspNetCore.Middleware;

namespace McpPaywall.AspNetCore.Extensions;

/// <summary>
/// Extension methods for setting up MCP Paywall services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add MCP Paywall services with default Cashu provider
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configureOptions">Configuration action</param>
    /// <returns>Service collection</returns>
    public static IServiceCollection AddMcpPaywall(
        this IServiceCollection services,
        Action<McpPaywallOptions> configureOptions)
    {
        return services.AddMcpPaywall<CashuPaymentProvider>(configureOptions);
    }

    /// <summary>
    /// Add MCP Paywall services with custom payment provider
    /// </summary>
    /// <typeparam name="TPaymentProvider">Payment provider implementation</typeparam>
    /// <param name="services">Service collection</param>
    /// <param name="configureOptions">Configuration action</param>
    /// <returns>Service collection</returns>
    public static IServiceCollection AddMcpPaywall<TPaymentProvider>(
        this IServiceCollection services,
        Action<McpPaywallOptions> configureOptions)
        where TPaymentProvider : class, IPaymentProvider
    {
        // Configure options
        services.Configure(configureOptions);

        // Add Entity Framework
        services.AddDbContext<PaywallDbContext>((serviceProvider, options) =>
        {
            var paywallOptions = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<McpPaywallOptions>>().Value;
            
            // Check if MySQL should be used (connection string contains "Server=")
            if (paywallOptions.ConnectionString.Contains("Server=", StringComparison.OrdinalIgnoreCase))
            {
                options.UseMySql(paywallOptions.ConnectionString, new MariaDbServerVersion(new Version(10, 5, 0)));
            }
            else
            {
                options.UseSqlite(paywallOptions.ConnectionString);
            }
        });

        // Add services
        services.AddScoped<IPaymentProvider, TPaymentProvider>();
        services.AddScoped<IPaywallService, PaywallService>();

        // Add controllers
        services.AddControllers();

        return services;
    }

    /// <summary>
    /// Add MCP Paywall services with Cashu provider and specific configuration
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configurePaywall">Paywall configuration</param>
    /// <param name="configureCashu">Cashu configuration</param>
    /// <returns>Service collection</returns>
    public static IServiceCollection AddMcpPaywallWithCashu(
        this IServiceCollection services,
        Action<McpPaywallOptions> configurePaywall,
        Action<CashuPaymentOptions> configureCashu)
    {
        services.Configure(configureCashu);
        return services.AddMcpPaywall(configurePaywall);
    }

    /// <summary>
    /// Add MCP Paywall services with configuration from appsettings.json
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Configuration</param>
    /// <param name="sectionName">Configuration section name (default: "McpPaywall")</param>
    /// <returns>Service collection</returns>
    public static IServiceCollection AddMcpPaywall(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = "McpPaywall")
    {
        services.Configure<McpPaywallOptions>(configuration.GetSection(sectionName));
        services.Configure<CashuPaymentOptions>(configuration.GetSection($"{sectionName}:Cashu"));

        return services.AddMcpPaywall<CashuPaymentProvider>(_ => { });
    }
}

/// <summary>
/// Extension methods for setting up MCP Paywall middleware
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Use MCP Paywall middleware and endpoints
    /// </summary>
    /// <param name="app">Application builder</param>
    /// <param name="basePath">Base path for paywall endpoints (optional)</param>
    /// <returns>Application builder</returns>
    public static IApplicationBuilder UseMcpPaywall(this IApplicationBuilder app, string? basePath = null)
    {
        // Ensure database is created
        using (var scope = app.ApplicationServices.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<PaywallDbContext>();
            var options = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<McpPaywallOptions>>().Value;
            
            if (options.EnsureDatabaseCreated)
            {
                // For MySQL/MariaDB, ensure database exists first
                if (options.ConnectionString.Contains("Server=", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        // Try to create database if it doesn't exist
                        dbContext.Database.EnsureCreated();
                    }
                    catch (Exception ex)
                    {
                        // If database creation fails, try to create the database first
                        var connectionString = options.ConnectionString;
                        var builder = new MySqlConnector.MySqlConnectionStringBuilder(connectionString);
                        var databaseName = builder.Database;
                        builder.Database = "";
                        
                        using var connection = new MySqlConnector.MySqlConnection(builder.ConnectionString);
                        connection.Open();
                        using var command = connection.CreateCommand();
                        command.CommandText = $"CREATE DATABASE IF NOT EXISTS `{databaseName}` CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;";
                        command.ExecuteNonQuery();
                        
                        // Now ensure tables are created
                        dbContext.Database.EnsureCreated();
                    }
                }
                else
                {
                    dbContext.Database.EnsureCreated();
                }
            }
        }

        // Add authentication middleware
        app.UseMiddleware<McpAuthenticationMiddleware>();

        // Map controllers with optional base path
        if (!string.IsNullOrEmpty(basePath))
        {
            app.UsePathBase(basePath);
        }

        return app;
    }

    /// <summary>
    /// Use MCP Paywall with default static files serving
    /// </summary>
    /// <param name="app">Application builder</param>
    /// <param name="basePath">Base path</param>
    /// <param name="staticFilesPath">Static files path (optional)</param>
    /// <returns>Application builder</returns>
    public static IApplicationBuilder UseMcpPaywallWithStaticFiles(
        this IApplicationBuilder app, 
        string basePath = "/paywall",
        string? staticFilesPath = null)
    {
        app.UsePathBase(basePath);
        
        if (!string.IsNullOrEmpty(staticFilesPath))
        {
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(staticFilesPath),
                RequestPath = ""
            });
        }
        else
        {
            app.UseDefaultFiles();
            app.UseStaticFiles();
        }

        return app.UseMcpPaywall();
    }
}