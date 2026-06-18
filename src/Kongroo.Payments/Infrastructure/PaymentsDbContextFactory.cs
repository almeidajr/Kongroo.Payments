using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Kongroo.Payments.Infrastructure;

/// <summary>Design-time factory used by EF migrations tooling.</summary>
internal sealed class PaymentsDbContextFactory : IDesignTimeDbContextFactory<PaymentsDbContext>
{
    public PaymentsDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<PaymentsDbContext>()
            .UseNpgsql("Host=localhost;Database=payments")
            .UseSnakeCaseNamingConvention()
            .Options;

        return new PaymentsDbContext(options);
    }
}
