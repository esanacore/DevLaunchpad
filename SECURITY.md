# Project Security

This project follows [Eric's Engineering Constitution Security Standards](constitution/SECURITY.md).

## Local Security Concerns

- **LAN Exposure**: None. Dev Launchpad does not run any network services or bind to any
  ports. It only launches local applications (editor, terminal, Explorer, browser) and,
  when you choose those actions, the `git`/`gh` command-line tools.
- **Credential Handling**: The extension does not collect, store, or transmit any credentials.
  Actions that reach the network (Clone, Sync All GitHub Repos) rely on your own existing
  `git`/`gh` credentials; never commit secrets to this repository.
- **Sensitive Data**: None. No PII is collected or processed. All configuration is stored
  locally in Windows packaged app storage (see [`docs/PRIVACY.md`](docs/PRIVACY.md)).

## Security Checklist

- [x] Credentials are stored in environment variables, not code. *(N/A — the extension handles no credentials.)*
- [x] Dependencies are audited for vulnerabilities. *(`NuGetAuditMode=direct` is enabled in `Directory.Build.props`.)*
- [x] Inputs are validated at boundaries. *(`ProcessLauncher` guards missing/invalid paths and targets before launching.)*
- [x] Logs do not contain secrets or PII. *(The local debug log records file paths and tool output only, never credentials.)*
