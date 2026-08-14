# Architecture

## Architectural stance

The current proposal is to start as a modular monolith with background workers
and one primary relational database. Separate modules by responsibility in code
and data ownership before separating deployment. This keeps the initial system
testable and operable while leaving extraction paths if measured scale or
isolation needs appear. See proposed
[ADR-0003](architecture/decisions/0003-start-as-a-modular-monolith.md).

The architecture is a target shape, not permission to implement every module at
once.

## Logical boundaries

| Boundary | Owns | Does not own |
| --- | --- | --- |
| **Collection** | Source configuration and lifecycle execution, scheduling, adapters, fetch attempts, raw observations, provenance | Candidate quality, semantic importance, user feed, research conclusions |
| **Knowledge & Discovery** | Exploration seeds, Discovery Leads and paths, source assessment, normalisation, entity references, duplicate relations, Story hypotheses, Topic links, Person/Account identity evidence, discovery Signals | Final personalised ordering, runtime research orchestration, unrestricted crawling |
| **Ranking & Review** | User interests and feedback, candidate selection, relevance/importance/novelty features, finite review assembly, explanations | Raw collection, global truth about expertise |
| **Research** | Research Cases, budgets, tool execution, evidence register, report generation and evaluation | General ingestion scheduling, application internals in agent context |
| **Application Interface** | User-facing commands, queries, review and feedback workflows | Cross-module data ownership shortcuts |

These are logical boundaries. Initially they may live in one process or solution.
Communication inside the monolith should use explicit application contracts;
avoid reading another boundary's tables merely because it is convenient.

## Core processing flow

1. **Seed and explore:** configured interests, Sources, People, repositories, and
   current Stories initiate bounded searches and reference expansion that create
   untrusted Discovery Leads.
2. **Assess and admit:** inspect candidates, preserve discovery provenance, and
   route candidate Sources through probation before normal review admission.
3. **Collect:** adapters fetch active and probation Sources within distinct
   budgets and platform rules and record attempts.
4. **Preserve:** store the original identity and recoverable representation.
5. **Normalise:** derive common metadata without erasing source-specific data;
   manual collection records a bounded fetch attempt and its outcome.
6. **Deterministic deduplication:** canonical locators, platform IDs, hashes, and
   explicit redirects remove exact duplicates cheaply.
7. **Story candidate generation:** time, linked entities, lexical similarity, shared
   references, and source relationships create a small set of plausible Story
   matches.
8. **Semantic judgement when justified:** a bounded classifier may decide among
   candidates, report uncertainty, or decline.
9. **Story and knowledge update:** write reversible, versioned relationships.
10. **Ranking:** compute separate features for relevance, importance, novelty,
   confidence, and quality; assemble a finite review with explanations.
11. **Feedback:** record user actions as contextual Signals and use them to
    refine both review ranking and later exploration without erasing explicit
    user interests.

The cheap-to-expensive funnel is deliberate. Do not send the full corpus to an
LLM or vector search when deterministic filters can reduce the candidate set.
See [ADR-0002](architecture/decisions/0002-deterministic-first-ai-boundary.md).

## Open-web discovery flow

Open-web discovery is adjacent to routine Collection but has a stricter admission
boundary:

1. select a bounded set of seeds and an exploration objective;
2. generate deterministic or evaluated semantic queries/reference expansions;
3. retrieve within provider, access, and cost constraints;
4. create or merge Discovery Leads while recording the complete discovery path;
5. inspect candidates and compute separate assessment components;
6. let the user dismiss, mute, or place a candidate Source into probation;
7. collect probation Sources under restricted frequency and volume;
8. promote, keep in probation, demote, or reject using versioned policy and
   representative evidence;
9. measure useful novelty, noise, concentration, and cost independently from
   routine ingestion.

Search results and outbound references are navigation inputs. They do not bypass
normal provenance, Story formation, evidence, or review-admission rules. See
[`DISCOVERY.md`](DISCOVERY.md).

## Research flow

Research is an isolated application workflow:

1. create a Research Case from a Story or question;
2. assemble a runtime context bundle containing the question, user constraints,
   candidate evidence, tool availability, and budget;
3. start the Research Agent with only the static files in
   `runtime/research-agent/` plus that bundle;
4. let it retrieve additional evidence within policy and budget;
5. persist tool observations and an evidence register independently of the
   narrative report;
6. validate report structure, citation coverage, unsupported-claim checks, and
   terminal status;
7. publish the report or an explicit insufficient-evidence/failure result.

The runtime agent must not receive controllers, database schemas, source code,
deployment details, secrets, or general coding-agent instructions.

## AI boundary

Appropriate early uses include bounded semantic clustering, classification of
ambiguous content, claim/evidence extraction, research planning, synthesis, and
explanation. Ingestion scheduling, retries, persistence, exact deduplication,
access control, quotas, job state, and citation validation remain deterministic.

Every AI capability must define:

- a narrow input/output contract;
- candidate-reduction strategy;
- policy/prompt and model version metadata;
- timeout, retry, concurrency, and monetary/token budgets;
- observable unavailable, failed, partial, and declined states;
- evaluation fixtures with representative and adversarial cases;
- a deterministic fallback or an explicit reason no fallback is valid.

Provider-specific SDK calls stay behind adapters. Domain modules depend on the
capability contract, not on a model vendor.

## Persistence and retrieval

- Use relational storage first for observations, relationships, jobs,
  provenance, user state, and derived artefact metadata.
- Keep raw payloads in relational or object storage according to measured size,
  retention, and legal constraints; store stable references either way.
- Use normal indexes and full-text search before introducing specialised vector
  infrastructure.
- Add embeddings only for a named retrieval/clustering use case with an offline
  evaluation showing improvement over lexical and metadata baselines.
- Consider a graph database only when specific traversal workloads are both
  important and demonstrably poor in the existing model.

## Observability

At minimum, trace an item from fetch through normalisation, duplicate/Story
decisions, ranking features, presentation, and feedback. Record job state,
latency, cost, model/policy version, candidate counts, confidence, and failure
category. Research additionally records tool use, evidence coverage, citation
validation, and stop reason. Discovery additionally records seed/query/reference
path, candidate merge, inspection and lifecycle decisions, exploration budget,
useful novelty, review admission, rejection, and later demotion.

Logs must not become an accidental store for secrets, full private content, or
hidden model reasoning.

## Evolution sequence

### Stage 1 — prove the review loop

RSS/Atom; raw provenance; deterministic deduplication; finite daily review;
source-priority explanations and explicit feedback.

### Stage 2 — show the three capabilities and reliable operation

Basic Story grouping; People/Account evidence foundation; bounded Deep Research;
GitHub as a second concrete connector; reliable scheduling, budgets, and
operational visibility.

### Stage 3 — prove discovery beyond configured seeds

Bounded open-web search and reference expansion; auditable Discovery Leads;
candidate review; Source probation; separate exploration/exploitation budgets;
useful-novelty and discovery-noise evaluation.

### Stage 4 — personalise and improve semantic quality

Explicit interests and feedback replay; explainable component ranking;
candidate-based semantic Story judgement; better Topic resolution; cost and
quality dashboards.

### Stage 5 — mature People and research intelligence

Evidence-backed expertise profiles, automated but explainable expert discovery,
People↔Story cross-signals, and deeper research workflows. Add a source-specific
connector only when it closes a measured information gap.

Add a source only when its access rules, provenance quality, incremental user
value, and operating cost are understood. Scale deployments based on measured
bottlenecks rather than projected complexity.

## Known over-engineering risks

- starting with every potential platform, especially access-constrained social
  networks;
- confusing bounded open-web discovery with an obligation to crawl or index the
  whole web;
- allowing search ranking, popularity, or repeated copies to bypass candidate
  assessment and Source probation;
- building a generic connector marketplace before two concrete adapters reveal
  stable common needs;
- multi-agent research before a single-agent tool loop has evaluations and stop
  conditions;
- real-time pipelines for a product whose intended use is a short periodic review;
- graph/vector databases selected from domain aesthetics rather than workloads;
- composite “intelligence” scores that destroy evidence and explainability;
- microservices before module boundaries and operational load are proven;
- using LLMs to compensate for missing identity, provenance, or state-machine design.

## What this document does not own

Product priorities belong in `PRODUCT.md`; domain definitions in `DOMAIN.md`;
decision history in ADRs; exact interfaces and storage schemas in code and tests;
temporary migration/delivery detail in active feature specifications.
