# CLAUDE.md — The Legend of Zerenn

**Project-specific Claude Code instructions.** Lives at the root of the Zerenn repo. Claude Code reads this automatically at session start.

---

## Project

- Unity 2D, top-down action-adventure (Zelda: A Link to the Past style)
- Target: PC (Windows), keyboard + mouse, gamepad deferred
- Scripts: `C:\Zelda\Assets\Scripts\` (Windows) / `/mnt/c/Zelda/Assets/Scripts/` (WSL)
- Publisher: Bald Guy & Company Games
- Package: `com.baldguyandcompany.thelegendofzerenn`
- Claude Code launch: `CLAUDE_CODE_EFFORT_LEVEL=xhigh CLAUDE_CODE_MAX_OUTPUT_TOKENS=64000 claude` (xhigh effort default for all sessions, 64K output ceiling)
- Long prompts (multi-part, 100+ lines) live in `Docs\Prompts\` — Opus writes them, Claude Code reads them via WSL path `/mnt/c/Zelda/Docs/Prompts/<filename>.md`. Folder may not exist yet; create it when first long prompt arrives.
- New Input System (`UnityEngine.InputSystem`) ONLY. Never use legacy `UnityEngine.Input`.

---

## Rules — READ THESE BEFORE EVERY TASK

### Universal (apply to every session)

- **One task per prompt.** Never combine tasks. Never touch files not listed in the TASK.
- **Partial edits only.** Do not rewrite entire files unless explicitly told to.
- **Show your work.** After every edit, print the actual changed lines. Do not say "done" without showing the code.
- **Do not infer.** If a line isn't where the prompt says it is, STOP and report. Do not guess or pattern-match.
- **No ambiguous conditionals.** Every step must be correct as written. No "do X unless Y breaks." If uncertainty exists, resolve it before writing the prompt — never inside it.
- **Read-only when auditing.** If the prompt says "audit" or "read only," do NOT edit any files. Triple warning.
- **Cascade rule.** If a session starts cascading (each fix creating new fixes), stop and revert immediately. Do not attempt "one more fix."
- **Build only what was asked.** No extra files, no abstractions for hypothetical future needs, no flexibility added for cases the user didn't ask about. If you find yourself adding "in case we need to..." — stop. (This counters a documented Opus 4.x tendency to over-engineer.)
- **Ship-bias awareness.** Never nudge toward shipping over correctness. "Ship it, the quirk is cosmetic" is the exact shortcut Zerenn exists to avoid.
- **Zerenn is not a code reviewer.** Audits are the AI team's job (Codex, Gemini, Claude Code in audit mode). Never ask him to read a block of code and tell you if it looks right.
- **Trust his bug diagnoses.** If he says it doesn't work, he tested it. Don't ask if he ran the prompt.
- **Inspector overrides code defaults.** If a SerializeField has a default in code but is set differently in the Inspector, the Inspector wins. Flag this, don't silently change code.
- **Destructive operations need explicit confirmation.** Force pushes, hard resets, recursive deletes, dropping branches, amending published commits, `--no-verify` — ask first.

### Project-specific (Zerenn)

- **New Input System ONLY.** `UnityEngine.InputSystem`. Never use legacy `UnityEngine.Input`. Ever.
- **InputManager singleton wraps all input.** Other scripts read `InputManager.Instance.<Action>Pressed` or `<Action>Held`. Don't bypass it.
- **Standardized input guard set.** Every script that reads input checks all four states before processing:
  ```csharp
  if (DialogueBox.IsActive || ShopUI.IsActive ||
      PauseManager.IsPaused || GameOverUI.IsActive) return;
  ```
- **isDead idempotency on every enemy and destructible.** Pattern:
  ```csharp
  private bool isDead = false;
  
  public void TakeDamage(int amount)
  {
      if (isDead) return;
      // ...
  }
  
  void Die()
  {
      if (isDead) return;
      isDead = true;
      // drops, effects, Destroy(gameObject)
  }
  ```
- **Singleton patterns are standardized.** Scene-scoped: null-check + Destroy on duplicate. Cross-scene (SaveManager only): null-check + DontDestroyOnLoad. Don't reinvent.
- **Save split is intentional.** `SaveManager.DeleteSave()` wipes only run state (RoomX, RoomY, Lives, HasSave). `SaveManager.DeleteAllData()` wipes everything for new game. Death uses DeleteSave + GameOverUI.SaveInventory. Don't mix them.
- **Save policy: bulk + inline hybrid.** `SaveManager.SaveAll()` at room transitions and pause→quit. Inline saves for one-time unlocks (heart upgrade, item unlocks, max HP, class). Documented in Decisions doc.
- **Per-instance pickup persistence via Inspector IDs.** HeartContainer, GoodAngel, ItemPickup, CrackedWall each have a serialized `*ID` string. PlayerPrefs key format: `Heart_<id>`, `Angel_<id>`, `Pickup_<id>`, `Wall_<id>`. Each instance needs a unique ID set in the Inspector.
- **Coroutines under timeScale=0 use WaitForSecondsRealtime.** UI animations, blink timers, dialogue typewriter all need to run during pause. Gameplay timers use regular WaitForSeconds.
- **Lifecycle cleanup goes in OnDestroy, not custom Die methods.** Multi-path destruction (room change, scene unload, hazard, normal death) all funnel through Destroy(). Custom Die methods miss most of these paths.
- **Damage routing pattern.** Directional attackers (Arrow, FireBolt, SwordBeam, SpearBeam, TemplarWave, Melee) check ShieldKnight first via `TakeDamage(int, Vector2)` — gate HitFlash on the bool return so block-flash isn't overwritten. AOE attackers (ExplosionEffect, FireTrail) use `IDamageable.TakeDamage(int)` no-source overload — bypasses shield by design.
- **Debug keys (O/R/T) are gated behind `#if UNITY_EDITOR`.** Never accessible in shipped builds. R wipes the entire save including audio/resolution PlayerPrefs — editor-only is the only safe configuration.
- **Cracked walls only break from bombs.** Beams, arrows, boomerang, grappling hook all stop on them but do NOT damage them. Only `ExplosionEffect` calls TakeDamage on CrackedWall. This preserves the bomb-as-key gating across the world.

---

## Code Style

- C# Unity conventions
- Prefer `TryGetComponent` over `GetComponent` where null checks follow
- Never use `&&` in shell commands — PowerShell doesn't support it
- One command per code block in terminal output

---

## Do NOT

- Create HTML, markdown, or handoff documents unless explicitly asked
- Add `using` statements that aren't needed
- Remove or modify code outside the scope of the current task
- Trust your own confirmation — always show the actual lines you changed
- Add comments that just restate what the code does
- Use legacy `UnityEngine.Input` — ever
- Add singleton patterns that don't match the existing two (scene-scoped and SaveManager)
- Bypass the standardized input guard set on input-reading scripts

---

## Reference Docs

The seven canonical Technical Reference docs live in `C:\Zelda\Docs\`:

1. **Zerenn-Project-Setup.md** — paths, repo, Unity config, conventions
2. **Zerenn-Features.md** — what's in the game (player-facing inventory)
3. **Zerenn-Architecture.md** — how systems fit together (singletons, damage flow, room transitions, save split, regression-preventing patterns)
4. **Zerenn-Data-Models.md** — every PlayerPrefs key with read/write/clear sites, all enums, all interfaces
5. **Zerenn-Decisions.md** — every "why we did it this way" call with rationale
6. **Zerenn-Bug-History.md** — every audit finding (P1/P2/P3, fixed/deferred), Known Issues, Lessons Learned
7. **Zerenn-Roadmap.md** — current state, immediate fixes, near-term plan, deferred refactors

Read order for a fresh session: Project-Setup → Features → Architecture → Data-Models → Decisions → Bug-History → Roadmap.

For older tactical reference, see `C:\Zelda\CONVENTIONS.md` — older doc with concrete code patterns (damage routing, enemy state machine pattern, common gotchas). Some of its content is now duplicated in the seven Technical Reference docs, but the file remains useful for quick code-pattern lookups.

The audit workflow doc is at `C:\Zelda\Docs\AI-Audit-Workflow.md` (portable, identical to the kit version, identical across all of Zerenn's projects).

Persistent persona context lives in `C:\Zelda\Docs\About-Me.md` (project-agnostic, identical across all projects).

---

## Audit Workflow

Read `Docs/AI-Audit-Workflow.md` for the full workflow.

Quick rules:
- Three auditors per batch (Codex, Claude Code, Gemini), all READ-ONLY, parallel sessions
- Findings route through Opus for severity triage
- Fix prompts are GROUPED (Group A/B/C), not one prompt per finding
- Group fixes by root cause / files touched / logical category
- Always end fix prompts with explicit verification steps (compile + named test scenarios)

When in audit mode (this session): READ-ONLY. Do not edit files. Produce findings only. Triple warning if you're tempted.

---

## Session Close-Out

### Step 1 — Verify nothing is uncommitted or unpushed

```powershell
git status
git log --oneline origin/main..HEAD
```

If `git status` shows modifications, commit them. If `git log` shows local-only commits, push them. Both must be clean before proceeding. This catches the "other machine had unpushed work" trap that costs hours of debugging.

### Step 2 — Focused audit (if code changed this session)

Run all three auditors simultaneously in WSL (hardware handles parallel execution):
- **Claude Code** — `Docs/Prompts/` audit prompt, read-only
- **Codex** — `Docs/Prompts/` audit prompt, read-only
- **Gemini** — `Docs/Prompts/` audit prompt, read-only, **triple read-only warning required or Gemini will attempt edits**

Scope: only files touched this session. Not a full codebase pass — that's the major audit cycle (last one ran April 2026).

Skip this step for doc-only sessions, audio tuning, or any session that didn't touch core gameplay code.

Route findings through Opus for severity triage. ~40% of findings are typically invalid. Group real fixes into A/B/C groups, one prompt per group.

### Step 3 — Update docs

Update any of these that changed this session:
- `CLAUDE.md` — if rules, file list, or workflow changed
- `Docs/Zerenn-Bug-History.md` — if new findings or fixes happened
- `Docs/Zerenn-Decisions.md` — if design decisions were locked or architecture changed
- `Docs/Zerenn-Roadmap.md` — if dashboard fields are stale or tasks completed
- `Docs/Zerenn-Architecture.md` — if structural changes were made
- `Docs/Zerenn-Data-Models.md` — if save keys, enums, or interfaces changed
- `Docs/Zerenn-Features.md` — if features added/removed
- `Docs/About-Me.md` — only if universal (cross-project) rules changed

### Step 4 — Write session handoff

Create `Docs/Sessions/Session-NN-Handoff.md` capturing: work completed, files modified, audit results, queued items, key decisions, commit history.

Folder may not exist yet — create it when first session handoff is written.

### Step 5 — Commit and push docs

```powershell
git add <changed doc files>
git commit -m "Session NN: close-out docs"
git push
```

### Step 6 — Final push verification

```powershell
git status
git log --oneline origin/main..HEAD
```

Both must return empty. If not, fix before proceeding.

### Step 7 — Flat copy to OneDrive stash

Copy all project knowledge files to the upload staging folder. Run in PowerShell:

```powershell
$source = "C:\Zelda"
$dest = "C:\Users\baldy\OneDrive\Desktop\BaldGuy&CompanyGames\Zerenn\Docs"

# Wipe destination so deleted/renamed files don't linger as stale uploads
if (Test-Path $dest) { Remove-Item "$dest\*" -Force -Recurse }
else { New-Item -ItemType Directory -Path $dest -Force | Out-Null }

# Project root files
Copy-Item "$source\CLAUDE.md" -Destination $dest -Force
Copy-Item "$source\CONVENTIONS.md" -Destination $dest -Force
Copy-Item "$source\README.md" -Destination $dest -Force

# Docs folder
Copy-Item "$source\Docs\*.md" -Destination $dest -Force

# Scripts (Assets\Scripts and Assets\Scripts\Enemies)
$scriptDest = "$dest\Scripts"
New-Item -ItemType Directory -Path $scriptDest -Force | Out-Null
New-Item -ItemType Directory -Path "$scriptDest\Enemies" -Force | Out-Null
Copy-Item "$source\Assets\Scripts\*.cs" -Destination $scriptDest -Force
Copy-Item "$source\Assets\Scripts\Enemies\*.cs" -Destination "$scriptDest\Enemies\" -Force

Write-Host "Copied $((Get-ChildItem $dest -Recurse -File).Count) files to $dest"
```

### Step 8 — Upload to Claude.ai

Drag-and-drop all files from the OneDrive stash folder into the Claude.ai project knowledge panel. Replace existing files.
