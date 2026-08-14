# Domain language and invariants

This document defines conceptual language. Concepts are not instructions to
create one database table or class per term.

## Core concepts

| Term | Meaning | Important distinction |
| --- | --- | --- |
| **Source** | A configured origin Radar can observe, such as a feed, repository, publication, channel, or account. | A Source is not the content it emits and not automatically trustworthy for every claim. |
| **Source Item** | One observed unit from a Source: post, release, paper, video, issue, comment, or article. | The original observation remains identifiable even after deduplication or clustering. |
| **Discovery Lead** | An untrusted pointer to a potentially relevant item, Source, repository, paper, Person, or Account found through search, a reference, or another Signal. | A Discovery Lead is a navigation candidate—not yet a Source Item, quality judgement, or evidence for its own claims. |
| **Source Assessment** | A time-bounded, purpose-specific evaluation of whether a candidate Source contributes useful original or interpretive signal. | It is evidence for a lifecycle decision, not permanent trust in a whole domain or author. |
| **Person** | A real-world individual Radar may learn from or reason about. | A Person is distinct from their platform accounts and organisations. |
| **Account** | A platform-specific identity associated with a Person or organisation, with a confidence level. | Account-to-Person linkage may be uncertain or change over time. |
| **Topic** | A stable-enough subject used for interests, classification, expertise evidence, and retrieval. | A Topic is not a Story; “PostgreSQL” is a Topic, a specific release is a Story. |
| **Story** | Radar's evolving hypothesis that several Source Items refer to the same meaningful development or event. | A Story is not merely a summary and is not assumed objectively correct. |
| **Claim** | A specific assertion made by a source or derived by Radar. | Claims carry provenance and type: observed, quoted, inferred, or opinion. |
| **Evidence** | A source-backed observation used to support, contradict, or contextualise a Claim. | Evidence strength depends on the claim and source relationship, not a global site score. |
| **Signal** | A time-stamped observation useful for discovery or ranking: a citation, release, interaction, contribution, repeated topic focus, or user action. | A Signal is an input, not a verdict about importance or expertise. |
| **Expertise Hypothesis** | A topic-scoped, time-sensitive inference that a Person may be worth following for a reason. | It must remain explainable and revisable; it is not a permanent label. |
| **Research Case** | A bounded investigation initiated from a Story or user question, with constraints, evidence, and status. | It is separate from the feed and may conclude with insufficient evidence. |
| **Research Report** | The auditable output of a Research Case. | It is a synthesis, not a replacement for its cited sources. |
| **User Interest** | An explicit or inferred topic preference with provenance, strength, and time. | Dismissal of one Story does not necessarily mean disinterest in its Topic. |

## Key relationships

- A Source emits Source Items.
- A configured Source, Topic, Person, Story, or Source Item may seed exploration
  that produces Discovery Leads.
- A Discovery Lead records how it was found and may be dismissed, merged,
  inspected, or placed into bounded source probation.
- A Source Assessment may justify promotion, demotion, or continued probation;
  it never erases prior Source Items or their provenance.
- A Source Item can mention zero or more People, Accounts, Topics, repositories,
  papers, tools, or other Source Items.
- A Story groups Source Items with membership confidence and a recorded reason.
- A Story contains or references Claims; Evidence can support, contradict, or
  contextualise them.
- A Person can control several Accounts. Identity resolution records confidence
  and evidence rather than silently merging identities.
- Signals can contribute to a Story hypothesis, an Expertise Hypothesis, or a
  ranking decision.
- A Research Case begins from a Story or question and produces zero or more
  reports; “insufficient evidence” is a valid result.

## Invariants

### Provenance and evidence

1. Every Source Item retains its source identity, canonical locator when
   available, observed time, and raw or recoverable representation subject to
   legal and storage constraints.
2. Derived content never overwrites the original observation.
3. Every material derived Claim can be traced to evidence or is explicitly
   labelled as an inference.
4. Contradictory evidence is preserved; the system does not silently select the
   most convenient source.
5. Source authority is evaluated per claim. “Official” is strong evidence for
   what a project announced, but not automatically for performance or adoption.
5a. A Source Item's `PublishedAt` is the publication time reported by its Source
   and may be absent; `ObservedAt` is when Radar observed it.
5b. Missing author and summary values remain absent rather than being invented.
5c. Deterministic exact identity is Source-scoped and uses a stable canonical
   locator when available; normalization never overwrites the raw observation.

### Story formation

6. A Story has its own stable identity and remains a mutable hypothesis over
   Source Items; no Source Item locator or derived canonical content becomes the
   Story's identity.
6a. Each Source Item has at most one current Story membership. Membership is
    reversible and records its creation method, method version, human-readable
    reason, and creation time.
6b. Manual merge moves current memberships into one resulting Story; manual
    split gives the selected Source Item its own current Story. Corrections are
    distinguishable from automatic grouping, auditable, and idempotent, and
    never delete or rewrite a Source Item or its provenance.
7. Exact duplicates, near-duplicates, and items about the same event are
   different relationships and must not be conflated.
8. Different interpretations or materially different claims about one event may
   share a Story while remaining separately attributable.
9. Ranking a Story does not change the underlying evidence or cluster.

### Discovery and source lifecycle

10. User-supplied or configured Sources, People, repositories, and Topics are
    exploration seeds, not the complete boundary of observable knowledge.
11. Every Discovery Lead retains its origin, discovery method/version, observed
    time, query or reference path, and applicable budget context.
12. Search position, repeated mention, engagement, and popularity are not enough
    to promote a candidate or establish source quality.
13. Candidate, probation, active-for-purpose, muted, and rejected states remain
    distinguishable; lifecycle transitions are reversible and auditable.
14. Discovery Leads do not enter the trusted review path until the applicable
    admission policy is satisfied. Rejection or demotion never deletes previous
    observations or supporting assessment evidence.
15. Exploration and routine collection have separate budgets and measurements
    so discovery cannot silently consume unlimited resources or flood the finite
    review.

### People and expertise

16. Person and Account are separate identities; ambiguous matches remain
    unresolved.
17. Expertise and “worth following” are Topic-specific hypotheses supported by
    time-bounded Signals.
18. Popularity, posting frequency, and network proximity are insufficient on
    their own to establish expertise.
19. Promotional ratio, primary-source usage, original work, implementation
    evidence, and credible references may be features; none is a universal
    moral judgement about the Person.
20. A recommendation exposes a human-readable rationale and representative
    evidence.

### Personalisation and feedback

21. Explicit user choices are distinguishable from inferred interests.
22. Feedback events are retained with context so later models do not reinterpret
    them as timeless preferences.
23. Relevance, importance, novelty, confidence, and source quality remain
    conceptually distinct even if a presentation score combines them.

### Finite daily review

24. A daily brief is a bounded, date-scoped view of Stories with contributing
    Source Items observed or published in that date; Stories outside its date
    and configured limit are not silently included.
25. The initial brief order is deterministic: the highest-priority contributing
    Source first, then the reported publication time, with observation time used
    when publication time is absent.
26. `read`, `important`, `saved`, and `not relevant` are explicit item feedback
    states. Repeating an action is idempotent, and feedback remains attached to
    the observed item when the same daily brief is reopened.
27. A brief is complete only when the feedback-bearing item for every presented
    Story is marked `read` or `not relevant`; saving or marking it important does
    not imply review.

### AI-derived results

28. AI-derived classifications, clusters, summaries, and hypotheses record the
    producing policy/model version, input references, timestamp, confidence or
    uncertainty, and terminal failure state.
29. AI unavailability cannot corrupt collection or erase existing evidence.
30. Hidden chain-of-thought is neither required nor stored; concise rationale,
    evidence references, and structured outputs are sufficient for audit.

## Modelling cautions

- Do not commit early to one global `quality_score`, `expert_score`, or
  `importance_score`. Preserve component signals and evaluation context.
- Do not use the Story cluster as the only canonical record; it is a mutable
  hypothesis over immutable observations.
- Do not assume a graph database because the domain contains relationships.
  Start with the simplest persistence model that supports measured queries.
- Do not make every extracted noun a durable Topic. Topic identity requires a
  merge/split strategy and user correction path.
- Do not infer that two accounts belong to one Person from display-name
  similarity alone.
- Do not model a search result or outbound link as a trusted Source Item merely
  because it is relevant enough to inspect.
- Do not collapse Source Assessment into one permanent global trust score;
  preserve purpose, evidence, time window, and user override.

## What this document does not own

Tables, fields, indexes, APIs, prompts, ranking weights, and class names belong
in code, migrations, tests, or an active feature specification. Structural
boundaries belong in `ARCHITECTURE.md`.
