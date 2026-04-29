# What Makes Zerenn Stable — The Working Rules

**Purpose:** Capture the rules that prevent specific failure modes in Legend of Zerenn development. Forked from `Brick-Headed-Stability-Playbook.md` (which forked from DFW's playbook) and customized for this project's architecture. Each rule exists because something went wrong once and the rule was the fix.

---

## The Real Reason This Works

Same principle as DFW and Brick Headed: stability isn't luck or talent or working harder. It's a small set of deliberate rules, each preventing a specific failure mode, applied consistently. The compounding effect is why a Unity project with 68 scripts, 14 enemy types, a full audit cycle, and ~8,300 lines of code is clean and playable.

The rules fall into five categories: **role separation, verification, documentation, build hygiene, recovery.** Each section below covers one or more of those.

---

## 1. Role Separation

Only Claude Code edits files. Everyone else is read-only.

### Why this matters

Multi-AI workflows fail when two AIs both believe they have write authority. They produce conflicting edits, overwrite each other's work, and create merge hell. By giving exactly one AI write access, the others become specialists at what they're good at without competing for the same job.

### The roles in Zerenn

| Role | Who | Access |
|------|-----|--------|
| Project Lead | Zerenn | Final say |
| Architect / planner | Opus (Claude.ai chat) | Read-only via Desktop Commander |
| Implementation | Claude Code | Sole writer; plus Unity MCP for editor operations |
| Primary auditor | Codex (ChatGPT) | Read-only |
| Secondary auditor | Gemini | Read-only |

### Unity MCP bridge — read AND write

The Unity MCP bridge (`com.unity.ai.assistant`) extends Claude Code's reach into the live Unity Editor: scene hierarchy, components, Inspector values, layers, tags, GameObjects, and scene management. Unlike Brick Headed (which uses MCP read-only), Zerenn uses MCP for both inspection and editor operations.

**What MCP is used for:**
- **Verification:** Read Inspector values before/after changes. Confirm GameObjects exist. Check console for errors. Compile check via `CompilationPipeline.RequestScriptCompilation`.
- **Editor wiring:** Create GameObjects, add components, set serialized fields, parent objects in hierarchy, configure UI layout. This replaces the manual "click through the editor" steps that Zerenn used to do by hand.
- **Scene inspection:** Find objects, read component state, audit layer/tag/physics configuration.

**What MCP is NOT for:**
- Runtime state injection during Play Mode
- Bypassing the edit-compile-test cycle for script logic (script changes still go through file writes)
- Modifying things without verification afterward

The bridge requires manual Accept in Project Settings → AI → Unity MCP Server on first connection each fresh session. If MCP commands fail with "no connection," that's the first thing to check.

---

## 2. Verification

Every Claude Code prompt must include a verification step. Required.

### What verification looks like in Zerenn

- **Unity MCP compile check.** `Unity_RunCommand` running `CompilationPipeline.RequestScriptCompilation` with `CleanBuildCache` flag. Confirm 0 errors.
- **Grep counts.** "After this change, `grep -c 'IsActive' /mnt/c/Zelda/Assets/Scripts/ShopUI.cs` should return exactly 4."
- **Inspector verification.** Use Unity MCP to read a SerializeField's runtime value and confirm it matches expected. Flags Inspector-override mismatches before they bite.
- **Expected output.** "After the edit, the file should contain exactly one method named Y."

If verification fails, the prompt instructs Claude Code to **STOP and report** rather than continue patching. This prevents cascade — the failure mode where Claude Code tries to fix a problem its own change caused, makes it worse, and so on.

### Why this is the most important prompt discipline

Without verification, Zerenn becomes the only feedback loop. That means every change rides on him noticing whether it worked, which is unsustainable across three projects. Verification offloads the feedback loop to the AI and only escalates real ambiguity to Zerenn.

### What's NOT applicable from DFW

- `npx tsc --noEmit` — TypeScript check, doesn't apply to C#
- `npx jest` — JS test runner; Zerenn has no formal test suite (relies on Unity Play Mode testing)

The underlying principle (verify before shipping) maps to: every Zerenn prompt that touches code must include a Unity MCP compile check, minimum.

---

## 3. Anchor to Grep'able Strings, Use `@` File References

Two related rules about how prompts reference code.

### Grep'able strings, not line numbers

Line numbers shift between when Opus reads a file and when Claude Code runs the prompt. A prompt that says "edit line 47" is wrong by the time Claude Code reads the file if anything else has changed. Anchor to unique strings instead.

```
// BAD
"Update the guard at line 22 in PauseManager.cs"

// GOOD
"In @Assets/Scripts/PauseManager.cs, find the existing
`if (DialogueBox.IsActive || ShopUI.IsActive || GameOverUI.IsActive) return;`
guard in Update(). Add `|| MinimapUI.IsVisible` to the condition."
```

The good version anchors to:
- The full guard condition — unique, grep'able
- The file path with `@` prefix — Claude Code auto-reads
- The method name (`Update()`) as a landmark

All three are stable across edits. The line number isn't.

### `@path/to/file.cs` for file references

Claude Code natively understands `@`-prefixed file paths and auto-reads them before responding. Don't make Claude Code infer that "Assets/Scripts/Foo.cs" means "go read this file."

Both rules together: prompts reference files via `@path` and locations within files via grep'able strings. Line numbers appear nowhere except as soft verification ("after the edit, the new method should appear around line ~165").

---

## 4. Plan Mode for Multi-File Features

For features touching 5+ files or spanning 2+ sessions, use Plan Mode to separate exploration from implementation.

### The pattern

Plan Mode is a Claude Code mode where it reads files and answers questions without making changes. The recommended workflow has four phases:

1. **Explore.** Plan Mode. "Read @Assets/Scripts/PlayerController.cs and @Assets/Scripts/PlayerHealth.cs and @Assets/Scripts/SaveManager.cs. Understand how the save system flows."
2. **Plan.** Still Plan Mode. "I want to add a dungeon key/lock system. What files need to change? What's the data flow? Create a written plan."
3. **Implement.** Switch to Normal Mode. Execute the plan, verifying against it.
4. **Commit.** Descriptive message, push.

Plan Mode adds overhead. For tasks where the scope is clear and the fix is small (typo, log line, variable rename), skip it. **Plan Mode pays off when the change touches multiple files, when the approach is uncertain, or when the code is unfamiliar territory.**

### Why this matters

Without Plan Mode for big features, Claude Code may produce a plausible implementation that solves the wrong problem — or solves the right problem in a way that conflicts with existing patterns. Plan Mode lets Zerenn review the approach before any code lands. Cheaper to fix a plan than to revert a half-done implementation.

### Zerenn-specific application

For features that touch the standardized input guard set (five UI states that suspend gameplay), the save system (SaveManager.SaveAll routing), or the player subsystem (PlayerController + PlayerHealth + PlayerClass + PlayerAnimator + PlayerShield + Melee), Plan Mode is especially valuable. These systems have documented invariants that are easy to violate without an upfront read.

---

## 5. Reference Existing Patterns

When adding code that follows an established pattern, the prompt must cite the canonical example.

### Zerenn's established patterns

- **Singleton pattern:** null-check + Destroy on duplicate (see GameState, RoomManager, ShopUI, DialogueBox, MinimapUI, RoomTracker)
- **Standardized input guard set:** `if (DialogueBox.IsActive || ShopUI.IsActive || PauseManager.IsPaused || GameOverUI.IsActive) return;`
- **Damage routing:** ShieldKnight check → IDamageable check → HitFlash (see CONVENTIONS.md)
- **Enemy state machine:** enum-based states, Rigidbody2D movement, Dropper on death, stun support via IStunnable
- **Same-frame input debounce:** `openFrame = Time.frameCount` pattern (see DialogueBox, ShopUI)
- **One-frame cooldown:** `wasDialogueActive` / `wasShopActive` mirror flags (see BuildingEntrance, ShopKeeper)
- **Pickup persistence:** Inspector-set ID + PlayerPrefs (`Heart_<id>`, `Angel_<id>`, `Wall_<id>`)
- **Root collider check:** `other.transform == other.transform.root` for multi-collider player

### Why this matters

Without pattern references, Claude Code can:
- Invent a new singleton pattern when six scripts already use a standardized one
- Use legacy `Input.GetKeyDown` instead of `InputManager.Instance`
- Skip the standardized input guard set on a new interactable
- Implement pickup persistence differently from HeartContainer/GoodAngel

The prompt should say "follow the HeartContainer pickup persistence pattern" — not describe the pattern from scratch.

---

## 6. Root Causes, Not Symptoms

Never suppress a warning or error without understanding why it exists.

### Zerenn audit examples

- **Debug keys in shipped builds (Batch 1 P1).** The symptom was "R key wipes save data." The root cause was debug keys bound unconditionally — not just R, but O and T too. Fix: `#if UNITY_EDITOR` on all three, not just R.
- **Shop opens then closes instantly (Batch 3 P1).** The symptom was "shop flickers." The root cause was same-frame input bleed — the E-press that closed dialogue was read by ShopUI in the same frame. Fix: `openFrame = Time.frameCount` debounce. Not "add a delay."
- **Heart upgrade infinite grind (Batch 1 P1).** The symptom was "max HP keeps going up." The root cause was the save/delete split — DeleteSave wiped the purchase flag but SaveInventory preserved the upgraded HP. Fix: restructure save split so DeleteSave only wipes run state, not persistent purchases.

### The rule

When a bug appears, ask "why does this happen?" not "how do I make it stop?" Suppressing (try/catch around the crash, adding a null check without knowing why it's null) creates time bombs. The fix should address the actual cause, documented in Bug-History.md with the reasoning.

---

## 7. Build Hygiene

Rules that keep the project buildable and the repo clean.

1. **Pull before starting work.** Sync to latest before any edits.
2. **Push before switching machines.** Never leave work only on one machine.
3. **Commit frequently.** Small, focused commits with descriptive messages.
4. **`git config core.autocrlf true` on every Windows machine.** Kills CRLF warnings.
5. **Unity `.gitignore`** from GitHub's official template. Library/, Temp/, Logs/ excluded.
6. **Debug keys gated behind `#if UNITY_EDITOR`.** O, R, T keys never ship to builds.
7. **New Input System only.** `UnityEngine.InputSystem` via InputManager. Never legacy `UnityEngine.Input`.
8. **No `&&` chaining in PowerShell.** One command per code block.

### Inspector overrides code defaults

When a `SerializeField` has a default value in code but a different value set in the Inspector, **the Inspector wins.** Always. Changing the code default does nothing if the Inspector value is set.

The rule: when changing a SerializeField default, change the code AND verify (or change) the Inspector value via Unity MCP. If only changing the Inspector value, flag it as Inspector-only — don't waste a prompt on code that Unity overrides.

---

## 8. The Cascade Rule

If a session cascades — each fix creates new fixes — **revert immediately**. Don't attempt one more fix.

### Why this is hard to follow

The instinct mid-cascade is "I'm so close, one more change will fix it." That instinct is wrong. By the time a session is cascading, the original change has tangled with downstream effects, and "one more fix" is solving problems your fixes created, not the original problem.

### Zerenn audit example

The full six-batch audit was designed to prevent cascading: findings were triaged by severity (P1/P2/P3), grouped into related fix prompts (Group A/B/C), and applied in order. Each group was verified before the next landed. If any group had cascaded, the instruction was revert that group, not stack more fixes.

The rule: once a session starts cascading, every subsequent fix has a higher chance of being a fix-of-a-fix-of-a-fix. **Revert is cheap. Cascading is expensive.**

---

## 9. PowerShell / WSL Separation

Zerenn's daily driver is PowerShell. WSL exists for Claude Code only. They never cross.

### The specific rules

- **PowerShell:** Unity Editor, git, project knowledge sync (Set-Clipboard / Copy-Item)
- **WSL:** Claude Code only
- Never run git in PowerShell while Claude Code works in WSL on the same repo
- Never run Unity from WSL
- No `&&` chaining in PowerShell — one command per block

### Why the separation matters

WSL and Windows have subtle differences in line endings, file permissions, and environment variables. Running git from both creates subtle corruption (line ending flips, permission shifts) that doesn't show up locally but breaks remote builds.

`git config core.autocrlf true` on Windows mitigates the line-ending issue specifically, but the broader principle stands: don't trust cross-environment tool outputs.

---

## 10. The Backup Discipline

Four backups: desktop, laptop, GitHub, USB.

- **Desktop:** primary working copy (`C:\Zelda\`)
- **Laptop:** if desktop dies mid-project
- **GitHub:** off-site, version-controlled, accessible from anywhere (private repo)
- **USB:** offline, immune to cloud account compromise

---

## 11. The Honesty Standard

When uncertain, say "I'm not sure" plainly. If a cause could be A, B, or C, list them as quick lines — don't hedge with formal phrasing to mask uncertainty.

Hedging-as-dishonesty is a documented Opus failure mode. The honest version is shorter and more useful: "Three possible causes — state race, missing guard, or stale cache. I'd check the guard first."

When Zerenn pushes back on an audit finding, he's usually right. Verify before disagreeing. During the six-batch audit, ~40% of auditor findings were invalid — precision in triage is more valuable than diplomacy.

---

## What's Different from DFW and Brick Headed

| Rule | DFW | Brick Headed | Zerenn |
|------|-----|--------------|--------|
| Lockfile rule | npm/Yarn specific | N/A | N/A |
| Compile check | `npx tsc --noEmit` | Unity MCP compile | Unity MCP compile |
| Test suite | `npx jest` | Play Mode + phone | Play Mode only (PC target) |
| MCP bridge | N/A | Read-only | Read AND write |
| Input system | N/A (web/native) | New Input System | New Input System via InputManager |
| File architecture | Flat modules | Partial classes | Flat scripts + Enemies subfolder |
| Build target | Android (EAS) | Android (Unity) | PC (keyboard + mouse) |

---

## What's Already Strong in Zerenn

- Full six-batch audit complete (100+ findings, all P1/P2 fixed, P3s triaged)
- Seven canonical docs in `Docs/` — Architecture, Bug-History, Decisions, Data-Models, Features, Project-Setup, Roadmap
- Multi-AI audit pattern (Codex + Gemini + Claude Code) is proven
- Unity MCP bridge gives live editor inspection AND write access — most capable of the three projects
- Standardized input guard set applied uniformly across all input-reading scripts
- Singleton pattern consistent across all six singletons
- Same-frame input debounce and one-frame cooldown patterns documented and applied
- Save system architecture (bulk save + inline save hybrid) is clean and documented
- Custom animation system (PlayerAnimator) avoids Unity Animator complexity entirely

---

## Current Gaps

1. **MCP write operations are new.** First session with editor write access. Need to establish verification patterns for MCP-created GameObjects (did the component attach? Is the field wired?).
2. **No formal session numbering yet.** Brick Headed and DFW have session handoff docs in `Docs/Sessions/`. Zerenn should adopt the same pattern.
3. **PlayerController is 656 lines.** Deferred split from audit. Works fine but is the largest single file and hardest to navigate for Claude Code.
4. **No boss encounters or dungeon system.** These are the next major features and will be the first real test of Plan Mode in this project.
5. **No audio.** Entire audio layer is unbuilt.

---

## The Meta-Rule

**Stability comes from making one rule for each thing that's gone wrong, and keeping every rule.** Not from being more careful, smarter, or working harder. Specific rules, applied consistently, that each prevent a specific failure.

Zerenn inherits DFW's stability foundation via Brick Headed. The Zerenn-specific extensions (MCP read+write, standardized input guards, six-batch audit cycle, seven-doc reference system) are additions, not replacements. Don't lose any rule. Don't bypass for "just this one quick fix."

DFW is the canonical reference. When a new failure mode appears in any project, capture the rule in DFW's playbook first, then port to Brick Headed and Zerenn.
