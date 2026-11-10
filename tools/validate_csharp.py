#!/usr/bin/env python3
"""Tree-sitter C# syntax validation for the Unity source tree.

This catches malformed C# syntax, not missing Unity/package symbols or API mismatches.
A successful result is not a substitute for a Unity Editor compilation.
"""

from __future__ import annotations

import sys
from pathlib import Path

import tree_sitter_c_sharp
from tree_sitter import Language, Parser

ROOT = Path(__file__).resolve().parents[1]
SOURCE_ROOT = ROOT / "Assets" / "_Project"
LANGUAGE = Language(tree_sitter_c_sharp.language())


def syntax_errors(parser: Parser, source: bytes) -> list[tuple[int, int, str]]:
    tree = parser.parse(source)
    errors: list[tuple[int, int, str]] = []
    pending = [tree.root_node]

    while pending:
        node = pending.pop()
        if node.type == "ERROR" or node.is_missing:
            row, column = node.start_point
            errors.append((row + 1, column + 1, node.type))
        pending.extend(reversed(node.children))

    return errors


def main() -> int:
    parser = Parser(LANGUAGE)
    files = sorted(SOURCE_ROOT.rglob("*.cs"))
    if not files:
        print("FAIL: no C# source files found under Assets/_Project.")
        return 1

    failures: list[str] = []
    for path in files:
        try:
            source = path.read_bytes()
        except OSError as error:
            failures.append(f"{path.relative_to(ROOT)}: unable to read source: {error}")
            continue

        for line, column, node_type in syntax_errors(parser, source):
            failures.append(
                f"{path.relative_to(ROOT)}:{line}:{column}: C# syntax node {node_type}"
            )

    if failures:
        for failure in failures:
            print(f"FAIL: {failure}")
        return 1

    print(f"PASS: parsed {len(files)} C# files with no syntax errors.")
    print("NOTE: Tree-sitter validates syntax only; Unity must still compile the project.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
