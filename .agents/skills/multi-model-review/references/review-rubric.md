# Review rubric

A useful model review is specific, grounded, and actionable.

## Required output fields

Each reviewer should return JSON with:

- `verdict`: `pass`, `concerns`, or `fail`.
- `summary`: brief overall assessment.
- `issues`: list of findings.
- `questions`: clarifying questions, only when the missing information blocks judgment.
- `confidence`: `low`, `medium`, or `high`.

Each issue should include:

- `severity`: `low`, `medium`, `high`, or `critical`.
- `category`: e.g. correctness, security, testing, architecture, ux, writing.
- `location`: file, function, line, paragraph, or section when available.
- `claim`: the problem.
- `evidence`: why the reviewer believes the claim.
- `suggested_fix`: concrete recommendation.

## Acceptance guidance for Codex

Accept feedback when it is:

- tied to a real line, behavior, or user constraint;
- compatible with the requested scope;
- likely to improve correctness, safety, quality, or clarity;
- not already covered by tests or implementation.

Reject or downgrade feedback when it is:

- vague;
- based on an invented API or file;
- a broad preference without a user-facing benefit;
- incompatible with the task constraints;
- a request to expand scope unnecessarily.
