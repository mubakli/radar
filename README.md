# Radar

Radar is a personal technical-intelligence, discovery, and research platform.
It helps a user understand what materially changed in their fields, decide what
is worth attention, discover high-signal people, and investigate a topic from
primary evidence.

Radar optimizes for **high signal, low noise** and a useful 10–15 minute review,
not an infinite feed.

Sources, repositories, Topics, and People supplied by the user are starting
seeds. Radar is intended to discover additional technical developments, primary
sources, projects, papers, and experts from accessible open-web evidence through
a bounded and auditable discovery loop.

## Start here

- Contributors and coding agents: read [`AGENTS.md`](AGENTS.md), then
  [`docs/README.md`](docs/README.md).
- Product intent and scope: [`docs/PRODUCT.md`](docs/PRODUCT.md).
- Domain language and invariants: [`docs/DOMAIN.md`](docs/DOMAIN.md).
- Discovery lifecycle and trust boundaries: [`docs/DISCOVERY.md`](docs/DISCOVERY.md).
- System boundaries and evolution: [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md).
- Milestone sequence and risk gates: [`docs/ROADMAP.md`](docs/ROADMAP.md).
- Runtime research-agent policy: [`runtime/research-agent/README.md`](runtime/research-agent/README.md).

This repository deliberately avoids a single all-purpose context file. Each
document owns one kind of durable knowledge; temporary work belongs in an issue
or a short-lived feature specification.

## Milestone 1 quickstart

Requirements: .NET SDK 10, Node.js 22 with npm, Docker, and Docker Compose.

```sh
make db-up
dotnet tool install --global dotnet-ef --version 10.0.0
make migrate
make seed
make dev
```

Open `http://localhost:3000` for the web app or
`http://localhost:5000/api/stories` for the story list. `RADAR_API_URL` in
`apps/web/.env.local` can change the API address;
the default is `http://localhost:5000`.

Run `make verify` for backend restore/build/unit and PostgreSQL integration
tests, frontend dependency verification/lint/typecheck/Vitest, and the
Playwright smoke test. Stop local PostgreSQL with `make db-down`.

## Repository map

- `apps/api/Radar.Api`: ASP.NET Core API, domain persistence, migrations, and fixture seed.
- `apps/web`: Next.js App Router UI.
- `tests`: unit and real-PostgreSQL API integration tests.
- `infra/compose.yaml`: local PostgreSQL 18.
- `docs/CURRENT.md`: concise milestone handoff.
