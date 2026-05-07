# AI-Audit-Workflow.md

**Portable workflow for running multi-AI codebase audits.** Stack-agnostic. Lift this file unchanged into any project's `Docs/` folder.

This is the pattern that produced the Zerenn audit (April 2026, 100+ findings, all P1s fixed in one extended session) and matures further with each project. Use it on any codebase that's grown past the point where one set of eyes is enough.

---

## When to Run an Audit

**Greenfield projects:** every 4-6 weeks of active development, or before any major content/feature milestone.

**Legacy projects (pre-audit-discipline):** one full sweep first, then settle into the regular cadence. The first audit on a long-running unaudited codebase is significantly bigger than subsequent ones.

**Forced moments:**
- Before a major refactor (audit reveals what depends on what)
- After a long break from the project (audit catches what you forgot)
- Before any content scale milestone (boss in a game, big feature in an app)
- After onboarding a new collaborator (audit captures tribal knowledge)

---

## The Roles

| Role | Who | Responsibility |
|------|-----|----------------|
| Triage | Opus (Claude chat) | Drafts audit prompts, consolidates findings, triages by severity, drafts fix prompts |
| Auditor 1 (primary) | Codex (ChatGPT) | Read-only review, finds concrete bugs |
| Auditor 2 (secondary) | Gemini | Read-only review, edge-case focus |
| Auditor 3 (tertiary) | Claude Code | Read-only review, implementer's-eye structural review |
| Implementer | Claude Code | Applies fix prompts, verifies builds, runs tests |
| Final say | You (project lead) | Designs decisions, approves fixes, runs playtests/QA |

**Critical rule:** auditors are READ-ONLY. They produce findings, never edit files. Every audit prompt explicitly says this. Gemini in particular needs three explicit warnings — past behavior is to "helpfully" rewrite files unless told repeatedly not to.

**Critical rule:** triage is centralized through Opus. Auditor findings don't go directly to Claude Code. They go to Opus first for severity triage, design questions, and grouped fix prompt drafting.

---

## Why Three Auditors

Different models catch different things.

- **Codex** finds concrete, named bugs. Severity-tagged, location-precise. Best for "this method has a null check missing."
- **Gemini** thinks in edge cases. Best for "what happens if X happens during Y." Finds rare but real bugs.
- **Claude Code** does implementer's-eye review. Best for "this code is technically correct but the structure invites bugs." Catches duplication, smells, architectural drift.

Triple coverage means a single auditor missing something doesn't mean it goes unfound. When all three agree on a finding, confidence is very high. When only one finds it, triage decides whether the chain of reasoning is sound.

---

## Batching Strategy

**Don't audit everything at once.** A monolithic audit produces too many findings to triage cleanly, and three auditors hitting the entire codebase eat context limits.

**Default batch size:** ~1,500 lines of related code per batch. Roughly 8-15 files depending on file size.

**Group files by domain:**
- Player/character core
- Managers, save system, world
- NPCs, dialogue, shop, items, UI
- Weapons, projectiles, combat
- Enemies (often split into 2 batches if there are many)

**Same-domain files audited together** so cross-cutting issues (shared patterns, duplicated code, interface contracts) get caught.

For a typical small-to-medium project, expect 4-6 batches. Plan one batch per session unless multiple auditors are running in parallel and you have time to triage all three at once.

---

## The Audit Prompt Structure

Every batch produces ONE markdown file with three sections — one for each auditor. They run in parallel (you open three terminals or three chat windows).

The structure:

```
# Audit Batch N — <Domain>

**Files:** <list with line counts>
**Total:** ~<N> lines
**Run all three in parallel. Paste findings to Opus when done.**

**NOTE:** Previous batches' fixes applied. <relevant context>

---

## Codex Prompt

```
You are auditing a <stack> codebase. You are READ-ONLY. Do not create,
modify, or delete any files.

PROJECT: <name>
STACK: <stack>
LANGUAGE: <language>

SCOPE — Batch N (~<line count>):
<file paths>

CONTEXT:
<short context about the project, prior audit state, parallel auditors>

BATCH-SPECIFIC FOCUS:
<3-7 specific things to look for in THIS batch — don't repeat generics>

GENERAL — ALWAYS LOOK FOR:
- Null handling
- Race conditions
- Coroutine/async lifecycle
- Time/state management pitfalls
- Allocations in hot paths
- Duplicate code across types
- Layer/permission/scope assumptions
- Violations of stack-specific best practices
- Unguarded debug logs

OUTPUT FORMAT:
For each finding:
- Severity: P1 / P2 / P3
- Bug: short name
- Location: file and line range
- Cause: why it's wrong
- Suggested fix: what should change (do NOT apply it)

P1 = bugs/crashes/data loss under normal play
P2 = architecture smells, degraded UX, missing edge cases
P3 = style, nits, minor refactoring

Do not apply any fixes. Do not write any files. Report only.
```

## Claude Code Prompt

```
[same structure as Codex prompt — adjust focus toward implementer review:
state machine completeness, side-by-side comparison of similar files,
duplication that compounds with growth, abandoned features.]
```

## Gemini Prompt

```
⚠️ READ-ONLY AUDIT — DO NOT CREATE, MODIFY, OR DELETE ANY FILES.
⚠️ DO NOT WRITE SCRIPTS TO /tmp.
⚠️ DO NOT RUN ANY COMMAND THAT WRITES TO DISK.

[same structure — adjust focus toward edge cases and unusual scenarios.
Add at least one mid-prompt reminder of READ-ONLY status.
Add a final reminder at the end. Three warnings minimum.]
```
```

---

## Severity Triage

Findings come back in inconsistent shapes from three auditors. Opus consolidates and re-tags by severity:

**P1 (blocks ship):**
- Crashes, data loss, save corruption
- Soft-locks the player can't recover from
- Game-breaking exploits players will find
- Anything that produces wrong values in the wrong direction (e.g., negative HP)

**P2 (architecture / design):**
- Real bugs that need a specific edge case to trigger
- Code smells that compound with content scale
- Performance problems that don't bite yet but will
- Inconsistencies that make future changes harder

**P3 (defer):**
- Style nits
- Optimization that won't matter at current scale
- Refactors that are nice-to-have but not blocking
- Dead code that's harmless

**Triage rule:** when auditors disagree on severity, Opus decides. Default to higher severity if unsure. P3 findings in current audit can become P1 after a refactor brings them into a hot path — the line moves with the codebase.

---

## Fix Prompt Strategy

**Group fixes, don't fire one prompt per finding.** A batch of 12 P1s should produce 2-3 grouped fix prompts, not 12 individual ones. Group by:

- **Related root cause** (e.g., all "missing isDead guard" fixes go in one prompt across all enemies)
- **Same files touched** (multiple fixes to the same handful of files = one prompt)
- **Logical category** (input guards, save system, lifecycle cleanup)

A typical batch produces three groups: **Group A** (hot bugs), **Group B** (architectural patches), **Group C** (design behavior changes). Run them in order, compile after each.

**Don't put more than ~5 distinct fixes in one prompt.** Past that, Claude Code has trouble keeping track of which file is which.

**Always end fix prompts with explicit verification steps:**

```
After all changes, verify the project compiles. Then test:
- <specific scenario 1>
- <specific scenario 2>
- <specific scenario 3>
```

---

## Design Questions

Some audit findings can't be fixed without a design decision from the project lead. Examples from past audits:

- "Should the boomerang break grass?" (yes — classic Zelda)
- "Should enemy projectiles destroy bushes?" (yes — consistency)
- "Should escaped GoblinThiefs refund stolen rupees?" (no — design intent)

**The pattern:** triage flags these explicitly, asks the question, waits for an answer before drafting the fix prompt. Don't guess the design. Don't pick the easier option.

When the answer comes back, capture it in the project's `Decisions.md` document with rationale. The audit-fix process produces decisions doc content as a side effect.

---

## What Audits Produce

Each audit cycle produces:

1. **N audit prompt files** (one per batch) — kept in `Docs/Audits/<date>/` for reference
2. **N findings dumps** (raw output from each auditor, three per batch) — kept in same folder
3. **Triage output** — Opus's consolidated finding list, severity-tagged, with design questions called out
4. **Fix prompts** (Group A/B/C per batch) — kept in same folder
5. **Updated `Bug-History.md`** — every finding gets an entry
6. **Updated `Decisions.md`** — design questions answered during the cycle become Decisions entries
7. **Updated commit history** — one commit per fix group, descriptive messages

The folder structure for an audit's artifacts:

```
Docs/Audits/<YYYY-MM-DD>/
├── batch1-prompts.md
├── batch1-findings-codex.md
├── batch1-findings-claudecode.md
├── batch1-findings-gemini.md
├── batch1-groupA-fixes.md
├── batch1-groupB-fixes.md
├── batch1-groupC-fixes.md
├── batch2-... (same shape)
└── audit-summary.md   (Opus drafts this at the end)
```

Optional but useful: an `audit-summary.md` at the end captures the cycle's themes (e.g., "this audit revealed save key drift across X files; these are now centralized") so future audits can spot regressions of past lessons.

---

## Common Patterns Audits Surface

After running enough audits, the same bug classes appear across projects. Watching for these in advance saves time:

**Same-frame input race.** Multiple readers of the same input action firing in one frame. Solutions: openFrame check, wasXActive mirror flag, root component check.

**Death/destroy idempotency.** `destroy` calls are deferred to end-of-frame in many engines. Two damage sources call die twice. Drops, effects, side-effects fire twice. Solution: `isDead` guard, set true as first line of die.

**Singleton drift.** Different patterns across files. Solution: pick one, document it, enforce.

**Save key drift.** Keys written but not deleted, deleted but not written, scattered across files. Solution: centralize delete logic, document every key in Data-Models.

**Lifecycle cleanup in custom methods.** `Die()` does cleanup, but room change / scene unload / hazard skip Die. Solution: put cleanup in the engine's standard destroy hook (OnDestroy in Unity, useEffect cleanup in React).

**Coroutines/async under pause.** Pause-aware coroutines that should NOT pause (UI animations) need explicit realtime variants.

**Cross-component duplication.** Several files implement the "same enemy / same screen / same form" pattern with 70%+ shared code. Solution: extract base class / hook / component.

**ShieldKnight-style branch duplication.** One special case duplicated across N callers. Solution: interface, polymorphism, or strategy pattern.

When you see one of these in a new project, you can often skip ahead to the known fix.

---

## Lessons Learned (cross-project)

**Auditors disagree, that's the point.** Triple coverage means findings are rarely missed. Agreement across auditors raises confidence; disagreement is a signal to think harder, not assume one is wrong.

**The auditor's job is not the implementer's job.** Read-only audits stay read-only. Findings route through Opus (triage) → Claude Code (implement). Mixing the roles loses the design discipline of triage.

**Triage is where the value is added.** Findings dumps are noisy. Severity tags, grouped fixes, and explicit design questions are what turn raw audit output into actionable work.

**Group fixes ruthlessly.** A batch of 12 P1s should NOT produce 12 fix prompts. Group by root cause, by files touched, by logical category.

**Capture design decisions immediately.** Every audit fix that involves a design call goes in `Decisions.md` the same session it's made. Future you needs the rationale, not just the choice.

**Audit cadence matters.** Audits get cheaper with practice and with shorter intervals. The first audit on an unaudited codebase is huge; the second is half the size; by the fourth they're routine.
