# Privacy Policy

_Last updated: 2026-06-06_

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

- It does **not** declare any network capability and makes no outbound network
  requests itself.
- It does **not** collect usage analytics or telemetry.
- It does **not** read repository contents. To show the current branch it reads
  only the local `.git/HEAD` and `.git/config` files; it never runs `git` or
  contacts a remote.

## Launching other applications

When you choose an action (open folder, open in editor/terminal, open a remote in
your browser), the extension asks Windows to launch that program with the path or
URL you selected. Any data handling from that point is governed by the privacy
policy of the program you launched (your browser, editor, etc.), not by Dev
Launchpad.

## Contact

Questions about this policy can be raised as an issue on the project's GitHub
repository.
