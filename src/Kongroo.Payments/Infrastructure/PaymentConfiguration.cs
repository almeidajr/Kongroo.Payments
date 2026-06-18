using Kongroo.Payments.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kongroo.Payments.Infrastructure;

public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.HasKey(payment => payment.Id);
        builder.Property(payment => payment.Id).HasConversion(id => id.Value, value => PaymentId.From(value));

        builder.Property(payment => payment.OrderId).HasConversion(id => id.Value, value => OrderId.From(value));
        builder.HasIndex(payment => payment.OrderId).IsUnique();

        builder.Property(payment => payment.CustomerId).HasConversion(id => id.Value, value => CustomerId.From(value));

        builder.Property(payment => payment.Email).HasMaxLength(256);
        builder.Property(payment => payment.CustomerName).HasMaxLength(256);
        builder.Property(payment => payment.Status).HasConversion<string>().HasMaxLength(16);
        builder.Property(payment => payment.ProcessedAt).HasPrecision(0);

        builder.ComplexProperty(
            payment => payment.Total,
            moneyBuilder =>
            {
                moneyBuilder.Property(money => money.Amount).HasPrecision(18, 2);
                moneyBuilder
                    .Property(money => money.Currency)
                    .HasConversion(
                        currency => CurrencyMappings.ToCode(currency),
                        code => CurrencyMappings.FromCode(code)
                    )
                    .HasMaxLength(CurrencyMappings.Length)
                    .IsFixedLength();
            }
        );
    }
}
