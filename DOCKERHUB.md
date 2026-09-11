# Kongroo Payments

Payment processing microservice for the Kongroo platform. Built with ASP.NET Core
and PostgreSQL, following Domain-Driven Design with a transactional outbox that
reliably publishes integration events (e.g. `PaymentProcessedIntegrationEvent`) via
MassTransit. The transport is selected by `Messaging__Transport`: RabbitMQ by
default, or Amazon SQS/SNS in Kubernetes.

It consumes `OrderPlacedIntegrationEvent` (topic `kongroo-order-placed`, published
by Kongroo Catalog), processes the payment against a configurable approval limit,
and publishes `PaymentProcessedIntegrationEvent` (topic `kongroo-payment-processed`)
with status `Approved` or `Rejected`.

## Tags

- `latest` — most recent stable release
- `x.y.z`  — specific version (e.g. `0.1.0`)
- `dev`    — in-progress development build

## Quick start

The container listens on port **8080** and requires a PostgreSQL database and a
messaging broker (RabbitMQ or Amazon SQS/SNS, per `Messaging__Transport`).

```bash
docker run -p 8080:8080 \
  -e ConnectionStrings__Database="Host=postgres;Database=kongroo_payments;Username=kongroo;Password=development" \
  -e RabbitMq__Host="rabbitmq" \
  -e RabbitMq__User="kongroo" \
  -e RabbitMq__Pass="development" \
  -e Payments__ApprovalLimit="1000.00" \
  -e Jwt__Issuer="Kongroo.Identity.Api" \
  -e Jwt__Audience="Kongroo.Identity.Api" \
  -e Jwt__SigningKey="<a-secret-key-at-least-32-characters-long>" \
  josealmeidajr/kongroo-payments:latest
```

## Endpoints

| Method & path | Description |
|---|---|
| `GET /` | Get the authenticated caller's payments (Admins may pass `?customerId=` to view another customer's payments) |
| `GET /{orderId}` | Get the payment for a single order owned by the authenticated caller |
| `GET /health` | Health check |
| `GET /metrics` | Prometheus metrics (unauthenticated, scraped in-cluster) |

Payments are created by consuming `OrderPlacedIntegrationEvent` from the broker, not
through an HTTP endpoint.

## Configuration

Configured via environment variables. The double underscore (`__`) maps to
nested configuration sections.

| Variable | Description |
|---|---|
| `ConnectionStrings__Database` | PostgreSQL connection string |
| `RabbitMq__Host` | RabbitMQ broker hostname |
| `RabbitMq__User` | RabbitMQ username |
| `RabbitMq__Pass` | RabbitMQ password |
| `Payments__ApprovalLimit` | Payments at or below this amount are approved; above are rejected |
| `Jwt__Issuer` | JWT issuer (must match the Identity service) |
| `Jwt__Audience` | JWT audience (must match the Identity service) |
| `Jwt__SigningKey` | JWT signing key (min 32 chars, must match the Identity service) |
| `Jwt__AccessTokenLifetimeMinutes` | Access token lifetime in minutes |
| `Messaging__Transport` | `RabbitMq` (default, used by Docker Compose and the tests) or `AmazonSqs` (Kubernetes) |
| `Aws__Region` | AWS region for SQS/SNS, required when `Messaging__Transport=AmazonSqs` (e.g. `us-east-1`) |
| `AWS_ACCESS_KEY_ID` / `AWS_SECRET_ACCESS_KEY` / `AWS_SESSION_TOKEN` | AWS SDK default credential chain, supplied by the `aws-credentials` Secret |

This service validates tokens it did not issue; `Jwt__Issuer`, `Jwt__Audience`,
and `Jwt__SigningKey` must match the Kongroo Identity service exactly.

## Requirements

- A reachable PostgreSQL database
- A reachable messaging broker (RabbitMQ or Amazon SQS/SNS, per `Messaging__Transport`)

## Source

Part of the Kongroo platform.
