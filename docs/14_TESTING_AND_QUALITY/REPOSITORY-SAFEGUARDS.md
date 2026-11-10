# Repository Safeguards

> Project: WHERE THE SOUNDINGS END  
> Status: Working baseline  
> Last reviewed: 2026-11-11

This document describes safeguards encoded in the repository and GitHub settings that must be enabled by a repository administrator. A file committed to Git does not automatically activate GitHub branch protection or organization-level security features.

## Safeguards committed to the repository

| Safeguard | Enforcement |
|---|---|
| Agent operating rules | Root AGENTS.md and GitHub Copilot instructions |
| Human code-owner review routing | .github/CODEOWNERS |
| Pull request review checklist | .github/pull_request_template.md |
| Static safety checks | tools/validate_safeguards.py |
| CI validation on pull requests and main pushes | .github/workflows/project-integrity.yml |
| Least-privilege CI | Read-only repository permissions; no workflow secrets; checkout credentials disabled |
| Workflow supply-chain hygiene | Third-party actions pinned to full commit SHAs |
| Dependency update proposals | Dependabot weekly GitHub Actions update checks |
| Vulnerability reporting | Root SECURITY.md |
| Local secret-file exclusion | Root .gitignore patterns |

## What the automated safeguard check covers

Run locally with:

**python tools/validate_safeguards.py --self-test**

The check verifies required policy files, review routing, PR checklist sections, workflow event/permission rules, immutable action references, disabled checkout credential persistence, job timeouts, and common secret-bearing file names. It scans readable repository text files for selected high-confidence credential formats and runs its own positive/negative pattern tests.

**Limitations:** this is a pattern-based current-working-tree scan. It is not a complete secret scanner, historical Git scan, malware scanner, dependency vulnerability audit, Unity compilation, or proof that no credential exists. If a credential is exposed, revoke/rotate it immediately and inspect repository history. Enable GitHub secret scanning and push protection where the repository/account plan makes them available.

## Required GitHub main branch ruleset

A repository administrator must configure this in GitHub because the repository file API cannot activate branch protection:

1. Open **Settings → Rules → Rulesets → New branch ruleset**.
2. Name it **Protect main**, target the **main** branch, and set enforcement to **Active**.
3. Do not add routine bypass actors. Keep administrative bypass disabled unless there is a deliberate emergency policy.
4. Require a pull request before merging.
5. Require at least **1 approval** and dismiss stale approvals when new commits are pushed.
6. Require review from Code Owners.
7. Require these status checks before merging:
   - **Validate Unity project and C# syntax**
   - **Repository safeguards**
8. Require branches to be up to date before merging.
9. Require all review conversations to be resolved.
10. Block force pushes and branch deletion.
11. Consider requiring linear history to keep the project history simple.

The two required check names are the job names in the project-integrity workflow. They may only appear in the selection list after the workflow has completed at least once on the branch.

## Recommended GitHub security settings

Under **Settings → Code security and analysis**, enable the following where available:

- Secret scanning.
- Push protection for detected secrets.
- Dependabot alerts.
- Dependabot security updates.

Review Dependabot pull requests before merging. An automated version update is not automatically compatible with the pinned Unity editor or package set.

## Operational rules for people and coding agents

- Do not force-push, delete protected branches/tags/releases, reset shared history, or run destructive cleanup commands without explicit approval for that exact operation.
- Do not overwrite or delete user-authored files to resolve a conflict. Preserve the original and request a decision when intent is ambiguous.
- Do not commit API keys, passwords, tokens, private keys, signing files, personal data, or unredacted save/log files.
- Do not execute downloaded shell scripts, unreviewed editor scripts, unknown binaries, or package install hooks without reviewing their source and provenance.
- Do not give CI write permissions or expose repository secrets to pull-request code unless a documented need and security review approve it.
- Do not change save schemas, stable content IDs, package/editor versions, or generated-scene contracts without compatibility tests and human review.
- Do not report a test, build, profile, or playtest as successful unless it was actually run and its result inspected.
- Keep changes small, documented, reversible, and covered by focused checks.

## Incident response for an exposed credential

1. Revoke or rotate the credential at the service that issued it.
2. Review access logs and usage for unexpected activity.
3. Remove the credential from the current tree and all affected branches.
4. Review Git history and coordinate any history rewrite with the repository owner; never force-push as an automatic cleanup.
5. Replace it with a secure secret store/environment variable and a sanitized example if needed.
6. Record the incident and remediation without copying the secret into issues, commits, or logs.
