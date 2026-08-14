# ADR-0003: Start as a modular monolith

- Status: Proposed
- Date: 2026-08-14
- Owners: Radar maintainer
- Supersedes: none
- Superseded by: none

## Context

Radar contains collection, discovery, ranking, personalisation, and research
responsibilities, but its initial workload, team size, and scaling bottlenecks
are unknown. Premature service separation would add deployment, messaging,
consistency, tracing, and local-development costs before the product loop is
validated.

## Decision

Begin with a modular monolith, background job execution, and one primary
relational database. Enforce logical module ownership and explicit application
contracts in code. Do not let shared deployment justify arbitrary cross-module
table access.

This decision remains proposed until the implementation stack and first feature
slice are selected.

## Consequences

- Transactions, local development, deployment, and end-to-end testing stay
  comparatively simple.
- Module boundaries can be learned before turning them into network boundaries.
- Discipline is required to prevent the monolith from becoming an unstructured
  shared-data application.
- A later extraction will require migrations and contract hardening, but only
  where measured isolation or scale makes that cost worthwhile.

## Alternatives considered

### Microservices from the beginning

Rejected for the initial stage because there is no measured scaling or team
ownership need that offsets operational complexity.

### Single unstructured application layer

Rejected because Radar's responsibilities have different trust, cost, and data
ownership boundaries even when deployed together.

### Event-sourced system as the default model

Rejected as a system-wide constraint. Selected append-only observations and
audit events are useful, but full event sourcing has not been justified.

## Validation / revisit trigger

Accept after choosing the implementation stack and confirming the initial slice
fits this shape. Revisit module extraction only when measured throughput,
failure isolation, security, deployment cadence, or independent ownership
cannot be handled reasonably within the monolith.

