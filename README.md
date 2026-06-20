# Kongroo.Payments

Payment processing microservice for FIAP Cloud Games. Consumes `OrderPlacedIntegrationEvent`
from `Kongroo.Catalog` and publishes `PaymentProcessedIntegrationEvent` (`Approved` / `Rejected`)
via RabbitMQ.

## Environment variables

| Variable | Purpose | Example |
|---|---|---|
| `ASPNETCORE_ENVIRONMENT` | Runtime environment | `Production` |
| `ConnectionStrings__Database` | PostgreSQL connection string | `Host=postgres;Database=kongroo_payments;Username=kongroo;Password=...` |
| `RabbitMq__Host` | RabbitMQ host (K8s Service name) | `rabbitmq` |
| `RabbitMq__User` | RabbitMQ username | `kongroo` |
| `RabbitMq__Pass` | RabbitMQ password | `...` |
| `Payments__ApprovalLimit` | Payments at or below this amount are approved; above are rejected | `1000.00` |
| `Jwt__Issuer` / `Jwt__Audience` / `Jwt__SigningKey` | JWT validation (issued by Identity) | — |

The service consumes `OrderPlacedIntegrationEvent` and publishes `PaymentProcessedIntegrationEvent`
(`Approved` / `Rejected`) via RabbitMQ. Migrations are applied automatically on startup in the
Development environment only.

## Running Locally

```bash
dotnet run --project src/Kongroo.Payments.Api
```

## Docker

```bash
dotnet restore
docker build -t kongroo-payments .
```
