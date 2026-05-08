# Close-Out — The Legend of Zerenn

**Audience:** Opus only. Claude Code does NOT auto-load this file.

This is the operations workflow Opus runs at the end of every Zerenn session. Lives outside `CLAUDE.md` so the workflow content doesn't compete for Claude Code's attention at session start.

---

## Step 1 — Verify nothing is uncommitted or unpushed

```powershell
git status
```

```powershell
git log --oneline origin/main..HEAD
```

If `git status` shows modifications, commit them. If `git log` shows local-only commits, push them. Both must be clean before proceeding.

---

## Step 2 — Focused audit (if code changed this session)

Run all three auditors on files touched this session. Scope: only files changed, not a full codebase pass.

- **Codex** — see `AGENTS.md` for prompt template
- **Claude Code** — read-only audit prompt
- **Gemini** — see `GEMINI.md` for prompt template

Route findings through Opus for severity triage. ~40% of findings are typically invalid. Group real fixes into A/B/C groups, one prompt per group.

Skip this step for doc-only sessions or any session that didn't touch source code.

---

## Step 3 — Unity MCP compile check (if code changed)

Verify via Unity MCP that the project compiles with 0 errors. Not required for doc-only sessions.

---

## Step 4 — Update docs

Update any of these that changed this session:

- `CLAUDE.md` — if rules, structure, or workflow changed
- `Docs\About-Me.md` — only if universal cross-project rules changed (changes propagate to DFW and Brick Headed)
- `Docs\Opus.md` — if drafting/audit/close-out rules changed
- `Docs\Zerenn-Decisions.md` — if locked design decisions changed
- `Docs\Zerenn-Architecture.md`, `Zerenn-Features.md`, `Zerenn-Data-Models.md`, `Zerenn-Bug-History.md`, `Zerenn-Project-Setup.md` — whichever reference docs are affected
- `Docs\Zerenn-Stability-Playbook.md` — if new failure modes or working rules emerged
- `AGENTS.md` / `GEMINI.md` — if audit rules or project invariants changed
- `Docs\Zerenn-Roadmap.md` — if tasks completed or priorities shifted
- `Docs\Close-Out.md` (this file) — if the close-out workflow itself changed

---

## Step 5 — Commit and push

```powershell
git add .
```

```powershell
git commit -m "Session NN: <summary>"
```

```powershell
git push
```

Separate commands — PowerShell doesn't `&&`-chain.

---

## Step 6 — Final push verification

```powershell
git status
```

```powershell
git log --oneline origin/main..HEAD
```

Both must return empty. If not, fix before proceeding.

---

## Step 7 — Flat copy to staging folder

Run the flat copy script to stage files for Claude.ai project knowledge upload:

```powershell
.\copy-for-claude.ps1
```

The script copies repo-root config files and all `Docs\*.md` to the upload staging folder. Scripts are NOT staged — Opus reads live scripts via Desktop Commander on demand. **Keep the script in sync with the actual file list** — when a doc is added, removed, or renamed, update `copy-for-claude.ps1` too.

---

## Step 8 — Upload to Claude.ai

Drag-and-drop all files from the staging folder into the Claude.ai project knowledge panel. Replace existing files.

---

## Step 8b — Final commit

Any doc changes made after Step 5 (handoff file, doc updates via Desktop Commander) need their own commit:

```powershell
git add .
```

```powershell
git commit -m "Session NN: handoff + doc updates"
```

```powershell
git push
```

---

## Step 9 — Session handoff

Write a handoff doc to `Docs\Sessions\Session-NN-Handoff.md` via Desktop Commander. This is how the next session knows what happened. Handoffs are not optional — without them, the next session starts blind.

The handoff must include:
- What changed (files created, modified, fixed)
- What's next (immediate priorities for next session)
- Known issues (anything unresolved, flagged for follow-up)
- Any decisions made that aren't yet in Zerenn-Decisions.md

Only the last 2 handoffs are staged by `copy-for-claude.ps1`. Older ones stay in the repo but don't upload to project knowledge.

---

## Opening a New Session

At the start of every session, Opus reads the most recent handoff in project knowledge to pick up where the last session left off. Update the "Session Priorities" section in the project instructions to reflect what's being worked on this session.

---

## Cross-Project Note

This Close-Out workflow is project-specific. DFW uses `ai-docs\Close-Out.md`; Brick Headed uses `Docs\Close-Out.md`. The structure is similar (verify-clean → audit → compile → docs → commit → upload) but file paths and verification commands differ. Do not copy this file across projects without adapting paths.
