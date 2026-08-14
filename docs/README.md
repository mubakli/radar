# Documentation map

This directory is Radar's durable project memory. It contains intent,
vocabulary, invariants, system boundaries, and accepted decisions—not a prose
mirror of the codebase.

## Ownership

| Question | Single owner | Must not contain |
| --- | --- | --- |
| Why does Radar exist, for whom, and what is intentionally out of scope? | [`PRODUCT.md`](PRODUCT.md) | Data models, endpoints, framework choices, task status |
| What do domain terms mean and which truths must always hold? | [`DOMAIN.md`](DOMAIN.md) | ORM schemas, UI copy, speculative implementation |
| How does Radar explore beyond configured seeds and control candidate/source quality? | [`DISCOVERY.md`](DISCOVERY.md) | Provider SDK details, mutable thresholds, search result dumps |
| What are the system boundaries, data flow, AI boundary, and evolution path? | [`ARCHITECTURE.md`](ARCHITECTURE.md) | File/class inventories, sprint plans, vendor tutorials |
| In what dependency order should capabilities be delivered and where are the risk gates? | [`ROADMAP.md`](ROADMAP.md) | Live task status, implementation diary, deadlines |
| Why was a costly or constraining choice made? | [`architecture/decisions/`](architecture/decisions/README.md) | Meeting notes, minor implementation choices, mutable status reports |
| What change are we currently trying to deliver? | [`features/`](features/README.md) | Permanent history, restated global architecture, completed task archives |
| How must the runtime Research Agent reason and report? | [`../runtime/research-agent/`](../runtime/research-agent/README.md) | Backend implementation, coding-agent instructions, user secrets |

`README.md` and `AGENTS.md` at repository root are entry points. They may link to
owned truths but must not duplicate them.

## Context routing for coding tasks

Always read root `AGENTS.md` and this file. Then add only the following:

| Task type | Additional context |
| --- | --- |
| Product behaviour, prioritisation, or scope | `PRODUCT.md`, relevant section of `DOMAIN.md`, active feature spec |
| Domain model, persistence, clustering, ranking | `DOMAIN.md`, `ARCHITECTURE.md`, relevant ADRs, active feature spec |
| Source adapter or ingestion work | `DOMAIN.md` provenance invariants, `ARCHITECTURE.md` collection flow, active feature spec |
| Open-web discovery, candidate generation, source probation, or expansion | `DISCOVERY.md`, relevant `DOMAIN.md` invariants, `ARCHITECTURE.md`, active feature spec |
| Milestone selection, phase gate, or next-slice planning | `ROADMAP.md`, `PRODUCT.md`, then only the documents routed by the selected milestone |
| System boundary, dependency, deployment, or major technology choice | `ARCHITECTURE.md`, relevant ADRs; create an ADR if the acceptance test below is met |
| Research-agent implementation or evaluation | `ARCHITECTURE.md`, all files under `runtime/research-agent/`, active feature spec |
| Copy or simple local UI change | `PRODUCT.md` only if product intent is material; otherwise local code/tests |
| Bug fix with clear local behaviour | Local code/tests and the owning invariant only; no broad documentation load |

Search before reading: locate the relevant term, feature slug, module, or ADR,
then open the smallest authoritative set. A new subdomain may add a local
`AGENTS.md` only when it has genuinely local invariants. Local files may add
constraints; they must link to rather than restate global policy.

## Where information belongs

| Information | Correct home |
| --- | --- |
| Durable user problem or product principle | `PRODUCT.md` |
| Stable domain definition or invariant | `DOMAIN.md` |
| Discovery purpose, candidate/source lifecycle, exploration policy, or promotion boundary | `DISCOVERY.md` |
| Current structural boundary or data flow | `ARCHITECTURE.md` |
| Costly decision and rejected alternatives | ADR |
| Proposed acceptance criteria and migration steps | Active feature spec |
| Work assignment, discussion, checklist, or deadline | Issue / project tracker |
| Exact implemented behaviour | Code and automated tests |
| Operational procedure that a human must execute | Add an operations runbook only when the procedure exists |
| Experiment result or ranking evaluation | Versioned evaluation fixture/report, not general documentation |

## ADR acceptance test

Create an ADR only if at least one is true:

- reversing the decision would require a migration or broad rewrite;
- it changes a trust, privacy, evidence, cost, or availability boundary;
- it selects a major storage/processing model or external dependency;
- several plausible options exist and future contributors will reasonably ask
  why one was rejected.

Otherwise record the choice in code, tests, the feature spec, or the pull
request.

## Maintenance and contradiction rules

1. **One truth, one owner.** Link instead of copying.
2. **Update in the same change.** A change to a durable truth is incomplete
   until its owning document changes.
3. **Do not guess through conflicts.** A conflict between code, tests, a feature
   spec, and durable documentation blocks the change until intent is reconciled.
4. **Authority is question-specific, not hierarchical.** Product intent comes
   from `PRODUCT.md`; terminology from `DOMAIN.md`; structure from
   `ARCHITECTURE.md`; decision rationale from accepted ADRs; the intended
   in-flight delta from the active feature spec; implementation from code/tests.
5. **Delete stale temporary context.** Completed feature specs are reconciled
   into durable owners and removed. Git is the archive.
6. **Record supersession.** Never rewrite an accepted ADR to imply a different
   historical decision. Add a new ADR and mark the old one superseded.
7. **Automate cheap checks.** CI should eventually verify internal links,
   duplicate ADR numbers, required ADR metadata, and that active feature specs
   have an owner and explicit exit criteria.

When the project grows tenfold, split documents by bounded context only after a
single owner becomes hard to navigate. Add an index at the old path and keep the
same ownership boundaries. Do not split merely to make files shorter.
