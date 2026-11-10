# Security Policy

## Reporting a vulnerability

Please do not publish exploitable vulnerabilities, private credentials, access tokens, save files containing personal data, or other sensitive material in a public issue or pull request.

Use GitHub's **Report a vulnerability** option in the repository Security tab when private vulnerability reporting is available. Otherwise, contact the repository owner privately through their GitHub profile and include only the minimum information needed to reproduce the issue.

## Supported project state

Security fixes are prioritized for the current development branch and the latest published build, once one exists. The project is currently a prototype and does not yet have a commercial release support window.

## Contributor safeguards

- Never commit API keys, passwords, access tokens, private keys, signing certificates, or personal credentials.
- Do not place production secrets in Unity assets, \`ProjectSettings\`, source code, screenshots, logs, or documentation.
- Keep machine-local configuration outside version control; commit only sanitized examples.
- Revoke and rotate any credential that has been committed or exposed. Removing it from the latest commit does not make the credential safe; review repository history as well.
- Report suspicious dependencies, build scripts, editor tools, workflow changes, or unexpected network access for human review.
