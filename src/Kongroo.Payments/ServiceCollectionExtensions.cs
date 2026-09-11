using Kongroo.BuildingBlocks.Application;
using Kongroo.BuildingBlocks.Infrastructure;
using Kongroo.Catalog.Contracts;
using Kongroo.Payments.Application;
using Kongroo.Payments.Contracts;
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

            services.AddMessaging(configuration);
        }

        private void AddMessaging(IConfiguration configuration)
        {
            var transport = configuration.GetValue("Messaging:Transport", MessagingTransport.RabbitMq);

            if (transport == MessagingTransport.RabbitMq)
            {
                services
                    .AddOptions<RabbitMqTransportOptions>()
                    .Bind(configuration.GetRequiredSection("RabbitMq"))
                    .ValidateDataAnnotations()
                    .ValidateOnStart();
            }

            services.AddMassTransit(busRegistration =>
            {
                busRegistration.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("payments"));

                busRegistration.AddEntityFrameworkOutbox<PaymentsDbContext>(outbox =>
                {
                    outbox.UsePostgres();
                    outbox.UseBusOutbox();
                    outbox.QueryDelay = TimeSpan.FromSeconds(1);
                });

                busRegistration.AddConsumer<OrderPlacedIntegrationEventConsumer>();

                if (transport == MessagingTransport.AmazonSqs)
                {
                    var region = configuration.GetValue<string>("Aws:Region");
                    if (string.IsNullOrWhiteSpace(region))
                    {
                        throw new InvalidOperationException(
                            "Configuration value 'Aws:Region' is required when Messaging:Transport is AmazonSqs."
                        );
                    }

                    busRegistration.UsingAmazonSqs(
                        (context, busFactory) =>
                        {
                            // Credentials come from the AWS SDK default chain (AWS_ACCESS_KEY_ID,
                            // AWS_SECRET_ACCESS_KEY, AWS_SESSION_TOKEN) — nothing to configure here.
                            busFactory.Host(region, static _ => { });
                            busFactory.Message<OrderPlacedIntegrationEvent>(static message =>
                                message.SetEntityName(MessagingTopics.OrderPlaced)
                            );
                            busFactory.Message<PaymentProcessedIntegrationEvent>(static message =>
                                message.SetEntityName(MessagingTopics.PaymentProcessed)
                            );
                            busFactory.ConfigureEndpoints(context);
                        }
                    );
                }
                else
                {
                    busRegistration.UsingRabbitMq((context, busFactory) => busFactory.ConfigureEndpoints(context));
                }
            });
        }
    }
}
