# Instructions for coding agents

## Mission

Build Radar as a high-signal technical-intelligence product. Preserve source
provenance, distinguish evidence from inference, and use AI only where semantic
judgement materially improves the result. Treat manually configured sources,
repositories, Topics, and People as discovery seeds—not as the permanent boundary
of what Radar is able to find.

## Before changing anything

1. Read this file and [`docs/README.md`](docs/README.md).
2. Classify the task using the context-routing table in `docs/README.md`.
3. Read only the routed documents and the nearest relevant code, tests, and
   local `AGENTS.md`, if one exists.
4. State the intended change, affected invariants, and validation method before
   a broad or cross-boundary edit.
5. If an active feature specification exists, treat it as the description of
   the intended delta—not as permanent system documentation.

Do not recursively load all documentation “just in case.” If two authoritative
documents conflict, stop and surface the contradiction instead of choosing the
more convenient interpretation.

## Non-negotiable engineering constraints

- Prefer deterministic, testable, observable, and inexpensive mechanisms.
- Put LLM or agent calls behind explicit interfaces and budgets.
- Preserve raw source identity and provenance before deriving summaries,
  clusters, scores, or recommendations.
- Keep discovery leads separate from observed Source Items and assessed Sources.
  A search result, mention, or outbound link is a candidate, not evidence of
  quality and not permission to place it directly in the review.
- Preserve the discovery path for every candidate and make source promotion,
  demotion, rejection, and user overrides reversible and auditable.
- Bound exploration separately from routine collection. Search, crawl depth,
  candidate volume, source probation, and cost must have explicit limits.
- Persist enough metadata to reproduce or audit AI-derived results: input
  references, policy/prompt version, model/provider, timestamp, confidence, and
  failure state. Do not persist hidden chain-of-thought.
- Separate observed facts, quoted source claims, community opinions, and Radar
  inferences.
- Design graceful degradation: collection, storage, and basic browsing must not
  depend on an LLM being available.
- Treat people/expertise scores as contextual, explainable, and time-sensitive;
  never as a universal measure of a person's quality.
- Do not introduce microservices, a graph database, a vector database, a
  generic plugin framework, or autonomous multi-agent orchestration without a
  measured need and an accepted ADR.
- Never weaken evidence or citation policy to make a research report look more
  complete.
- Never turn Radar into a closed allow-list reader or an unbounded crawler. It
  must explore beyond seeds while protecting the finite review from discovery
  noise.

## Documentation rules

- Code and tests own implementation detail. Do not copy routes, fields, enums,
  or class inventories into durable documentation unless they express a stable
  public contract.
- Update the document that owns a changed truth; do not repeat the truth in a
  second document.
- Create an ADR only for a decision that is costly to reverse, crosses a major
  boundary, or constrains future work.
- Use a feature specification only for a non-trivial in-flight change. Fold its
  durable results into the owning documents, then delete it; Git retains history.
- Documentation-only changes still require link and contradiction checks.

## Definition of done

- Relevant tests and deterministic checks pass.
- New AI behaviour has evaluation cases, cost limits, observable failure modes,
  and a deterministic fallback or explicit unavailable state.
- New discovery behaviour has candidate/replay fixtures, admission and noise
  measurements, exploration budgets, and an auditable user override.
- Provenance and uncertainty remain visible across transformations.
- Any affected durable decision, invariant, or policy is updated in its single
  owning document.
- Temporary feature context has been reconciled or intentionally left active.

## Runtime-agent separation

Files under `runtime/research-agent/` are product runtime policy. They are not
general coding instructions. A coding task that implements research behaviour
may read them; the runtime Research Agent must not receive repository internals,
database design, controller structure, or this file.
