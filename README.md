# <img alt="Kongroo" src="./logo.png" width="40"/> Kongroo.Payments

Payment processing microservice for FIAP Cloud Games. Consumes `OrderPlacedIntegrationEvent`
from `Kongroo.Catalog` and publishes `PaymentProcessedIntegrationEvent` (`Approved` / `Rejected`)
over MassTransit — RabbitMQ by default (Docker Compose, tests) or Amazon SQS/SNS in Kubernetes, selected
by `Messaging__Transport`.

## Environment variables

| Variable                                            | Purpose                                                           | Example                                                                 |
| --------------------------------------------------- | ----------------------------------------------------------------- | ----------------------------------------------------------------------- |
| `ASPNETCORE_ENVIRONMENT`                            | Runtime environment                                               | `Production`                                                            |
| `ConnectionStrings__Database`                       | PostgreSQL connection string                                      | `Host=postgres;Database=kongroo_payments;Username=kongroo;Password=...` |
| `RabbitMq__Host`                                    | RabbitMQ host (K8s Service name)                                  | `rabbitmq`                                                              |
| `RabbitMq__User`                                    | RabbitMQ username                                                 | `kongroo`                                                               |
| `RabbitMq__Pass`                                    | RabbitMQ password                                                 | `...`                                                                   |
| `Payments__ApprovalLimit`                           | Payments at or below this amount are approved; above are rejected | `1000.00`                                                               |
| `Jwt__Issuer` / `Jwt__Audience` / `Jwt__SigningKey` | JWT validation (issued by Identity)                               | —                                                                       |
| `Messaging__Transport`                              | `RabbitMq` (default, compose/tests) or `AmazonSqs` (k8s)       | `AmazonSqs`  |
| `Aws__Region`                                       | AWS region for SQS/SNS when `AmazonSqs`                          | `us-east-1`  |
| `AWS_ACCESS_KEY_ID` / `AWS_SECRET_ACCESS_KEY` / `AWS_SESSION_TOKEN` | AWS SDK default credential chain (Secret `aws-credentials`) | Learner Lab session values |

The service consumes `OrderPlacedIntegrationEvent` and publishes `PaymentProcessedIntegrationEvent`
(`Approved` / `Rejected`) over the configured transport. The Kubernetes manifests default to
`Messaging__Transport=AmazonSqs`, so the pod only becomes Ready once the `aws-credentials` Secret (shipped and
refreshed by Kongroo.Orchestration) holds live Learner Lab credentials; set `Messaging__Transport=RabbitMq`
in the ConfigMap to run the cluster against RabbitMQ instead. Migrations are applied automatically on startup in all environments (warning logged when not Development).

The consumed order event includes customer and line-level game details (`CustomerId`,
`Lines[].GameId`, `Lines[].UnitPrice`) for challenge traceability. Payments currently
uses the order total for approval and persists the payment by order and customer id.

## Messaging topics (Amazon SQS transport)

Consumes SNS topic `kongroo-order-placed` (published by Catalog) and publishes `kongroo-payment-processed`
(consumed by Catalog and the Notifications Lambda). Names are fixed in `MessagingTopics`.

## Observability

`GET /metrics` exposes OpenTelemetry metrics in Prometheus format (ASP.NET Core request duration by
route and status, HttpClient, .NET runtime, MassTransit publish/consume counters).

## Running Locally

```bash
dotnet run --project src/Kongroo.Payments
```

## Docker

```bash
dotnet restore
docker build -t kongroo-payments .
```
