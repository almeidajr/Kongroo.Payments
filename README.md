# Kongroo.Payments

Payment processing microservice for FIAP Cloud Games (Phase 2 scaffold — domain logic TBD).

## Environment Variables

| Variable | Source | Description |
|---|---|---|
| `ConnectionStrings__Database` | Secret | PostgreSQL connection string |

## Running Locally

```bash
dotnet run --project src/Kongroo.Payments.Api
```

## Docker

```bash
dotnet restore
docker build -t kongroo-payments .
```
