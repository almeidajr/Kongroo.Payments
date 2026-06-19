using Kongroo.BuildingBlocks;
using Kongroo.BuildingBlocks.Application;
using Kongroo.Payments.Application;
using Kongroo.Payments.Domain;
using Kongroo.Payments.Infrastructure;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Kongroo.Payments;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddPaymentsModule(IConfiguration configuration)
        {
            services.AddApplication();
            services.AddInfrastructure(configuration);

            return services;
        }

        private void AddApplication()
        {
            services.AddScoped<ProcessPaymentCommandHandler>();
            services.AddScoped<GetPaymentsQueryHandler>();
            services.AddScoped<GetPaymentQueryHandler>();
            services.AddScoped<IDomainEventHandler, PaymentProcessedDomainEventHandler>();
        }

        private void AddInfrastructure(IConfiguration configuration)
        {
            services.AddOutboxDbContext<PaymentsDbContext>(configuration);

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
                busRegistration.AddConsumer<OrderPlacedIntegrationEventConsumer>();
                busRegistration.UsingRabbitMq((context, busFactory) => busFactory.ConfigureEndpoints(context));
            });
        }
    }
}
