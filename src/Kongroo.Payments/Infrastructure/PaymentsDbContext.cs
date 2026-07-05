using Kongroo.BuildingBlocks.Infrastructure;
using Kongroo.Payments.Domain;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Kongroo.Payments.Infrastructure;

public sealed class PaymentsDbContext(DbContextOptions<PaymentsDbContext> options)
    : RelationalDbContext<PaymentsDbContext>(options),
        IRelationalDbContext
{
    public static string Schema => "payments";

    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new PaymentConfiguration());
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
    }
}
