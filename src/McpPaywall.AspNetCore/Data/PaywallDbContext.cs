using Microsoft.EntityFrameworkCore;
using McpPaywall.AspNetCore.Models;

namespace McpPaywall.AspNetCore.Data;

/// <summary>
/// Entity Framework DbContext for MCP Paywall
/// </summary>
public class PaywallDbContext : DbContext
{
    public PaywallDbContext(DbContextOptions<PaywallDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Payment records table
    /// </summary>
    public DbSet<PaymentRecord> PaymentRecords { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PaymentRecord>(entity =>
        {
            entity.HasKey(e => e.QuoteId);
            entity.Property(e => e.QuoteId).HasMaxLength(100);
            entity.Property(e => e.AccessToken).HasMaxLength(200);
            entity.Property(e => e.UserIdentifier).HasMaxLength(100);
            entity.Property(e => e.ClaimedToken).HasColumnType("TEXT");
            entity.Property(e => e.Unit).HasMaxLength(10);
            entity.Property(e => e.Provider).HasMaxLength(50);
            entity.Property(e => e.Amount).HasColumnType("DECIMAL(18,8)");
            
            // Indexes for performance
            entity.HasIndex(e => e.AccessToken).IsUnique();
            entity.HasIndex(e => e.ExpiresAt);
            entity.HasIndex(e => new { e.IsPaid, e.ExpiresAt });
            entity.HasIndex(e => e.Provider);
            entity.HasIndex(e => e.CreatedAt);
        });

        base.OnModelCreating(modelBuilder);
    }
}