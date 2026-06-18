using Kongroo.BuildingBlocks.Infrastructure;
using Kongroo.Payments.Domain;
using Microsoft.EntityFrameworkCore;

namespace Kongroo.Payments.Infrastructure;

public sealed class PaymentsDbContext(DbContextOptions<PaymentsDbContext> options)
    : OutboxDbContext<PaymentsDbContext>(options),
        IRelationalDbContext
{
    public static string Schema => "payments";

    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new PaymentConfiguration());
    }
}
