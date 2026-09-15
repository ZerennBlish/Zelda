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

## Switch computers or start a new chat

Zerenn uses separate desktop and laptop clones, synchronized through GitHub on `Codex`. The documented paths are desktop `C:\Zelda` and laptop `D:\Zelda`; verify the actual checkout on the machine running the task. Using remote control from the laptop to operate the desktop leaves the active files on the desktop.

### Before leaving the active clone

Update the latest handoff and affected tracker entries. For a requested synchronization or computer handoff, commit the intended project changes and documentation on `Codex`, then push to `origin/Codex`. Record the published commit in the task response. Preserve unrelated local changes and identify anything that has not been transferred.

### On the destination clone

1. Verify the repository root, branch, local changes, and origin URL. The expected origin is `https://github.com/ZerennBlish/Zelda.git`. Confirm that the other computer has finished writing and pushed its work.
2. Refresh remote state. Select the local `Codex` branch; if it does not exist, create it to track `origin/Codex`. Resolve any local-work conflict before switching branches.
3. With a clean destination checkout on `Codex`, fast-forward to `origin/Codex`. If the branch has diverged or local changes need preserving, inspect and report that specific state before changing history or replacing files. Do not automatically reset, stash, force-push, or copy one clone over the other.
4. Verify the resulting commit and working tree. Then read [Start-Here.md](Start-Here.md), its current handoff, and [Tracked-Items.md](Tracked-Items.md). A new chat obtains project context from those files; Git does not transfer the conversation itself.
5. Before Unity work, verify the editor and `unity_zelda` point to this clone. The user-local MCP registration does not travel through Git. See [Project Setup](Zerenn-Project-Setup.md) and [Unity MCP Rules](Unity-MCP-Rules.md).

From a verified destination checkout, refresh the branch:

```powershell
git fetch origin Codex
```

After confirming the destination is clean and on `Codex`, update it without creating a merge commit:

```powershell
git merge --ff-only origin/Codex
```

Verify the result:

```powershell
git status --short --branch
```

```powershell
git rev-list --left-right --count HEAD...origin/Codex
```

After a successful refresh, `0 0` means the local and fetched remote branches have the same commits. Only claim the destination clone is updated after checking it on that computer.

## When work changes direction

- Capture a new bug report promptly, including its source and reproduction status.
- Continue the active objective unless Zerenn changes it.
- If a change starts cascading into unrelated systems, stop expanding scope and explain the boundary.
- Do not discard work or automatically reset after a machine switch. Inspect current state and remote freshness first; destructive recovery still requires explicit confirmation.
