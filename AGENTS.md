# AGENTS.md

This repository follows Eric's Engineering Constitution.

## Before Beginning Work

Read:

- `constitution/CONSTITUTION.md`
- `constitution/AI_WORKFLOW.md`
- `constitution/TESTING.md`
- `constitution/DOCUMENTATION.md`
- `constitution/SECURITY.md`
- `constitution/ARCHITECTURE.md`
- `constitution/RELEASES.md`
- `constitution/TODO_GUIDELINES.md`
- `README.md`
- `TODO.md`
- `CHANGELOG.md`

## Required Work Habits

- Understand the task before changing files.
- Create an implementation plan for non-trivial work.
- Follow existing project conventions.
- Keep changes focused.
- Add or update automated tests for behavior changes.
- Update documentation when behavior, setup, usage, architecture, or operations change.
- Update TODO.md with discovered work or completed roadmap items.
- Update CHANGELOG.md for user-facing changes.
- Keep the demo page current — when a change alters user-facing behavior and the repository has a `demo.html`, update it in the same change; a demo still showing the old behavior is an incomplete change (`constitution/DOCUMENTATION.md`, "Demo Page").
- Consult and extend the session log — before planning, read the newest rows of `docs/sessions/README.md` and search `docs/sessions/` for earlier sessions on the same topic (`bash constitution/scripts/find_sessions.sh <keyword>`), reading the ones that match; before completing non-trivial work, add this session's summary there (request, context, decisions, discoveries, outcome — a summary, never a transcript, and never a secret) and list it in the index (`constitution/AI_WORKFLOW.md`, "Session Summaries").
- Review security impact before completing work.
- Suggest future improvements when useful.

## Before Completing Work

- Review documentation impact.
- Review testing impact.
- Update TODO.md.
- Update CHANGELOG.md.
- Perform a security review.
- Report tests run and any tests not run.
- Summarize work clearly.
