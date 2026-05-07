# Opus.md

**Operating manual for Opus in claude.ai chats.** This is the doc you read to understand how prompts get drafted, how audits get run, and how sessions close out for this project.

For Zerenn's persona, communication style, and universal cross-project conventions, see `Docs\About-Me.md`.
For project-specific Legend of Zerenn code rules, see `CLAUDE.md` at the repo root.
For locked design decisions, see `Docs\Zerenn-Decisions.md`.

---

## How These Docs Are Organized

| Doc | Audience | Role |
|-----|----------|------|
| `CLAUDE.md` (project root) | Claude Code | Project tech stack + Zerenn-specific rules. Auto-loaded by CC at session start. Lean by design. |
| `Docs\About-Me.md` | Both Opus and CC, all projects | Persona + universal cross-project conventions. Same content lives in DFW, Brick Headed, Legend of Zerenn — copies stay in sync. |
| `Docs\Opus.md` (this file) | Opus | How prompts get drafted, audits get run, sessions close. The Opus orchestration manual. |
| `Docs\Zerenn-Decisions.md` | Both Opus and CC | Design truth. Updated whenever locked design decisions change. Most valuable file in the repo. |

When in doubt about which doc owns a rule:
- Rules about *how prompts are drafted* go in Opus.md
- Rules about *how the project's tech works* go in CLAUDE.md
- Rules about *who Zerenn is and how he wants to be talked to* go in About-Me.md

---

## Effort Levels (Opus 4.7, May 2026)

`low` / `medium` / `high` / `xhigh` / `max`. Set `/effort xhigh` once at session start as the default — Opus 4.7's intended baseline per Anthropic's April 23, 2026 post-mortem. Correct for almost everything.

Use `/effort max` only for: major architectural decisions, large multi-module changes, mission-critical bug hunts, audits (entire output is reasoning — max payoff for the latency cost), 2,000+ line file audits.

**Never use `ULTRATHINK` or `THINK HARD` keywords in individual prompts.** Documented April 2026 bug: both keywords hardcode reasoning to `high` regardless of session level — at `xhigh` or `max` they actively *downgrade*. GitHub FR is open to fix this; until then, the rule is absolute at every effort level. Persistent `/effort` is the only correct lever; per-prompt thinking keywords actively hurt.

---

## Session Management

Session hygiene matters as much as prompt structure. These commands aren't recovery tools — use them proactively. Context is the fundamental constraint; performance degrades as the window fills.

- **`/clear`** — between unrelated tasks. The "kitchen-sink session" pollutes context with stale information and degrades performance. Clear context entirely between unrelated work.
- **`Esc`** — stops Claude mid-action without losing context. Use the moment you notice it going off track.
- **`Esc + Esc` or `/rewind`** — opens the rewind menu. Restore conversation only, code only, or both to any prior checkpoint. After two failed corrections on the same issue, rewind beats correcting again — the failed attempts are polluting context. **Rule of thumb: rewind > correct.**
- **`/compact <focus>`** — beats letting auto-compact fire. When the context window is filling: `/compact focus on the minimap system, drop the enemy AI debugging`. Auto-compact triggers when Claude is at its least intelligent point (context rot).
- **Subagents for investigation.** Research and exploration tasks should run in a subagent. File reads, greps, and dead-end traces stay in the child's context; only the final summary returns to the main session. Use `"use a subagent to investigate X"` for any task that's primarily exploration.

---

## Failure Patterns to Recognize

Anthropic's docs enumerate five common failure modes. Naming them helps catch them faster.

- **Kitchen-sink session.** One task, then unrelated work, then back to the first. Context fills with irrelevant files. **Fix:** `/clear` between unrelated tasks.
- **Correcting over and over.** Claude does something wrong, you correct, still wrong, correct again. Context becomes polluted with failed approaches. **Fix:** after two failed corrections, `/clear` and write a better initial prompt. Don't try a third correction.
- **Over-specified CLAUDE.md.** A long CLAUDE.md causes Claude to ignore half of it. **Fix:** prune ruthlessly. For each line, ask *would removing this cause Claude to make mistakes?* If not, cut it.
- **Trust-then-verify gap.** Claude produces plausible code that doesn't handle edge cases. **Fix:** verification is non-negotiable. Unity MCP compile check, grep counts, expected outputs — if you can't verify it, don't ship it.
- **Infinite exploration.** Asking Claude to "investigate" without scope. **Fix:** scope investigations narrowly, or delegate to a subagent.
- **Cascade.** Each fix creating new fixes — the original problem still there but new ones appearing. **Fix:** revert immediately. Don't attempt "one more fix." Earned its own rule from the six-batch audit.

---

## Claude Code Prompt Drafting

Maps to Anthropic's official Claude Code best practices. These take precedence over stylistic preferences when there's a conflict.

### Drafting Rules

- **Verification is the single highest-leverage practice.** Every prompt MUST include a way for CC to verify its work. For Zerenn: Unity MCP compile check (must return 0 errors when code changed), and grep counts for added symbols ("confirm exactly N occurrences of MethodName"). If verification fails, the prompt MUST instruct CC to STOP and report — never proceed assuming it worked.
- **Anchor to grep'able strings, not line numbers.** Line numbers shift between when Opus reads a file and when CC runs the prompt. Use unique strings: `find the existing "SaveAll()" call in SaveManager` beats `at line 42`.
- **Word-boundary grep for substring-overlap cases.** When verifying a name that's a substring of another (`IsActive` inside `DialogueBox.IsActive`), specify `grep -w` or `\bName\b`.
- **Reference existing patterns when adding similar code.** "Follow the HeartContainer pickup persistence pattern" beats describing the pattern from scratch.
- **Use `@Assets/Scripts/FileName.cs` syntax for file references.** CC auto-reads `@`-prefixed paths.
- **Address root causes, not symptoms.** "Find why it's thrown" not "catch the exception to silence it."
- **Plan Mode for non-trivial work.** Use when the approach is uncertain or changes touch 5+ files. Skip for typo fixes and variable renames.
- **One concern per prompt.** Don't bundle fix + refactor + new feature. Ship as separate prompts.
- **Read current code state before drafting find/replace blocks.** Read the file via Desktop Commander before drafting; don't trust memory.

### Format

Every Claude Code prompt uses this structure:

```
TASK: [one-line description of what changes and where]

FILE: @Assets/Scripts/<filename>.cs

WHAT TO DO: [numbered or bulleted steps. Be specific. No "do X if Y" conditionals.]

WHY: [design intent, locked decisions referenced, why this approach over alternatives]

CODE:

Find:
[exact text or block from current file — anchor to grep'able strings, never line numbers]

Replace with:
[exact replacement text, with all formatting preserved]

[repeat Find/Replace as needed for the same file]

VERIFICATION:
- Unity MCP compile check must return 0 errors.
- grep -c "<symbol>" /mnt/c/Zelda/Assets/Scripts/<file>.cs → must return N.
- [additional grep counts, expected outputs, or MCP Inspector checks]
- If any verification fails: STOP and report counts.
```

### Example (abbreviated)

```
TASK: Add MinimapUI.IsVisible to the standardized input guard in PlayerController.

FILE: @Assets/Scripts/PlayerController.cs

WHAT TO DO:
1. Find the existing input guard in Update().
2. Add MinimapUI.IsVisible to the condition.

WHY: MinimapUI is a new UI-suspending state. All scripts using the input guard set need it added for consistency.

CODE:

Find:
if (DialogueBox.IsActive || ShopUI.IsActive || PauseManager.IsPaused || GameOverUI.IsActive) return;

Replace with:
if (DialogueBox.IsActive || ShopUI.IsActive || PauseManager.IsPaused || GameOverUI.IsActive || MinimapUI.IsVisible) return;

VERIFICATION:
- Unity MCP compile check must return 0 errors.
- grep -c "MinimapUI.IsVisible" /mnt/c/Zelda/Assets/Scripts/PlayerController.cs → expect 1.
- If any verification fails: STOP and report.
```

### Delivery

Chat-paste is the standard. Opus drafts the prompt inline in conversation, Zerenn copies into the CC CLI per task. A single file at `Docs\Prompts\<name>.md` is used only when the prompt is long enough that chat scroll-back becomes unwieldy.

---

## Audit Workflow

Multi-AI audit pattern:

- **Opus** (claude.ai chat) drafts prompts and triages findings
- **Claude Code, Codex, Gemini** run as parallel auditors (READ-ONLY)
- Claude Code is also the implementer when not auditing
- Auditors do NOT edit files; they produce findings only
- Findings route through Opus for severity triage and fix prompt drafting
- Fix prompts are GROUPED (Group A/B/C) — not one prompt per finding
- ~40% of audit findings are typically invalid — be precise, not speculative

Project-specific auditor docs:
- **Codex** — see `AGENTS.md` at repo root
- **Gemini** — see `GEMINI.md` at repo root

---

## Session Close-Out

Every session ends with the same workflow. Opus runs it without being asked. Full step-by-step lives in `Docs\Close-Out.md`.

Summary: verify-clean → focused audit (if code changed) → compile check → update docs → commit/push → copy-for-claude → upload to Claude.ai → session handoff.

---

## Documentation Discipline

When making non-trivial changes, update the relevant doc the same session. The Decisions doc (`Zerenn-Decisions.md`) is the most valuable — capture rationale, not just the choice.

For Opus's own doc updates via Desktop Commander: announce first ("I'm going to update [file] to add [change], because [reason]"), then write. Strict announce-and-write rule applies to docs only — code edits go through CC, never Opus directly.

---

## Cross-Project Portability

`About-Me.md` and `Opus.md` are designed to be IDENTICAL across all of Zerenn's projects (DFW, Brick Headed, Legend of Zerenn). Same content, same structure. Only `CLAUDE.md` and the project's Decisions doc are project-specific.

The folder these live in varies per project (DFW uses `ai-docs\` because lowercase `docs\` hosts the Play Store privacy policy via GitHub Pages; Brick Headed and Zerenn use `Docs\`). The folder name doesn't matter — the content split does.

When setting up a new project: copy `About-Me.md` and `Opus.md` from any existing project's AI-context folder. Edit the Decisions-doc filename and folder-path references if needed. Write a new lean `CLAUDE.md` for the project's specific tech stack.
