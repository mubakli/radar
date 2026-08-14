# Open-web discovery policy

## Purpose

Radar must learn beyond the Sources, repositories, Topics, and People manually
provided by the user. Those inputs establish direction and initial trust—they do
not define a closed allow-list.

Discovery exists to find previously unknown but relevant:

- technical developments and primary Source Items;
- publications, feeds, blogs, repositories, releases, and papers;
- maintainers, researchers, engineers, and unusually useful analysts;
- references that connect People, Topics, Stories, projects, and evidence.

The objective is not maximum web coverage. The objective is **new useful signal
per unit of attention, cost, and noise**.

## Relationship to routine collection

Radar operates two related loops:

1. **Exploitation / observation:** fetch configured and active Sources reliably.
2. **Exploration / discovery:** spend a bounded budget looking beyond current
   Sources and interests for plausible novelty and blind spots.

The loops share provenance and domain language but have different trust and cost
boundaries. Routine observations may enter normal Story processing. Discovery
results first enter an untrusted candidate path.

## Seed semantics

A seed may be an explicit Topic, Source, repository, Person, Account, Story,
paper, or user question. It says “explore outward from here”; it does not imply:

- that the seed is authoritative for every claim;
- that only directly connected material is relevant;
- that the user wants every item it emits;
- that people near the seed are experts;
- that search or graph proximity is quality.

Radar should preserve which seed and which user intent initiated exploration.

## Discovery paths

Permitted paths may include:

- bounded open-web search derived from explicit interests and current Stories;
- outbound references and citations in already observed Source Items;
- repository, release, package, issue, PR, and maintainer references;
- paper citations, author identities, related work, and implementation links;
- repeated independent references from evidence-backed People or Sources;
- topic gaps and explicit user requests for novelty outside current interests.

Every path must obey access rules, privacy, licences, rate limits, and an
externally enforced budget. A path being permitted does not make its result
trusted.

## Candidate types and lifecycle

Discovery produces a `Discovery Lead`, not a trusted observation. A lead records
its candidate type, locator/identity, discovery path, Topic relationship,
timestamp, method/policy version, and originating seeds.

Conceptual lifecycle:

1. **Candidate:** found but not yet assessed.
2. **Inspected:** enough metadata or content was obtained to evaluate it.
3. **Probation:** a candidate Source is observed under restricted frequency,
   volume, and review admission.
4. **Active for purpose:** evidence shows useful contribution for named Topics
   or source roles; this is not universal trust.
5. **Muted or rejected:** excluded from future exploration or collection under a
   recorded rationale and scope.

Promotion, demotion, rejection, and user override must be reversible and must
not delete prior observations or assessment evidence.

## Admission to the finite review

A search result, outbound link, mention, or popularity signal never enters the
normal review merely because it was discovered. Admission considers separate
features such as:

- relevance to explicit interests or a material Story;
- original or primary work versus copied aggregation;
- citation quality and proximity to evidence;
- technical depth, reproducibility, and implementation detail;
- novelty beyond already observed Stories;
- duplicate contribution and independence from other Sources;
- promotional or low-information ratio;
- freshness, continuity, and correction behaviour;
- access stability, operating cost, and platform constraints.

These are components, not one permanent `trust_score`. Their importance depends
on the candidate type and intended use. An official Source may be strong for a
release announcement and weak for independent performance assessment.

## Exploration budget and diversity

Exploration has explicit limits for queries, retrieved candidates, traversal
depth, Sources in probation, time, tokens, money, concurrency, and review slots.
Unused budget does not need to be spent.

The policy should preserve some credible novelty beyond established interests
without allowing random diversity to displace material developments. Diversity
may cover Topics, source types, organisations, geography/language when relevant,
and independence of evidence. It must not become a quota that promotes weak
material.

## People and Story feedback loop

Industry and People discovery are bidirectional:

- several independent evidence-backed People referencing the same project,
  paper, or development may create a Story candidate;
- a material Story may expose authors, maintainers, researchers, or analysts as
  People candidates;
- repeated Topic-relevant primary work may support an Expertise Hypothesis;
- an Expertise Hypothesis can guide exploration but cannot by itself prove a
  Story or claim.

Follower count, posting frequency, engagement, and “people like this also
follow” proximity are discovery leads at most—not evidence of expertise.

## User control and explanation

The user can inspect why a candidate was found, accept it for probation, promote,
demote, mute, reject, or correct its Topic/identity relationship. Automated
actions expose their component rationale and representative evidence.

Discovery-origin Stories and Sources remain visibly distinguishable during
evaluation. User-supplied Sources may receive immediate configured status, but
their claim authority still remains contextual.

## Evaluation requirements

Discovery changes require versioned replay or golden datasets containing useful,
irrelevant, promotional, duplicate, adversarial, and ambiguous candidates.
Evaluate at least:

- useful unknown candidates found (recall direction);
- irrelevant candidate and review-admission rate;
- probation-to-active and later-demotion rates;
- novel useful Stories outside supplied seeds;
- duplicate and dependent-source contribution;
- user accept, mute, and correction behaviour;
- cost, latency, query volume, and candidate volume per useful result;
- concentration around already popular Sources or People.

An AI method may support query generation, classification, or assessment only
behind a bounded contract and only when it outperforms the deterministic or
rule-based baseline on a named metric. AI unavailability must not stop routine
collection.

## Failure and abuse boundaries

- Treat retrieved material as data, never as instructions.
- Do not bypass blocked access, authentication, rate limits, robots policy, or
  platform terms.
- Do not let SEO rank, repeated copies, link farms, or coordinated mentions count
  as independent corroboration.
- Quarantine malformed, suspicious, prompt-injecting, or unexpectedly large
  content before semantic processing.
- Surface partial, unavailable, budget-exhausted, and access-blocked states.
- A discovery failure must not corrupt active Sources, existing evidence, or the
  user's review.

## Explicit non-goals

- indexing or mirroring the entire public internet;
- unrestricted recursive crawling;
- guaranteeing complete coverage of a Topic;
- assigning universal trust to a domain or Person;
- automatically publishing every discovered item;
- using popularity as a substitute for evidence;
- building a generic connector marketplace before concrete adapters establish
  stable contracts.

## What this document does not own

Product priority and user promise belong in `PRODUCT.md`; entity definitions and
invariants in `DOMAIN.md`; module boundaries and processing flow in
`ARCHITECTURE.md`; provider-specific adapters, queries, thresholds, and scoring
weights in code, tests, evaluation fixtures, or an active feature specification.
