# Active feature specifications

This directory holds short-lived context for non-trivial changes. It is not a
backlog and not an archive.

## Create a feature spec when

- expected behaviour or non-goals are ambiguous;
- the change crosses more than one architectural boundary;
- it changes a domain invariant, stored representation, public contract, or AI
  evaluation behaviour;
- rollout, migration, failure handling, or acceptance criteria cannot be made
  clear in a small issue.

Do not create one for a local bug with an obvious failing test, mechanical
refactoring, dependency patch, or copy/style change.

## Lifecycle

1. Create `<feature-slug>.md` from the structure below.
2. Link the issue/PR and name an owner.
3. Keep only the intended delta; link to durable context instead of restating it.
4. During implementation, record resolved scope decisions in the spec.
5. Before completion, update tests and the durable documents/ADRs that own any
   lasting truth.
6. Delete the feature spec in the completing change. Git retains history.

## Template

```markdown
# Feature: concise name

- Status: Draft | Active | Blocked
- Owner: name
- Issue: link or identifier
- Target outcome: one sentence

## Problem and evidence
What user/system problem is observed? Link evidence; do not assume a solution.

## Scope
- Included:
- Excluded:

## Intended behaviour
Describe externally observable behaviour and important state transitions.

## Affected durable context
Link exact PRODUCT, DOMAIN, DISCOVERY, ARCHITECTURE, runtime-policy, and ADR
sections that are actually affected.
List proposed changes; do not copy their contents.

## Failure, cost, and trust boundaries
What happens on timeout, partial data, AI unavailability, weak evidence, and
budget exhaustion? Which provenance or privacy constraints apply?

## Acceptance checks
- Deterministic tests:
- AI evaluation cases, if applicable:
- Observability/cost checks:
- Migration/rollback checks:

## Open questions
Only unresolved decisions that block or materially change the feature.

## Exit reconciliation
- [ ] Durable truths moved to their single owners
- [ ] Costly decisions captured in ADRs
- [ ] Tests/contracts updated
- [ ] Temporary spec deleted
```
