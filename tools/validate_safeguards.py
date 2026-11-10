#!/usr/bin/env python3
"""Repository safety checks for secrets, agent guidance, and GitHub Actions policy.

This is a high-confidence pattern and policy check, not a complete secret scanner,
malware scanner, dependency audit, or replacement for GitHub secret scanning.
"""

from __future__ import annotations

import argparse
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
SKIP_DIRS = {
    ".git", ".venv", "venv", "Library", "Temp", "Obj", "Build", "Builds",
    "Logs", "UserSettings", "node_modules", "__pycache__",
}
MAX_TEXT_FILE_BYTES = 5 * 1024 * 1024

SECRET_FILE_NAMES = {
    ".env", ".env.local", ".env.production", ".env.development",
    "credentials.json", "secrets.json", "id_rsa", "id_ed25519",
}
SECRET_FILE_SUFFIXES = {".pem", ".p12", ".pfx"}
SECRET_FILE_PREFIXES = {"service-account-"}

SECRET_PATTERNS: tuple[tuple[str, re.Pattern[str]], ...] = (
    ("private-key material", re.compile(
        r"-----BEGIN (?:RSA |EC |OPENSSH |DSA )?PRIVATE KEY-----"
    )),
    ("AWS access key ID", re.compile(r"\bAKIA[0-9A-Z]{16}\b")),
    ("GitHub access token", re.compile(
        r"\b(?:gh[pousr]_[A-Za-z0-9]{20,}|github_pat_[A-Za-z0-9_]{30,})\b"
    )),
    ("Slack access token", re.compile(r"\bxox[baprs]-[A-Za-z0-9-]{20,}\b")),
    ("Google API key", re.compile(r"\bAIza[0-9A-Za-z_-]{35}\b")),
    ("Stripe live secret key", re.compile(r"\bsk_live_[A-Za-z0-9]{20,}\b")),
    ("high-confidence credential assignment", re.compile(
        r"""(?ix)
        \b(?:api[_-]?key|client[_-]?secret|access[_-]?token|refresh[_-]?token|password)
        \b["']?\s*[:=]\s*["']
        ([A-Za-z0-9/+_.=-]{20,})
        ["']
        """
    )),
)
PLACEHOLDER_PREFIXES = (
    "your_", "your-", "replace", "example", "changeme", "change_me",
    "placeholder", "dummy", "sample", "not_a_real", "redacted",
)


def is_secret_filename(path: Path) -> bool:
    name = path.name.lower()
    if name in SECRET_FILE_NAMES:
        return True
    if name.startswith(".env.") and name != ".env.example":
        return True
    if path.suffix.lower() in SECRET_FILE_SUFFIXES:
        return True
    return any(name.startswith(prefix) for prefix in SECRET_FILE_PREFIXES)


def scan_text(text: str) -> list[str]:
    findings: list[str] = []
    for label, pattern in SECRET_PATTERNS:
        for match in pattern.finditer(text):
            captured = match.group(1) if match.lastindex else match.group(0)
            normalized = captured.strip().lower()
            if label == "high-confidence credential assignment" and any(
                normalized.startswith(prefix) for prefix in PLACEHOLDER_PREFIXES
            ):
                continue
            findings.append(label)
            break
    return findings


def iter_repository_files():
    for path in ROOT.rglob("*"):
        if not path.is_file() or path.is_symlink():
            continue
        relative = path.relative_to(ROOT)
        if any(part in SKIP_DIRS for part in relative.parts):
            continue
        yield path


def validate_secrets() -> tuple[list[str], int]:
    failures: list[str] = []
    scanned = 0
    for path in iter_repository_files():
        relative = path.relative_to(ROOT).as_posix()
        if is_secret_filename(path):
            failures.append(f"Sensitive-looking file must not be committed: {relative}")
            continue

        try:
            if path.stat().st_size > MAX_TEXT_FILE_BYTES:
                continue
            text = path.read_text(encoding="utf-8")
        except (OSError, UnicodeDecodeError):
            continue

        scanned += 1
        for finding in scan_text(text):
            failures.append(f"Potential {finding} detected in: {relative}")
    return failures, scanned


def validate_repository_policy() -> list[str]:
    failures: list[str] = []

    required_files = (
        "AGENTS.md",
        ".github/copilot-instructions.md",
        ".github/CODEOWNERS",
        ".github/pull_request_template.md",
        "SECURITY.md",
        ".github/dependabot.yml",
        "docs/14_TESTING_AND_QUALITY/REPOSITORY-SAFEGUARDS.md",
        "tools/validate_safeguards.py",
        ".github/workflows/project-integrity.yml",
    )
    for relative in required_files:
        if not (ROOT / relative).is_file():
            failures.append(f"Missing safeguard file: {relative}")

    agents_path = ROOT / "AGENTS.md"
    if agents_path.is_file():
        agents = agents_path.read_text(encoding="utf-8")
        for phrase in (
            "Never force-push",
            "Protect save compatibility",
            "Do not commit secrets",
            "Destructive operations",
            "Review the diff",
        ):
            if phrase.lower() not in agents.lower():
                failures.append(f"AGENTS.md is missing safeguard rule: {phrase}")

    owners_path = ROOT / ".github/CODEOWNERS"
    if owners_path.is_file():
        owners = owners_path.read_text(encoding="utf-8")
        if not re.search(r"(?m)^\*\s+@Legioniz\s*$", owners):
            failures.append("CODEOWNERS must request @Legioniz review for repository changes.")

    pr_template_path = ROOT / ".github/pull_request_template.md"
    if pr_template_path.is_file():
        template = pr_template_path.read_text(encoding="utf-8")
        for phrase in (
            "Tests and verification",
            "Save-data and migration impact",
            "Documentation updated",
            "Third-party asset/license review",
        ):
            if phrase.lower() not in template.lower():
                failures.append(f"Pull request template is missing checklist section: {phrase}")

    workflows_dir = ROOT / ".github/workflows"
    if workflows_dir.is_dir():
        for workflow in sorted(workflows_dir.glob("*.yml")) + sorted(workflows_dir.glob("*.yaml")):
            text = workflow.read_text(encoding="utf-8")
            relative = workflow.relative_to(ROOT).as_posix()

            if re.search(r"(?m)^\s*pull_request_target\s*:", text):
                failures.append(f"{relative}: pull_request_target is prohibited for this project.")
            if not re.search(r"(?m)^permissions:\s*$", text):
                failures.append(f"{relative}: declare explicit workflow permissions.")
            if not re.search(r"(?m)^\s+contents:\s*read\s*$", text):
                failures.append(f"{relative}: contents permission must be read-only.")
            if re.search(r"(?m)^\s*(?:contents|actions|packages|id-token|pull-requests|issues|security-events):\s*write\s*$", text):
                failures.append(f"{relative}: write permissions require a separate security review.")
            if "secrets." in text:
                failures.append(f"{relative}: do not expose repository secrets to CI workflows.")

            uses_refs = re.findall(r"(?m)^\s*uses:\s*([^\s#]+)", text)
            for reference in uses_refs:
                if reference.startswith("./") or reference.startswith("docker://"):
                    if reference.startswith("docker://") and "@sha256:" not in reference:
                        failures.append(f"{relative}: container action must be pinned by digest: {reference}")
                    continue
                if not re.search(r"@[0-9a-fA-F]{40}$", reference):
                    failures.append(f"{relative}: action must be pinned to a full commit SHA: {reference}")

            if "actions/checkout@" in text and not re.search(
                r"(?m)^\s* persist-credentials:\s*false\s*$", text
            ):
                # YAML indentation may vary; the expression below is the authoritative check.
                if not re.search(r"(?m)^\s+persist-credentials:\s*false\s*$", text):
                    failures.append(f"{relative}: checkout must set persist-credentials: false.")

            if not re.search(r"(?m)^\s*timeout-minutes:\s*\d+\s*$", text):
                failures.append(f"{relative}: jobs must define timeout-minutes.")
            if re.search(r"(?i)\b(?:curl|wget)\b[^\n]*\|\s*(?:sh|bash)\b", text):
                failures.append(f"{relative}: piping downloaded scripts directly to a shell is prohibited.")
            if re.search(r"(?i)\bgit\s+push\b[^\n]*--force|\bgit\s+reset\s+--hard\b|\brm\s+-rf\b", text):
                failures.append(f"{relative}: destructive Git/shell command detected in workflow.")

    gitignore_path = ROOT / ".gitignore"
    if gitignore_path.is_file():
        gitignore = gitignore_path.read_text(encoding="utf-8")
        for pattern in (".env", ".env.*", "!.env.example", "*.pem", "*.p12", "*.pfx"):
            if pattern not in gitignore:
                failures.append(f".gitignore is missing secret-protection pattern: {pattern}")

    workflow_path = ROOT / ".github/workflows/project-integrity.yml"
    if workflow_path.is_file():
        workflow = workflow_path.read_text(encoding="utf-8")
        if "python tools/validate_safeguards.py" not in workflow:
            failures.append("Project integrity workflow must execute validate_safeguards.py.")
        if not re.search(r"(?m)^\s*pull_request:\s*$", workflow):
            failures.append("Project integrity workflow must validate pull requests.")

    return failures


def run_self_tests() -> list[str]:
    failures: list[str] = []
    token_cases = (
        ("AWS", "AWS_ACCESS_KEY_ID=" + "AKIA" + "1" * 16, "AWS access key ID"),
        ("GitHub", "token = " + "ghp_" + "A" * 36, "GitHub access token"),
        ("private key", "-----BEGIN " + "PRIVATE KEY-----", "private-key material"),
        ("Google", "AIza" + "A" * 35, "Google API key"),
    )
    for name, sample, expected in token_cases:
        if expected not in scan_text(sample):
            failures.append(f"Secret scanner self-test failed for {name} pattern.")

    safe_examples = (
        'api_key = "YOUR_API_KEY_GOES_HERE"',
        'password: "replace-with-a-real-secret"',
        'access_token = "example-token-value-that-is-not-real"',
    )
    for sample in safe_examples:
        if scan_text(sample):
            failures.append("Secret scanner self-test incorrectly flagged a placeholder.")
    return failures


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--self-test", action="store_true", help="Run built-in scanner tests before scanning.")
    args = parser.parse_args()

    failures: list[str] = []
    if args.self_test:
        failures.extend(run_self_tests())

    policy_failures = validate_repository_policy()
    secret_failures, scanned = validate_secrets()
    failures.extend(policy_failures)
    failures.extend(secret_failures)

    if failures:
        for failure in failures:
            print(f"FAIL: {failure}")
        return 1

    print("PASS: repository safeguards and workflow policy checks completed.")
    print(f"PASS: scanned {scanned} readable text files for high-confidence credential patterns.")
    print("NOTE: this is not a complete secret/history scanner; enable GitHub secret scanning and push protection where available.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
