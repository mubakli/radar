# Architecture Decision Records

ADRs preserve the reason for costly, constraining, or cross-boundary decisions.
They are not a diary of every implementation choice.

## Lifecycle

Statuses are `Proposed`, `Accepted`, `Rejected`, `Deprecated`, or `Superseded by
ADR-NNNN`. An accepted ADR is immutable except for corrections and status links.
Change a decision by adding a new ADR that supersedes the old one.

## Naming

Use `NNNN-short-kebab-title.md` with the next unused four-digit number. Never
renumber accepted records.

## Minimal template

```markdown
# ADR-NNNN: Decision title

- Status: Proposed
- Date: YYYY-MM-DD
- Owners: names or team
- Supersedes: none
- Superseded by: none

## Context
What forces, constraints, and evidence make a decision necessary?

## Decision
What is being decided? State the boundary precisely.

## Consequences
What becomes easier, harder, more expensive, or constrained?

## Alternatives considered
Which credible options were rejected, and why?

## Validation / revisit trigger
What evidence would show this decision is wrong or should be revisited?
```

Do not put rollout checklists, framework tutorials, meeting transcripts, or
mutable implementation inventories in an ADR.

## Index

- [ADR-0001: Separate durable development context from runtime research context](0001-separate-development-and-runtime-context.md)
- [ADR-0002: Use a deterministic-first AI boundary](0002-deterministic-first-ai-boundary.md)
- [ADR-0003: Start as a modular monolith](0003-start-as-a-modular-monolith.md)

