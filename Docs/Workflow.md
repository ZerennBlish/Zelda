# Zelda Workflow

This is the development loop for Zerenn and Codex in the Codex desktop app. It adapts the document organization of IdBidOnThat to a Unity game with Codex leading this branch.

[AGENTS.md](../AGENTS.md) owns role and project rules. [Close-Out.md](Close-Out.md) owns the close-out sequence. [Start-Here.md](Start-Here.md) maps the document set.

## 1. Establish the task and current state

Read the user's request, latest handoff, and tracker. Check the active branch and existing changes. Use live source and editor state for current facts; old handoffs describe what was known then.

Why: a correct change against the wrong branch, wrong Unity editor, or stale version is still the wrong change.

## 2. Make the intended behavior clear

Work through unresolved game or interface decisions with Zerenn. Read the relevant design decisions and sibling implementations. For a larger feature, identify the affected files, call paths, persistence effects, and the checks that will demonstrate success.

Why: implementation should not silently decide game design or override Inspector values.

## 3. Implement the scoped change

Codex performs the assigned implementation directly. Claude can support when assigned. Keep a single writer on the shared checkout and editor, and preserve unrelated work.

Why: branch ownership avoids conflicting edits while allowing both tools to implement.

## 4. Verify what changed

| Change | Evidence to collect |
| --- | --- |
| Documentation | Check references, roles, source claims, and diff formatting |
| C# logic | Successful Unity compilation plus focused behavioral checks or applicable tests |
| Serialized fields or scene wiring | Confirm actual Inspector values and object/component relationships through a safe route |
| Gameplay or UI | Exercise the changed behavior, including relevant pause, death, transition, and repeated-input cases |
| Build or package setup | Confirm resolution/compilation or the requested build result; distinguish readiness from a full game test |

Record the checks actually performed and any gaps. Do not invent test results or use a symbol count as proof of runtime behavior.

## 5. Review and triage

Codex reviews its diff and verification. For independent review, use Claude as the support auditor; additional auditors are used when assigned or requested. Give reviewers the complete agreed scope, the exact revision or working diff, and relevant intentional behavior.

Use [Audit-Briefs.md](Audit-Briefs.md) and [AI-Audit-Workflow.md](AI-Audit-Workflow.md). Findings must have a reachable failure path and evidence. Confirm fixes with targeted checks. A design question goes to Zerenn; an unverified claim stays labeled as such.

## 6. Preserve the result

Update the affected docs, tracker, and handoff. Complete the close-out sequence when the session ends. Follow the user's Git delivery instructions for `Codex`; do not infer permission to merge into `Dev` or `main`.

## When work changes direction

- Capture a new bug report promptly, including its source and reproduction status.
- Continue the active objective unless Zerenn changes it.
- If a change starts cascading into unrelated systems, stop expanding scope and explain the boundary.
- Do not discard work or automatically reset after a machine switch. Inspect current state and remote freshness first; destructive recovery still requires explicit confirmation.
