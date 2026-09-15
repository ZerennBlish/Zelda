# Zelda Process Error Log

This file records process failures and the rules they produced. Gameplay defects and fix history belong in [Zerenn-Bug-History.md](Zerenn-Bug-History.md); outstanding work belongs in [Tracked-Items.md](Tracked-Items.md).

Entries are Zelda-specific. Do not import another project's failures as if they happened here. Record what was observed, the cause if established, and the prevention rule.

## E-001 - Unity connection targeted another project

**Source:** Session 05, 2026-09-14, MCP setup in this Codex task.

**Observed:** The existing Codex server named `unity` was configured with `--project-path C:\UnderwhelmingSteve`. Zelda required its own connection.

**Resolution:** Added `unity_zelda`, pinned to `C:\Zelda`, and verified the project path through a direct MCP editor-status call.

**Rule:** Select the Zelda connection explicitly and verify its returned project path before editor work. Tool availability alone does not identify the project.

## E-002 - Configured does not mean loaded

**Source:** Session 05, Unity Pipeline installation.

**Observed:** The package installer updated the manifest successfully while the editor still reported no ready Pipeline instance. The package then resolved and compiled after the Zelda editor was brought to the foreground.

**Rule:** Verify package resolution and editor readiness after installation. Distinguish a saved MCP configuration, a ready editor bridge, and a successful direct MCP call. None alone proves a gameplay test passed.

## E-003 - Shell checks saw different Codex configuration

**Source:** Session 05, connection verification.

**Observed:** Sandboxed `codex mcp get` reported no server, including for the previously configured `unity` server. Reading the intended user's configuration and verifying it in that user's context confirmed the entries existed.

**Rule:** Check the actual configuration path and execution context before recreating a supposedly missing connection. Do not print credentials or raw process command lines during diagnostics.

## E-004 - Role changes left conflicting documentation

**Source:** Session 05, documentation adaptation.

**Observed:** After AGENTS.md assigned Codex implementation ownership, supporting docs still called Codex read-only and Claude the sole writer. Close-out also assumed an Opus-led Claude.ai upload workflow.

**Resolution:** Established a Codex operating guide and common workflow, updated the role-bearing documents, and separated historical handoffs from current instructions.

**Rule:** When a role or delivery workflow changes, update the documents that prescribe it and link them to the current authority. Preserve historical reports as history.

## E-005 - Style hook rejected a documentation patch

**Source:** Session 05, documentation adaptation.

**Observed:** A write hook rejected an added line containing U+2014. The line repeated an unchanged historical heading as part of a replacement block; the patch did not apply.

**Resolution:** Retained the heading as unchanged patch context and retried with ASCII punctuation in new content. The corrected patch applied.

**Rule:** Preserve existing text as context when it is not changing. Use ASCII punctuation in added content to comply with the active write hook; do not disable the hook.

## Entry template

Before adding an entry, distinguish a project failure from an unverified diagnosis. Keep credentials out of examples and command output.

```text
## E-NNN - Short title
Source: session/date and evidence
Observed: what actually happened
Cause: established cause, or explicitly unknown
Resolution: action and observed outcome
Rule: the specific prevention step
```
