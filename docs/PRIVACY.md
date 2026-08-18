# Privacy Policy

_Last updated: 2026-08-16_

Dev Launchpad ("the extension") is a Microsoft PowerToys Command Palette extension
that helps you launch repositories, developer tools, and local servers.

## Summary

**Dev Launchpad does not collect, transmit, or share any personal data.** It has no
servers, no analytics, no telemetry, and no network calls of its own.

## What the extension stores

All configuration is stored **locally on your device** and never leaves it:

- Your settings (projects folder, editor command, terminal command, custom
  commands, favorite websites, local servers).
- The list of repositories you have **pinned** and **recently opened**.
- A local debug log used for troubleshooting.

These are written to your machine under:

```
%LOCALAPPDATA%\Packages\DevLaunchpad_<publisher-id>\LocalState\DevLaunchpad\
```

You can inspect or delete these files at any time. Uninstalling the extension
removes them.

## What the extension does *not* do

- It does **not** declare any network capability, and the extension process itself
  makes no outbound network requests, collects no usage analytics, and sends no
  telemetry.
- It does **not** read the *contents* of your repositories. The repository list
  shows each repo's branch and status by reading local `.git` metadata files
  (e.g. `.git/HEAD`, `.git/config`) directly, without running `git`.
- It does **not** store, transmit, or handle any credentials, tokens, or passwords.

## Actions that run git or the GitHub CLI

Some actions you can choose *do* invoke external command-line tools on your behalf:

- **Switch branch** runs `git checkout` in the selected repository.
- **Clone Repository** runs `git clone` for the URL you provide.
- **Sync All GitHub Repos** runs the GitHub CLI (`gh`) and `git` to list, clone, and
  fast-forward-pull your repositories.

These tools may contact remote servers (such as GitHub) using **your own** existing
`git`/`gh` configuration and credentials. Dev Launchpad does not supply, store, or
transmit those credentials — authentication and any resulting network activity are
handled entirely by `git`/`gh` under your account, exactly as if you had run the
command yourself in a terminal.

## Launching other applications

When you choose an action (open folder, open in editor/terminal, open a remote in
your browser), the extension asks Windows to launch that program with the path or
URL you selected. Any data handling from that point is governed by the privacy
policy of the program you launched (your browser, editor, etc.), not by Dev
Launchpad.

## Contact

Questions about this policy can be raised as an issue on the project's GitHub
repository.
