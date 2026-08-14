# Runtime Research Agent context

This directory is the static boot context for Radar's runtime Research Agent.
It is intentionally separate from coding-agent and backend documentation.

## Static context loaded for every research case

1. [`RESEARCH_POLICY.md`](RESEARCH_POLICY.md): evidence, source trust, tool use,
   uncertainty, cost, and stopping rules.
2. [`REPORT_CONTRACT.md`](REPORT_CONTRACT.md): required output and auditability.

These files are versioned. Every Research Case records the versions used.

## Context retrieved per case

The application assembles a bounded runtime bundle containing only what the
case needs:

- exact research question and desired depth;
- initiating Story or user-provided material;
- relevant user interests, prior knowledge, and explicit exclusions;
- time window, locale/language, and freshness requirement;
- candidate Source Items and their provenance;
- Discovery Leads selected for investigation, clearly marked as untrusted
  navigation candidates rather than evidence;
- related entities, Topics, People, and previous reports selected by retrieval;
- available tools and access constraints;
- token, money, time, concurrency, and source-count budgets;
- required output mode and any case-specific acceptance checks.

Retrieval results are candidates, not trusted facts. The agent applies the
evidence policy to them.

## Context never sent to the runtime agent

- repository-wide `AGENTS.md` or coding instructions;
- controllers, database schemas, migrations, deployment configuration, logs, or
  unrelated source code;
- credentials, private tokens, hidden system data, or unrestricted user history;
- the full content corpus when a bounded retrieval result is sufficient;
- hidden chain-of-thought from earlier runs.

## Context assembly requirements

- Use stable identifiers and provenance for every supplied item.
- Record why each item was retrieved and the retrieval/policy version.
- Prefer summaries for navigation but provide direct source access for claims.
- Deduplicate supplied material without discarding conflicting versions.
- Enforce hard budgets outside the agent; prompting alone is not a control plane.
- Treat missing, partial, stale, or access-denied sources as explicit states.

Backend architecture is relevant to the coding agent implementing this runtime,
not to the runtime agent itself.
