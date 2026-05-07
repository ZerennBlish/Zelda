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

## Step 9 — Session handoff

Session handoffs are written by Opus in the design chat at the end of the session and carried forward as context for the next session. They are not stored as files in the repo.

---

## Cross-Project Note

This Close-Out workflow is project-specific. DFW uses `ai-docs\Close-Out.md`; Brick Headed uses `Docs\Close-Out.md`. The structure is similar (verify-clean → audit → compile → docs → commit → upload) but file paths and verification commands differ. Do not copy this file across projects without adapting paths.
