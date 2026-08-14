# ADR-0004: Initial technology stack

- Status: Accepted
- Date: 2026-08-14
- Owners: Radar maintainer
- Supersedes: none
- Superseded by: none

## Context

Milestone 1 needs a small, reproducible vertical slice without prematurely
introducing distributed infrastructure or an application-specific BFF.

## Decision

Use .NET 10 / ASP.NET Core Minimal API with EF Core 10 and Npgsql against
PostgreSQL 18. Keep the backend as one deployable modular-monolith application,
with feature boundaries represented in code. Use Next.js App Router,
TypeScript, and Tailwind CSS for the web UI. Use Docker Compose for local
PostgreSQL, EF migrations for schema changes, xUnit/Testcontainers for backend
tests, Vitest for fast web tests, and Playwright for the critical browser flow.

The Next.js application consumes the ASP.NET Core API and never connects to
PostgreSQL. No frontend BFF or generated client is needed for this slice.

## Alternatives considered

- A separate frontend BFF was rejected because it would duplicate the API
  boundary without a current need.
- Microservices, a message broker, and a monorepo build framework were rejected
  because the initial workload and ownership boundaries do not justify them.
- An in-memory database was rejected for integration tests because it cannot
  validate PostgreSQL migrations or relational behaviour.

## Consequences

Local setup remains explicit and inexpensive, and the API remains the source of
truth for domain and persistence rules. The initial UI is intentionally thin;
production deployment, authentication, background execution, and real source
connectors remain future work.
