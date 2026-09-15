# Zelda Session Close-Out

This file owns the close-out sequence for Codex-led work on the `Codex` branch. Run it when Zerenn ends the session or the agreed session work is complete. A normal follow-up in the same task does not require starting a new session record.

## 1. Capture scope and outstanding work

Check the current branch, working tree, and diff. Separate this session's changes from pre-existing work. Add newly reported issues to [Tracked-Items.md](Tracked-Items.md) with evidence and status before context is lost.

Keep work on `Codex`. Do not stage unrelated files or merge into another branch as part of an implied close-out.

## 2. Finish appropriate verification

- Docs-only work: review content, relative links, roles, and `git diff --check`.
- Code or package work: confirm Unity has finished compiling and inspect relevant diagnostics.
- Logic changes: run the applicable focused tests or exercise the changed paths.
- Scene/Inspector changes: verify actual serialized values and the resulting editor behavior through the approved route.
- Gameplay/UI changes: record the observed playtest result. If Zerenn's visual or feel check is still needed, state it as pending; do not claim it passed.

Record command results and remaining gaps. An editor reporting ready is not evidence of a complete gameplay or release test.

## 3. Review and resolve findings

Perform a self-review of the changed scope. When an independent audit is assigned or required for the agreed delivery, prepare the brief from [Audit-Briefs.md](Audit-Briefs.md) and use [AI-Audit-Workflow.md](AI-Audit-Workflow.md).

Record whether Claude or another reviewer actually ran. Triage evidence, fix verified in-scope defects, and rerun affected checks after fixes. Capture deferred work in the tracker. Do not label an unaudited change independently reviewed.

## 4. Update the authoritative documents

| Change | Update |
| --- | --- |
| Assignment, permissions, invariant | AGENTS.md and affected supporting instructions |
| Operating workflow | Codex.md / Workflow.md / this file, according to ownership |
| Game design decision | Zerenn-Decisions.md |
| Architecture or feature behavior | Relevant technical reference |
| Save keys or persistence contract | Zerenn-Data-Models.md |
| Confirmed gameplay defect/fix | Zerenn-Bug-History.md |
| Process failure | Error-Log.md |
| Open work or completed item | Tracked-Items.md |

Record completion evidence before removing a finished tracker item. Historical docs are not silently promoted to current verification.

## 5. Write the handoff

Create or update `Docs/Sessions/Session-NN-Handoff.md`, choosing the next number from the existing filenames. Preserve prior sessions. Update the current-handoff link in [Start-Here.md](Start-Here.md).

Include the date, branch, base revision, task scope, changed files, decisions, verification, actual audit status, outstanding tracker IDs, and commit/push status. Distinguish this session's work from pre-existing changes.

## 6. Complete the requested Git delivery

Commit and push on `Codex` when that is part of Zerenn's request or the agreed delivery. Stage the specific intended files. Keep unrelated changes out of the commit.

Before claiming synchronization, verify the actual upstream and fresh remote state. A missing upstream means the branch is local-only until publication is performed; `origin/main..HEAD` is not a synchronization check for `Codex`.

Merges into `Dev` or `main`, force pushes, and destructive recovery require Zerenn's instruction. Do not perform them automatically at close-out.

If Git delivery was not requested, report the remaining local changes plainly.

## 7. Deliver the result

Summarize what changed, what passed, and anything still pending. Link the handoff and relevant files in the Codex task.

The checked-in docs are Codex's continuity source. Optional Claude.ai knowledge staging/upload is a separate support handoff when requested. The existing `copy-for-claude.ps1` is not run automatically; it clears an external staging folder.
