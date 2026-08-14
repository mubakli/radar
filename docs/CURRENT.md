# Current State

- Completed milestone: Milestone 1, working product backbone.
- Working flow: PostgreSQL migration and fixture seed -> ASP.NET Core API -> Next.js story list -> story detail with source provenance.
- Repository shape: `apps/api/Radar.Api`, `apps/web`, `tests/Radar.Api.UnitTests`, `tests/Radar.Api.IntegrationTests`, `infra/compose.yaml`, root `Makefile` and `Radar.slnx`.
- Main commands: `make db-up`, `make migrate`, `make seed`, `make dev`, `make verify`, `make db-down`.
- Permanent decisions: modular monolith (ADR-0003) and the .NET/PostgreSQL/Next.js stack (ADR-0004); EF migrations are the only schema creation path.
- Milestone 2 extension point: introduce the first Collection feature flow that creates `SourceItem` records while preserving `SourceId`, `CanonicalLocator`, `ObservedAt`, and `RawContent`; Story membership remains a separate relationship.
- Known limitations: the only data is a development fixture; there is no authentication, real ingestion, scheduling, ranking, or production deployment.
