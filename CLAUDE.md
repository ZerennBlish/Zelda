# CLAUDE.md — The Legend of Zerenn

**Project-specific Claude Code instructions.** Lives at the root of the Zelda repo. Auto-loaded by CC at session start.

For universal cross-project conventions and persona context, see `Docs\About-Me.md`.
For Opus orchestration rules (prompt drafting, audit workflow, session close-out), see `Docs\Opus.md`.
For locked design decisions, see `Docs\Zerenn-Decisions.md`.

---

## Project

- Unity 2D top-down action-adventure (Link to the Past style)
- Room-based world, each room 18×10 units (16:9 aspect ratio)
- PC target, keyboard + mouse, gamepad support later
- Package: com.baldguyandcompany.thelegendofzerenn
- Publisher: Bald Guy & Company Games
- Repo: `C:\Zelda` (desktop) / `D:\Zelda` (laptop). WSL: `/mnt/c/Zelda` (desktop) / `/mnt/d/Zelda` (laptop). Junction `C:\Zelda` → `D:\Zelda` on laptop keeps all paths working.
- Unity MCP bridge (`com.unity.ai.assistant`) for editor operations and verification
- Desktop Commander available for read-only repo inspection from Opus. Claude Code is the sole file editor.

---

## Commands

Unity MCP compile check is the primary verification. No tsc, no jest, no lockfile.

```
Unity MCP compile check    # Must pass after every code change — 0 errors
grep -c "<symbol>" file.cs # Count verification for added/removed symbols
```

Skip compile check for doc-only sessions — it verifies nothing in those cases.

---

## Project-Specific Code Rules

- **New Input System only.** All input routes through `InputManager.Instance` using `UnityEngine.InputSystem`. Never use legacy `UnityEngine.Input`.
- **Standardized input guard set.** Every input-reading script must check: `if (DialogueBox.IsActive || ShopUI.IsActive || PauseManager.IsPaused || GameOverUI.IsActive) return;`
- **Singleton pattern:** null-check + Destroy on duplicate. Six singletons use this: GameState, RoomManager, ShopUI, DialogueBox, MinimapUI, RoomTracker.
- **Same-frame input debounce:** `openFrame = Time.frameCount` pattern (see DialogueBox, ShopUI).
- **One-frame cooldown:** `wasDialogueActive` / `wasShopActive` mirror flags (see BuildingEntrance, ShopKeeper).
- **Root collider check:** `other.transform == other.transform.root` for multi-collider player.
- **`isDead` idempotency guards** on all enemies and destructibles.
- **Debug keys (O, R, T) gated behind `#if UNITY_EDITOR`.** Never ship debug keys.
- **Damage routing:** ShieldKnight directional block → IDamageable → HitFlash. AOE (ExplosionEffect, FireTrail) intentionally bypasses ShieldKnight block.
- **Save system:** bulk save via `SaveManager.SaveAll()` at transitions + inline `PlayerPrefs` for one-time unlocks (HeartContainer, GoodAngel, CrackedWall). Hybrid is intentional.
- **`GameOverUI` saves only persistent inventory after death** — does NOT call `SaveAll()`. Intentional.
- **Per-instance pickup persistence** via Inspector-set IDs (`Heart_<id>`, `Angel_<id>`, `Wall_<id>`).
- **PlayerAnimator:** script-driven sprite indexing into 54-frame sheets (6×9 grid). No Unity Animator.
- **Archer class:** `meleeEnabled = false`. Archer not swinging is correct.
- **Inspector values override code defaults.** When changing a SerializeField default, change the code AND verify the Inspector value via Unity MCP.
- **See `Docs\Unity-MCP-Rules.md` for all MCP safety rules** — tool selection, read limits, crash avoidance, hosts file workaround, verification pattern. Read before any MCP write operation. Key rule: use `Unity_RunCommand` for writes, never `Unity_ManageGameObject` (freezes editor via recursive serialization).
- **MCP create position parameter is world-space, not local.** When creating a GameObject as a child via MCP, set `Transform.localPosition` explicitly via `component_properties` or a follow-up modify call. The default `position` parameter sets world coordinates and ignores parent offset.

---

## Structure

```
Assets/Scripts/            — All gameplay scripts (flat)
Assets/Scripts/Enemies/    — Enemy AI scripts (14 types + projectiles + buffs)
Docs/                      — AI context docs, session handoffs, prompts
```

Key scripts by system:
- **Player:** PlayerController, PlayerHealth, PlayerClass, PlayerAnimator, PlayerShield, Melee, PlayerBuff
- **Combat:** Arrow, Bomb, Boomerang, GrapplingHook, SwordBeam, SpearBeam, TemplarWave, FireBolt, FireTrail, ExplosionEffect
- **World:** RoomManager, RoomTransition, RoomTracker, BuildingEntrance, SecretTransition, CrackedWall, Destructible
- **UI:** HealthUI, LivesUI, RupeeUI, ArrowUI, BombUI, MinimapUI, GameOverUI, PauseManager
- **NPC/Shop:** DialogueBox, DialogueTrigger, NPC, ShopKeeper, ShopUI
- **Core:** GameState, GameController, SaveManager, InputManager, Collectible, Dropper, ItemPickup, HitFlash
- **Pickups:** HeartContainer, GoodAngel, GrapplePoint
- **Interfaces:** IDamageable, IStunnable

---

## Patterns

- `InputManager.Instance.Move` / `.Attack` / `.Interact` for all input
- Singleton: null-check + Destroy on duplicate in Awake()
- Input guard: four-bool check before processing any gameplay input
- Same-frame debounce: `openFrame = Time.frameCount` at open, reject input on same frame
- One-frame cooldown: `wasDialogueActive` mirror flag, set in LateUpdate, checked in Update
- Pickup persistence: `PlayerPrefs.GetInt("Heart_" + id)` pattern
- Damage: `IDamageable.TakeDamage(int)` + `HitFlash` component on all enemies
- Enemy drops: `Dropper` component with weighted loot table

---

## Do NOT

- Use `UnityEngine.Input` (legacy) — always `InputManager.Instance`
- Use Unity Animator — PlayerAnimator is script-driven
- Skip the input guard set on any new input-reading script
- Use MCP to bypass edit-compile-test for script logic — MCP write is for editor wiring only
- Create extra files or abstractions for hypothetical future needs
- Remove or modify code outside the scope of the current task
- Add `using` directives that aren't needed
- Trust your own confirmation — always show actual lines changed
- Add comments that just restate what the code does
- Recommend merging or splitting files unless explicitly asked
- Repeat a failing approach more than twice — if an MCP operation or code edit fails with the same or similar error twice, try a fundamentally different approach (read underlying files directly, use a different tool, change strategy). STOP and report what was tried rather than spending more than 5 minutes on the same failing path.

---

## Reference Docs

Project-specific reference docs in `Docs\`:
- `Docs\Zerenn-Architecture.md` — systems, singletons, data flow
- `Docs\Zerenn-Features.md` — player-facing feature descriptions
- `Docs\Zerenn-Data-Models.md` — PlayerPrefs keys, enums, interfaces
- `Docs\Zerenn-Bug-History.md` — audit findings + root causes
- `Docs\Zerenn-Decisions.md` — locked design decisions with rationale
- `Docs\Zerenn-Project-Setup.md` — paths, repo, build profiles
- `Docs\Zerenn-Roadmap.md` — what's built, what's next, deferred work
- `Docs\Zerenn-Stability-Playbook.md` — working rules that prevent failure modes

Read relevant docs when working on related systems, not for every task.

For universal rules and persona context, see `Docs\About-Me.md`.
For Opus orchestration manual, see `Docs\Opus.md`.

---

## When Compacting

Preserve: list of modified files, current compile status, any errors, and the task goal.
