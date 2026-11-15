# Help

This file provides help for humans and AI agents working on this project.

## For Humans

- **Primary Maintainers**: Eric Sanacore ([@esanacore](https://github.com/esanacore)).
- **Getting Started**: Read `README.md` and `docs/SETUP.md`.
- **Asking for Help**: Open an issue at
  [github.com/esanacore/DevLaunchpad/issues](https://github.com/esanacore/DevLaunchpad/issues).

## For AI Agents

- **Entry Point**: Read `AGENTS.md` and `constitution/CONSTITUTION.md`.
- **Workflow**: Follow `constitution/AI_WORKFLOW.md`.
- **Command Help**: See `docs/COMMAND_REFERENCE.md`.
- **Troubleshooting**: See `docs/TROUBLESHOOTING.md`.
- **Handoffs**: If you are finishing a session, see `docs/AGENT_HANDOFF.md`.
- **Demo Page**: Keep the demo page current — when a change alters user-facing behavior and the repository has a `demo.html`, update it in the same change; a demo still showing the old behavior is an incomplete change (`constitution/DOCUMENTATION.md`, "Demo Page").
- **Session Log**: Consult and extend the session log — before planning, read the newest rows of `docs/sessions/README.md` and search `docs/sessions/` for earlier sessions on the same topic (`bash constitution/scripts/find_sessions.sh <keyword>`), reading the ones that match; before completing non-trivial work, add this session's summary there (request, context, decisions, discoveries, outcome — a summary, never a transcript, and never a secret) and list it in the index (`constitution/AI_WORKFLOW.md`, "Session Summaries").

## Escalation Policy

If an agent is stuck or encounters an ambiguous situation:
1. Search `docs/TROUBLESHOOTING.md`.
2. Check `TODO.md` for related tasks or blockers.
3. Stop and ask the human user for clarification.
