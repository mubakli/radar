# ADR-0001: Separate durable development context from runtime research context

- Status: Accepted
- Date: 2026-08-14
- Owners: Radar maintainer
- Supersedes: none
- Superseded by: none

## Context

Coding agents need product intent, domain invariants, architecture boundaries,
decision rationale, and the scope of the current change. Radar's runtime
Research Agent needs evidence policy, source evaluation, tool/budget rules,
uncertainty handling, and a report contract.

Combining these concerns in a large context file would increase token use,
expose irrelevant implementation detail to the runtime agent, create duplicate
truths, and make stale instructions harder to detect.

## Decision

Use three distinct context lifecycles:

1. **Durable development context** in root `AGENTS.md` and `docs/`, routed by
   responsibility and loaded selectively.
2. **Temporary delivery context** in `docs/features/`, created only for
   non-trivial in-flight changes and deleted after durable truths are reconciled.
3. **Runtime research context** in `runtime/research-agent/`, containing static
   policy and output contracts. Per-case facts, user constraints, sources,
   budgets, and tool availability are retrieved into a runtime bundle.

The runtime Research Agent does not receive repository implementation context.
Coding agents read runtime policy only when implementing or evaluating research
behaviour.

## Consequences

- Agent context is smaller, task-specific, and easier to audit.
- Runtime research policy can be versioned and evaluated independently from
  backend architecture.
- Contributors must route information to the correct owner instead of placing
  everything in one file.
- Context assembly requires an explicit task classifier and runtime bundle.
- Cross-links and contradiction checks become important maintenance work.

## Alternatives considered

### One repository-wide `CONTEXT.md`

Rejected because it mixes lifecycles, grows monotonically, and encourages every
agent to load irrelevant material.

### Derive all context from source code

Rejected because code rarely preserves product intent, rejected alternatives,
evidence policy, or the reason for structural decisions.

### Store all knowledge in an external vector database

Rejected as the source of truth. Retrieval may later help select context, but it
does not solve authority, duplication, versioning, or contradiction.

## Validation / revisit trigger

Revisit if agents routinely miss required context despite following the routing
map, if the same truth must be maintained in multiple owners, or if runtime
policy cannot be independently versioned and evaluated.

