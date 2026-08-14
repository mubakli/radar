.DEFAULT_GOAL := help
DB_PORT ?= 5432

help:
	@printf '%s\n' 'make db-up | db-down | migrate | seed | dev | verify'

db-up:
	DB_PORT=$(DB_PORT) docker compose -f infra/compose.yaml up -d --wait postgres

db-down:
	docker compose -f infra/compose.yaml down

migrate:
	ConnectionStrings__Radar='Host=localhost;Port=$(DB_PORT);Database=radar;Username=radar;Password=radar' dotnet ef database update --project apps/api/Radar.Api --startup-project apps/api/Radar.Api

seed:
	ConnectionStrings__Radar='Host=localhost;Port=$(DB_PORT);Database=radar;Username=radar;Password=radar' dotnet run --project apps/api/Radar.Api -- seed

dev:
	trap 'kill 0' INT TERM EXIT; ConnectionStrings__Radar='Host=localhost;Port=$(DB_PORT);Database=radar;Username=radar;Password=radar' dotnet run --project apps/api/Radar.Api --urls http://localhost:5000 & (cd apps/web && npm run dev) & wait

verify: db-up migrate seed
	dotnet restore Radar.slnx
	dotnet build Radar.slnx --no-restore
	dotnet test tests/Radar.Api.UnitTests --no-build
	dotnet test tests/Radar.Api.IntegrationTests --no-build
	cd apps/web && npm ci
	cd apps/web && npm run lint
	cd apps/web && npm run typecheck
	cd apps/web && npm test
	cd apps/web && npx playwright install chromium
	set -e; ConnectionStrings__Radar='Host=localhost;Port=$(DB_PORT);Database=radar;Username=radar;Password=radar' dotnet run --project apps/api/Radar.Api --urls http://localhost:5000 >/tmp/radar-api.log 2>&1 & api_pid=$$!; trap 'kill $$api_pid' EXIT INT TERM; sleep 8; (cd apps/web && npm run e2e)
