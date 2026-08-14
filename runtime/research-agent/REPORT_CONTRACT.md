# Research report contract

Every Research Case ends in exactly one status:

- `Completed`
- `Completed with material uncertainty`
- `Insufficient evidence`
- `Budget exhausted`
- `Blocked by access or tool failure`
- `Cancelled`

The report is Markdown for human use. The application stores case status,
evidence records, tool events, policy/model versions, budgets, and validation
results as structured data outside the prose report.

## Required report structure

### 1. Answer in brief

Give the best-supported answer in a few sentences. Include the status and the
most decision-relevant uncertainty. Do not write an executive summary that
overstates the evidence.

### 2. What changed or what was found

Describe the concrete development, mechanism, or answer. Separate observed
facts, source claims, and Radar inferences with clear wording and citations.

### 3. Why it matters

Explain technical or practical consequences for the user's stated interests.
Distinguish demonstrated impact from plausible future impact.

### 4. Evidence and analysis

Organise by the research subquestions, not by browsing chronology. For each
major conclusion include:

- conclusion or Claim;
- epistemic label;
- strongest supporting evidence;
- contradicting or limiting evidence;
- scope/version/date;
- concise confidence rationale.

Use a table only when it makes comparison clearer; do not force every claim
into one oversized table.

### 5. Alternatives, prior art, and disagreements

Describe credible alternatives or earlier approaches and preserve material
disagreement. State whether an apparent novelty is new in implementation,
packaging, performance, accessibility, or only marketing.

### 6. People worth following

Optional. Include a Person only when the investigation found topic-relevant,
demonstrated work or unusually useful analysis. For each include:

- relevant Topic;
- why their work is useful;
- representative primary work or analysis;
- uncertainty or possible incentive/conflict.

Do not rank people globally or use follower count as the main rationale.

### 7. Unknowns and next verification steps

List unresolved questions in impact order. Name the specific evidence, test,
version, or access that could resolve each one.

### 8. Sources

Provide a deduplicated source list grouped as primary evidence, independent
analysis, and community/discovery leads. Include stable revision/version and
access date when relevant. A source appearing here does not replace inline
citations.

### 9. Research boundary

State:

- terminal status and stop reason;
- time/freshness window;
- material inaccessible sources or failed branches;
- budget limitations that affected coverage;
- policy version used.

## Quality gates

A report cannot be `Completed` unless:

- critical subquestions are answered or explicitly bounded;
- material factual claims have direct citations;
- source claims and Radar inferences are not phrased as observed facts;
- primary evidence was sought and missing access is disclosed;
- conflicting evidence is represented;
- citations resolve to the cited material and support the nearby claim;
- the report includes a terminal status and stop reason;
- no source or tool instruction overrode research policy.

Style, length, and section depth may adapt to the question. Evidence standards
and terminal-state requirements may not.

