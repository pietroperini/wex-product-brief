# WEX Product Brief API

This is a small ASP.NET Core API built as part of a technical exercise.  
It exposes endpoints to create and retrieve product briefs, with a focus on idempotency handling and keeping the design simple and testable.

---

## What this project is

The idea here was to implement a backend service with:

- basic clean separation of concerns
- idempotent request handling
- simple persistence using SQLite
- no external dependencies required to run it

It’s not meant to be a full production system, but it follows patterns that would scale into one.

---

## Tech stack

- .NET 8 (ASP.NET Core Web API)
- Entity Framework Core
- SQLite
- System.Text.Json

---

## How to run

Clone the repo:

```bash
git clone https://github.com/pietroperini/wex-product-brief.git
cd wex-product-brief
```

Run it:

```bash
dotnet restore
dotnet build
dotnet run --project src/WexProductBrief.Api
```

The API should start on the default localhost ports.

---

## API

### Create purchase transaction

```http
POST /api/PurchaseTransaction
```

Example payload:

```json
{
  "description": "Test-Transaction",
  "transactionDate": "2026-05-14T07:22:56.343Z",
  "purchaseAmount": 1000
}
```

Response:

```json
{
  "id": "0ae449b4-82c6-4aac-b0e5-afff8a8bc027",
  "description": "Test-Transaction",
  "transactionDate": "2026-05-14T07:22:56.343Z",
  "purchaseAmount": 100
}
```

```http
Get /api/PurchaseTransaction/{id}/currency/{countryCurrencyDesc}
```

Response:

```json
{
  "id": "65f63f17-8fd4-4c1d-83b8-35572f4836e7",
  "description": "Test2",
  "transactionDate": "2026-05-14T07:27:06.652",
  "originalAmount": 100.22,
  "originalCurrency": "US-Dollar",
  "convertedAmount": 526.56,
  "targetCurrency": "Brazil-Real",
  "exchangeRate": 5.254
}
```


---

## Idempotency

The API supports idempotent requests.

If a request comes with the same idempotency key + same payload, it won’t be processed twice. Instead, the previous response is returned.

The idea is to avoid duplicate writes when clients retry requests.

---

## Database

SQLite is used just to keep things simple.

- DB is created automatically on startup
- no setup required
- data is persisted locally in a file

---

## Structure

The project is split roughly like this:

```text
Controllers → Application services → Data access (EF Core)
```

Controllers are kept thin, most of the logic is in services/facades.

---

## Notes

Some trade-offs made intentionally:

- SQLite instead of a real database (for simplicity)
- no distributed cache (kept in-memory / DB for idempotency)
- no authentication layer
- minimal validation logic

---

## If I had more time

- add proper test coverage (unit + integration)
- introduce Redis for idempotency storage
- improve validation layer
- add structured logging
- containerize with Docker

---

## Final note

The goal was to keep it simple but still show how I would structure a service like this in a real backend system.