using Kongroo.BuildingBlocks.Application;
using Kongroo.BuildingBlocks.Infrastructure;
using Kongroo.Payments.Application;
using Kongroo.Payments.Domain;
using Kongroo.Payments.Infrastructure;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Kongroo.Payments;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddPaymentsModule(IConfiguration configuration)
        {
            services.AddValidation();
            services.AddApplication();
            services.AddInfrastructure(configuration);

            return services;
        }

        private void AddApplication()
        {
            services.AddScoped<ProcessPaymentCommandHandler>();
            services.AddScoped<GetPaymentsQueryHandler>();
            services.AddScoped<GetPaymentQueryHandler>();

            services.AddDomainEventHandler<PaymentProcessedDomainEventHandler>();
        }

        private void AddInfrastructure(IConfiguration configuration)
        {
            services.AddSingleton(TimeProvider.System);

            services.AddRelationalDbContext<PaymentsDbContext>(contextOptions =>
                contextOptions.UseNpgsql(
                    configuration.GetConnectionString("Database"),
                    postgresOptions => postgresOptions.MigrationsHistoryTable("migrations", PaymentsDbContext.Schema)
                )
            );
            services.AddDbInitializer<PaymentsDbContext>();

            services
                .AddOptions<PaymentApprovalOptions>()
                .Bind(configuration.GetRequiredSection(PaymentApprovalOptions.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();
            services.AddSingleton<IPaymentApprovalPolicy, ThresholdApprovalPolicy>();

            services
                .AddOptions<RabbitMqTransportOptions>()
                .Bind(configuration.GetRequiredSection("RabbitMq"))
                .ValidateDataAnnotations()
                .ValidateOnStart();
            services.AddMassTransit(busRegistration =>
            {
                busRegistration.SetKebabCaseEndpointNameFormatter();

                busRegistration.AddEntityFrameworkOutbox<PaymentsDbContext>(outbox =>
                {
                    outbox.UsePostgres();
                    outbox.UseBusOutbox();
                    outbox.QueryDelay = TimeSpan.FromSeconds(1);
                });

                busRegistration.AddConsumer<OrderPlacedIntegrationEventConsumer>();
                busRegistration.UsingRabbitMq((context, busFactory) => busFactory.ConfigureEndpoints(context));
            });
        }
    }
}
