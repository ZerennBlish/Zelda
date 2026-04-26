# Zerenn — Project Setup

**Part of the Zerenn Technical Reference.** Engine version, paths, repo, workflow, build configuration. The "how to actually work on this project" doc.

If you're sitting down at a fresh machine and want to clone, build, and run Zerenn, this is the file you read.

---

## Project Identity

- **Title:** The Legend of Zerenn
- **Publisher:** Bald Guy & Company Games
- **Package:** `com.baldguyandcompany.thelegendofzerenn`
- **Genre:** Top-down 2D action-adventure (Link to the Past style)
- **Platform:** PC (Windows primary). Keyboard + mouse. Gamepad deferred.
- **Engine:** Unity (new Input System — `UnityEngine.InputSystem`, NEVER legacy `UnityEngine.Input`)

---

## Paths

### Windows (primary dev machine)
- **Project root:** `C:\Zelda\`
- **Scripts:** `C:\Zelda\Assets\Scripts\`
- **Enemies subfolder:** `C:\Zelda\Assets\Scripts\Enemies\`
- **Docs:** `C:\Zelda\Docs\`

### WSL (Claude Code, Codex, Gemini, audit tools)
- **Project root:** `/mnt/c/Zelda/`
- **Scripts:** `/mnt/c/Zelda/Assets/Scripts/`
- **Docs:** `/mnt/c/Zelda/Docs/`

### Linux Mint laptop (secondary dev machine)
- Same WSL-style mount paths via the shared GitHub repo

---

## Per-Machine Settings (do NOT transfer through Git)

These are environment-specific. Set them once on each machine you use:

- **File → Build Profiles → Windows → Switch Platform** (if not already default)
- **Game View aspect ratio: 16:9 Landscape** (matches the 18×10 unit room dimensions)
- Visual Studio / Rider / VS Code as code editor (any works — pick one and stick with it)

---

## Project Settings (DO transfer through Git)

These live in `ProjectSettings/` and are versioned:

- **Company Name:** Bald Guy & Company Games
- **Product Name:** The Legend of Zerenn
- **Package Name:** `com.baldguyandcompany.thelegendofzerenn`
- **Input System:** new (UnityEngine.InputSystem). Legacy Input Manager is disabled.
- **2D physics layers:** Wall, CrackedWall, Player, Enemy, Destructible, Pickup, Projectile

If `ProjectSettings/` ever has merge conflicts (e.g., URP global settings), resolve carefully — this is config that shapes how the entire project compiles.

---

## Repository

- **Host:** GitHub (private)
- **Repo name:** `Zelda` (the directory name; the game's project name is "Legend of Zerenn")
- **Owner:** `ZerennBlish`
- **Branches:** `main` (production / stable) — feature branches as needed
- **Tool:** GitHub Desktop preferred over CLI Git for visual diff/commit/push workflow
- **gitignore:** Unity's official template from GitHub (covers `Library/`, `Temp/`, `obj/`, `Build*/`, `Logs/`, etc.)

### Per-Machine Git Config

Run on every Windows machine to silence the CRLF warning storm:

```
git config core.autocrlf true
```

### Workflow Discipline

1. **Pull before starting work** — sync to latest on whichever branch you're on
2. **Commit frequently** — small, focused commits with descriptive messages
3. **Push before switching machines** — never leave uncommitted work on the desktop and try to continue on the laptop
4. **Audit on a clean repo** — auditors should run on the latest committed state, not the working directory

### Backup Strategy (four locations)

1. **Local machine** — desktop's `C:\Zelda\`
2. **GitHub** — private repo
3. **Second machine** — laptop's clone
4. **USB** — periodic full project snapshot (manual)

---

## Team & Tools

| Role | Who | Tool |
|------|-----|------|
| Project Lead, Designer | Zerenn | Unity Editor |
| Head Developer / Architect | Opus (Claude chat) | claude.ai |
| Coder / Implementer | Claude Code | WSL terminal |
| Auditor (primary) | Codex (ChatGPT) | WSL terminal, READ-ONLY |
| Auditor (secondary) | Gemini | WSL terminal, READ-ONLY |
| Auditor (tertiary) | Claude Code | WSL terminal, READ-ONLY for audit sessions |

**Workflow rules:**
- Auditors are READ-ONLY. They produce findings, never edit files.
- One task per Claude Code prompt for surgical fixes; grouped prompts for related multi-file changes.
- Use "ultrathink" at the start of complex Claude Code prompts.
- Auditor findings route through Opus (triage) → Claude Code (implement). Never apply audit findings directly without triage.
- See `AI-Audit-Workflow.md` for the full workflow.

---

## Code Conventions

### Input System
- Always use `UnityEngine.InputSystem`
- Never use legacy `UnityEngine.Input`
- All bindings live in `InputManager.cs` (singleton, scene-scoped)
- Wrap actions to expose `WasPressedThisFrame` and `Held` properties (avoids leaking InputAction details to consumers)

### Singletons (standardized pattern)

Scene-scoped singletons:

```csharp
void Awake()
{
    if (Instance != null && Instance != this)
    {
        Destroy(gameObject);
        return;
    }
    Instance = this;
}
```

Cross-scene singletons (SaveManager only):

```csharp
void Awake()
{
    if (Instance == null)
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    else
    {
        Destroy(gameObject);
    }
}
```

### Input Guard Set

Every script that reads input checks all four:

```csharp
if (DialogueBox.IsActive || ShopUI.IsActive ||
    PauseManager.IsPaused || GameOverUI.IsActive) return;
```

### Death Idempotency

Every enemy and every destructible:

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
    // drops, effects, Destroy
}
```

### Coroutines Under timeScale=0

- Use `WaitForSecondsRealtime` for coroutines that should run during pause (UI animations, blink timers, dialogue typewriter)
- Use `WaitForSeconds` for gameplay timers that should freeze with pause

### Lifecycle Cleanup

Multi-path cleanup goes in `OnDestroy`, not custom `Die()` methods. Room change, scene unload, hazard, normal death — all funnel through `Destroy()`, which fires `OnDestroy`.

### Persistence

- Bulk saves at room transitions and pause→quit via `SaveManager.SaveAll()`
- Inline saves for one-time unlocks (heart upgrade, item unlocks, max HP, class) write their own keys directly
- Hybrid policy is intentional — see `Zerenn-Decisions.md`

### Documentation

- Every architectural decision goes in `Zerenn-Decisions.md`
- Every audit finding goes in `Zerenn-Bug-History.md`
- Every save key goes in `Zerenn-Data-Models.md`
- Every player-facing system goes in `Zerenn-Features.md`

---

## Build & Run

### From the Editor (development)
- Open `C:\Zelda\` in Unity Hub
- Select the `Game` scene (or `MainMenu` to test the full flow)
- Press Play

### Build Profiles
- File → Build Profiles → Windows
- Standalone Windows 64-bit
- Compression: LZ4 (default is fine)
- Output: `C:\Zelda\Builds\` (in `.gitignore`)

### Debug Keys (UNITY_EDITOR only)
- **O** — refill all consumables
- **R** — full reset (calls `SaveManager.DeleteAllData`, reloads scene)
- **T** — cycle player class

These are wrapped in `#if UNITY_EDITOR` and have no effect in shipped builds.

---

## Hardware

- **Desktop (primary):** Ryzen 5 5600X, 32GB RAM, RTX 4070, Windows
- **Laptop (secondary):** Gateway, Linux Mint, WSL2

The desktop is where most Unity Editor work happens. The laptop is for audits, code review, and Claude Code sessions when away from the desk.

---

## Knowledge Cutoff for Future Maintainers

If you're picking up this project months from now and something doesn't make sense:

1. **Read `Zerenn-Bug-History.md` first** — it lists every audit finding and every documented "why we did it this way" hack
2. **Then read `Zerenn-Decisions.md`** — every design and architectural call is in there with rationale
3. **Then read `Zerenn-Architecture.md`** — for "what calls what"
4. **Then read `Zerenn-Data-Models.md`** — for "what's saved where"
5. **`Zerenn-Features.md` and this file** are reference; consult as needed

If something looks weird and isn't documented, it might be a bug — or it might be a deliberate choice that wasn't captured. Add it to Decisions or Bug-History before "fixing" it.
