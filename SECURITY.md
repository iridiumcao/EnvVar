# Security Policy

## Supported Versions

EnvVar is maintained as a single-release Windows desktop application. Security fixes are applied on `main` and shipped in the latest published release.

| Version | Supported |
| ------- | --------- |
| Latest GitHub release | :white_check_mark: |
| Older releases | :x: |
| Unreleased local builds and forks | :x: |

## Reporting a Vulnerability

Please report suspected vulnerabilities **privately**. Do **not** open a public GitHub issue for problems involving:

- the installer or release artifacts
- environment variable read/write behavior
- registry access or privilege escalation
- JSON import/export handling
- local data stored under `%LocalAppData%\EnvVar`

Use GitHub's **Report a vulnerability** flow from the repository's **Security** tab when it is available. If private reporting is not available, contact the maintainer through a private GitHub channel and include a link to this repository.

To help with triage, include:

1. The affected version or commit SHA, and whether the build came from a GitHub release or a local build.
2. Your Windows version, and whether the app was running with administrator privileges.
3. Clear reproduction steps, expected behavior, actual behavior, and any proof of concept.
4. The security impact, including whether the issue affects user-level variables, system-level variables, installer behavior, or local metadata/history files.
5. Relevant logs or screenshots with secrets, private paths, and real environment variable values removed.

Please do not include passwords, access tokens, private keys, or full copies of sensitive environment variables in the report.

We aim to acknowledge valid reports within 7 days and will share status updates during triage. If a report is accepted, fixes will be prepared for the latest supported release and disclosure will be coordinated after a patched release is available.

Unsigned-installer reputation warnings by themselves, such as Windows SmartScreen or machine-learning-only VirusTotal detections, are not automatically security vulnerabilities. However, if you have evidence that a published artifact has been tampered with, report it privately.
