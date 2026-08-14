# ADR-0002: Use a deterministic-first AI boundary

- Status: Accepted
- Date: 2026-08-14
- Owners: Radar maintainer
- Supersedes: none
- Superseded by: none

## Context

Radar needs semantic clustering, open-ended research, claim comparison, and
expert discovery, but it also needs predictable collection, persistence,
retries, cost, and failure handling. Sending every item through an LLM would
make the system expensive, difficult to reproduce, and dependent on model
availability.

## Decision

Use deterministic mechanisms for ingestion, scheduling, state transitions,
exact deduplication, provenance, budgets, access control, validation, and basic
retrieval. Use AI only behind bounded capability contracts for work requiring
semantic judgement or open-ended synthesis.

Before any AI call, reduce candidates with cheaper identifiers, metadata,
lexical methods, or explicit rules. Record input references, policy/model
version, cost, confidence, and terminal state for derived outputs. Collection
and existing evidence remain usable when AI is unavailable.

## Consequences

- The system is cheaper, more reproducible, and easier to debug.
- AI quality can be evaluated per capability rather than as hidden application
  behaviour.
- Some pipelines require both deterministic candidate generation and semantic
  judgement, increasing interface design work.
- The system must represent partial, declined, unavailable, and failed AI states.
- New AI use requires evidence that it improves a named user outcome over the
  deterministic baseline.

## Alternatives considered

### LLM-first processing of every item

Rejected because cost and latency scale with corpus size, behaviour is difficult
to reproduce, and provider failure would become a system-wide failure.

### No AI in the product

Rejected because semantic Story formation and adaptive Deep Research contain
open-ended work where rigid rules alone would sharply limit value.

### AI as an unversioned internal implementation detail

Rejected because derived claims, clusters, and reports must remain auditable and
comparable across model or policy changes.

## Validation / revisit trigger

Revisit a specific boundary when evaluation data shows an AI method materially
outperforms the deterministic baseline at acceptable cost and failure rate, or
when a currently semantic task becomes reliably deterministic.

