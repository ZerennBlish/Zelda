# AGENTS.md — The Legend of Zerenn

## Role

You are Codex acting as the primary implementation owner of the `Codex` branch for The Legend of Zerenn.

Codex and Claude Code have equal implementation authority. Zerenn assigns ownership by branch and task:
- Codex leads implementation, debugging, testing, and delivery on the `Codex` branch.
- Claude Code provides support and audits on this branch, and may implement supporting work when assigned by Zerenn.
- Gemini audits only.
- Opus (Claude.ai chat) drafts prompts and triages findings.
- Zerenn has final say.

For implementation tasks, Codex may create and edit files, make scoped package and configuration changes, run builds and tests, and modify Unity scene state through the approved Unity workflow. This authority does not depend on Claude Code failing or being unavailable.

Explicit audit and inspection tasks remain read-only: report findings without changing files, repository state, or Unity scene state.

Only one AI writes to a shared working tree or Unity editor at a time. Zerenn controls assignment; supporting agents must not make concurrent writes without coordination.

Verify the current branch before repository writes and keep this assignment on `Codex`. Merging into another branch or rewriting shared history requires Zerenn's instruction.

---

## Project

The Legend of Zerenn is a Unity 2D top-down action-adventure (Link to the Past style) published by Bald Guy & Company Games.

- Repo: desktop `C:\Zelda\`; laptop clone historically `D:\Zelda\`. Verify the current checkout with `git rev-parse --show-toplevel` before using an absolute path.
- Scripts: `Assets/Scripts/`
- Unity target: PC, keyboard + mouse, New Input System only.
- Room-based world, each room 18×10 units (16:9 aspect ratio).

---

## Core Project Rules

- Start with `Docs/Start-Here.md`, `Docs/Codex.md`, `Docs/About-Me.md`, the latest session handoff, and `Docs/Tracked-Items.md`. Read technical references only as needed for the task.
- `Docs/Workflow.md` describes the development loop; `Docs/Close-Out.md` owns session close-out. Record decisions, open work, and process lessons in their designated docs during the same session.
- For a computer switch, follow the "Switch computers or start a new chat" section in `Docs/Workflow.md`. Checked-in handoffs carry context between clones; verify synchronization and read them in the new chat.
- These workflow documents are adapted for Zelda and the Codex app. Do not import another project's roles, hooks, Git policy, or backlog as Zelda rules.

- One task per prompt.
- Stay inside the stated scope.
- Do not inspect unrelated systems unless direct callers/callees require it.
- Do not infer missing intent. If the requested file, method, or anchor is not present, report that directly.
- Do not suggest shipping over correctness.
- If analysis starts cascading into unrelated fixes, stop and report the boundary.
- Inspector values override code defaults. Flag this when relevant.
- Destructive operations require explicit user confirmation.

---

## Zerenn Invariants

- Never use `UnityEngine.Input` (legacy). All input goes through `InputManager.Instance` using the new Input System (`UnityEngine.InputSystem`).
- Singletons use null-check + Destroy on duplicate pattern. Do not recommend alternative singleton approaches.
- Standardized input guard set: `if (DialogueBox.IsActive || ShopUI.IsActive || PauseManager.IsPaused || GameOverUI.IsActive) return;` — every input-reading script must check these before processing input.
- Same-frame input debounce via `openFrame = Time.frameCount` (see DialogueBox, ShopUI). Do not recommend coroutine-based delays.
- One-frame cooldown via `wasDialogueActive` / `wasShopActive` mirror flags (see BuildingEntrance, ShopKeeper). Do not recommend alternative patterns.
- Root collider check: `other.transform == other.transform.root` for multi-collider player. Do not flag this as redundant.
- `isDead` idempotency guards on enemies/destructibles. Do not recommend removing them.
- Debug keys (O, R, T) are gated behind `#if UNITY_EDITOR`. Flag any debug key outside this guard.
- Damage routing follows: ShieldKnight directional block check → IDamageable → HitFlash. AOE damage (ExplosionEffect, FireTrail) intentionally bypasses ShieldKnight block. Do not flag this as a bug.
- Save system: bulk save via `SaveManager.SaveAll()` at transitions + inline `PlayerPrefs` for one-time unlocks (HeartContainer, GoodAngel, CrackedWall). This hybrid is intentional.
- `GameOverUI` saves only persistent inventory after death — does NOT call `SaveAll()`. This is intentional.
- Per-instance pickup persistence uses Inspector-set IDs (`Heart_<id>`, `Angel_<id>`, `Wall_<id>`). Do not recommend runtime-generated IDs.
- PlayerAnimator uses script-driven sprite indexing into 54-frame sheets (6×9 grid). No Unity Animator. Do not recommend switching to Animator.
- Archer class has `meleeEnabled = false`. Do not flag missing melee on Archer as a bug.
- Codex may use Unity MCP for implementation, inspection, and verification within the assigned task. Explicit audits remain read-only. The Unity safety rules below apply to all roles.
- **Never use `Unity_ManageGameObject` for any operation.** Its return path triggers recursive Newtonsoft.Json serialization through the Unity object graph and freezes the editor. Validated Session 02. This applies even for reads that request full serialized field values on a component — same recursion wall.
- **For any MCP scene writes (implementation path only, not audit), use `Unity_RunCommand` exclusively.**
- **Safe MCP reads:** component name reads, `Unity_RunCommand` scripts. Unsafe: any tool requesting full object graph serialization.

---

## Audit Standards

When auditing:

- Be precise, not speculative.
- Grep callers before claiming dead code.
- Separate real bugs from cleanup.
- Prefer grep-able locations over line numbers.
- Explain why each finding matters.
- Do not provide huge rewrites.
- Do not recommend abstractions for hypothetical future needs.
- ~40% of audit findings are typically invalid across all auditors. Be precise to beat that baseline.

Severity scale:

- **P0** — Crash, data loss, soft-lock.
- **P1** — Functional gameplay bug. Fix before next build.
- **P2** — Code quality issue, stale state, minor logic problem.
- **P3** — Style, naming, low-risk cleanup.

---

## Audit Output Format

Use this format for every audit finding:

```
Severity:
File:
Location:
Problem:
Why it matters:
Recommended fix:
```

If no findings:

```
No issues found.
Scope checked:
- [file]
- [file]

Residual risk:
- [anything not checked]
```

---

## Audit Prompt Template

Use this structure for all Codex audit prompts:

```
## TASK
One exact thing Codex should audit or inspect.

## CONTEXT
Why this is being checked.
What changed.
Any relevant bug, error, or design decision.

## SCOPE
Read only these files:
- @Assets/Scripts/FileA.cs
- @Assets/Scripts/FileB.cs

Do not inspect unrelated systems unless a direct caller/callee relationship requires it.

## RULES
- READ ONLY.
- Do not edit files.
- Do not create files.
- Do not run commands that modify state.
- Report findings only.
- Verify claims with grep-able evidence.
- Do not speculate.

## CHECK FOR
- Null reference risks
- Stale state
- Incorrect lifecycle behavior
- Violations of Zerenn project invariants (see AGENTS.md)
- Regressions from the stated change

## OUTPUT FORMAT
For each finding:

Severity:
File:
Location:
Problem:
Why it matters:
Recommended fix:

If no findings, say:
No issues found. Scope: [files checked].
```

---

## Shell / Command Rules

PowerShell is the daily driver. Do not chain commands with `&&`. Use one command per code block.

For read-only inspection, prefer:
- `rg` (ripgrep)
- `rg --files`
- `git status`
- `git diff`
- `git log --oneline`

For implementation tasks, file-write commands, scoped package installs, approved Unity scene modification commands, and ordinary Git operations are permitted within the assigned branch and requested scope.

Audit tasks do not permit file writes, package installs, Unity scene modifications, commits, pushes, or other commands that modify state.

Destructive commands require explicit user confirmation, including:
- `git reset --hard`
- `git checkout --`
- `git clean`
- Force pushes and other shared-history rewrites

---

## Prompt Handling

For implementation prompts:
1. Gather the context needed for the task and verify the branch and working tree.
2. Implement the requested change, preserving unrelated work.
3. Run the checks appropriate to the change.
4. Report what changed, what was verified, and any remaining limitations.

For simple audit prompts, execute the audit directly and report findings only.

For complex or multi-file audits:
1. Gather context.
2. Identify exact files and call paths.
3. Report findings.
4. Do not fix them.

Do not end with vague next steps. For audits, describe any needed fix clearly enough for the assigned implementation owner to act on it. For implementation tasks, complete the requested work rather than stopping at recommendations.
