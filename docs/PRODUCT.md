# Product intent

## Problem

Technical information is abundant but attention is scarce. Recommendation
algorithms hide niche experts and reward engagement rather than learning value.
Manually monitoring official blogs, repositories, papers, communities, and
technical creators does not scale.

Radar helps one user answer four questions:

1. What materially changed in my fields?
2. Which changes are worth my attention?
3. Which people can help me learn more over time?
4. Where should I start if I want to understand this deeply?

## Product promise

A 10–15 minute Radar session should leave the user with a defensible view of
the few developments that matter, why they matter, where the evidence came
from, and what deserves deeper investigation.

Radar is not a general news reader. It is a personal technical-intelligence
system optimised for **high signal, low noise, provenance, and learning**.

## Capabilities

### Technology Radar

Continuously observe trusted sources **and** explore beyond them for new technical
developments, primary sources, repositories, papers, and useful publications.
Normalise the resulting observations, group multiple mentions of the same
development into a Story, and rank Stories by relevance, importance, novelty,
confidence, and evidence quality. User-supplied sources are seeds, not a closed
allow-list.

### People Radar

Discover people who repeatedly provide useful technical signal. Recommendations
must explain the topics, evidence, and time window that justify them. People are
knowledge sources and participants in the discovery graph—not interchangeable
social-media accounts and not a popularity leaderboard. Discovery must include
people the user has never supplied or followed when open-web evidence shows
topic-relevant primary work, implementation, or unusually useful analysis.

### Deep Research

Investigate a Story or question through primary sources, code, papers, issues,
maintainer discussions, expert analysis, and competing approaches. Preserve
disagreement, cite claims, distinguish inference from sourced fact, and say when
the evidence is insufficient.

The three capabilities reinforce each other: high-signal people can reveal an
emerging Story; an important Story can reveal which people have demonstrated
expertise; research can improve future source and topic understanding.

## Product principles

- **Attention is the constrained resource.** Success is not item volume or
  session length.
- **Evidence before eloquence.** A shorter traceable result beats a polished but
  weakly supported explanation.
- **Primary sources first.** Commentary adds interpretation but does not replace
  direct evidence.
- **Personal relevance without a filter bubble.** Explain recommendations and
  reserve some space for credible novelty outside established interests.
- **Seeds, not boundaries.** Configured sources and People establish initial
  direction; bounded exploration must be able to find unknown but relevant work.
- **Explore without flooding.** Discovery candidates are evaluated separately
  and must earn entry into the finite review.
- **People are contextual.** Expertise is topic-specific, demonstrated over
  time, and never reduced to follower count.
- **Deterministic by default.** AI is reserved for semantic or open-ended work
  where it earns its cost and complexity.
- **User control.** The user can inspect, correct, mute, follow, save, and provide
  feedback on sources, Stories, topics, and people.

## Initial product slice

The first useful version should prove the attention-saving loop:

1. collect from a small set of reliable, accessible sources such as RSS/Atom
   and GitHub releases;
2. preserve original items and provenance;
3. deduplicate exact and near-identical items with cheap methods first;
4. present a finite daily review with explicit relevance reasons;
5. let the user save, dismiss, and mark an item as useful;
6. start Deep Research on one selected Story using the runtime policy.

Initially, People Radar should support manually curated people and evidence
capture. This is a bootstrap mechanism, not the final product boundary.
Automated expert discovery should follow reliable provenance, identity
resolution, and Story formation.

After the review loop and routine collection are reliable, the next product
slice must prove controlled open-web discovery: generate bounded exploration
from interests and known Stories; create auditable candidates from search and
outbound references; evaluate candidate sources through probation; and measure
whether previously unknown material improves the review without increasing
noise beyond an accepted threshold.

## Explicit non-goals for the initial system

- indexing the entire public internet or recursively crawling without limits;
- supporting every social network or bypassing platform access rules;
- real-time alerts for every event;
- an infinite engagement feed;
- a universal “expert score” or public ranking of people;
- fully autonomous research without budgets or stop conditions;
- training a foundation model;
- becoming a publishing, social, or team-collaboration platform;
- guaranteeing that a Story cluster or expertise inference is objectively true.

## Outcome signals

Early success is measured with behaviour and review quality, not vanity metrics:

- useful Stories per review and percentage of presented Stories dismissed;
- duplicate mentions suppressed without hiding materially different claims;
- time required to understand the day's important changes;
- saved/read/researched actions and explicit usefulness feedback;
- recommendation explanations the user considers defensible;
- research claims with direct citations and clearly labelled uncertainty;
- LLM cost and latency per useful Story or completed research job.
- useful Stories, sources, repositories, papers, and People discovered outside
  the user's supplied seeds per bounded exploration cost;
- candidate acceptance, probation success, and discovery-noise rates;

These are evaluation directions, not fixed targets. Targets belong in an
experiment or feature specification once baseline data exists.

## What this document does not own

Domain entity definitions belong in `DOMAIN.md`; system boundaries in
`ARCHITECTURE.md`; current delivery scope in `features/`; implementation and
exact ranking formulae in code and tests.
