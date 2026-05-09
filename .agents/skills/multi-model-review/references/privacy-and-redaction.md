# Privacy and redaction policy

This plugin sends selected artifacts to external AI providers configured by the user. The providers are governed by their own terms, retention settings, and data-use policies.

## Never send

- API keys, OAuth tokens, session cookies, private keys, certificates, SSH keys.
- `.env` files or deployment secrets.
- Production logs containing customer data.
- Customer names, emails, addresses, phone numbers, payment data, health data, or other regulated information unless the user has explicitly approved sending that data to the configured provider.
- Full repositories when a diff or small snippet is sufficient.

## Prefer sending

- A task summary.
- A compact git diff.
- Relevant snippets with secrets removed.
- Test output with sensitive values redacted.
- Public docs or public error messages.

## Redaction behavior

The MCP server includes pattern-based redaction for common secret formats. It is intentionally conservative, but regex redaction is not perfect. Codex should still inspect artifacts before calling the tool.
