# Zerenn — Architecture

**Part of the Zerenn Technical Reference.** How the systems fit together. The "what calls what and why."

This is a structural document. Feature-level "what does the boomerang do" lives in `Zerenn-Features.md`. Persistence "what's saved where" lives in `Zerenn-Data-Models.md`. The "why" behind every choice lives in `Zerenn-Decisions.md`. This file describes the wiring.

---

## Top-Level Layout

The codebase has 68 C# scripts at `Assets/Scripts/` (and `Assets/Scripts/Enemies/` for enemy implementations). Total ~8,000 lines after audit fixes.

Three categories of script:

- **Singletons** — manage global state and persist for the lifetime of a scene (or longer)
- **Components** — attached to GameObjects, do one job (a weapon, an enemy, a UI element)
- **Interfaces** — `IDamageable` and `IStunnable` define the damage/stun contract

---

## Singletons

Five singletons drive the game. Three lifecycle patterns exist depending on what the singleton owns:

### SaveManager (DontDestroyOnLoad)

The only cross-scene singleton. Survives MainMenu → Game transitions because save state is consumed in both scenes (NewGame and ContinueGame both read PlayerPrefs).

- `SaveAll()` — bulk save, called at room transitions and pause→quit
- `DeleteSave()` — wipes run state only (RoomX/Y, Lives, HasSave). Used on game over.
- `DeleteAllData()` — wipes everything including persistent unlocks. Used on NewGame and debug FullReset.
- `CacheReferences()` — populated on first SaveAll, reused for the lifetime of the singleton

Caches references to PlayerController, PlayerHealth, PlayerClass, GameState, RoomTracker, RoomManager. Pre-audit, each SaveAll did 5 FindFirstObjectByType scans. Now: one scan per reference, reused forever.

### Scene-Scoped Singletons (null-check + Destroy on duplicate)

These follow a standardized pattern:

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

Active in the Game scene only. Re-created on scene reload.

- **GameState** — `rupees`, `StealRupees()`, `AddRupees()`, `SubtractRupees()`. Single source of truth for the rupee count. UI reads from it.
- **RoomManager** — current room coordinate, room transitions, room-local projectile cleanup, RoomTracker/MinimapUI notifications. Owns `roomWidth` and `roomHeight` constants (read by FlyingSkull and other scripts that need room dimensions).
- **RoomTracker** — visited rooms HashSet, persists to PlayerPrefs as `VisitedRooms` comma-separated string.
- **DialogueBox** — typewriter dialogue UI, `Show()` / `Close()`, `IsActive` flag, `onDialogueComplete` callback.
- **ShopUI** — shop modal, three buy methods (BuyArrows, BuyBombs, BuyHeart), `Show()` / `Close()`, `IsActive` flag.
- **MinimapUI** — visited-rooms grid, Tab toggle, `RefreshMap()` called from RoomManager after room changes.
- **GameOverUI** — game-over screen, `IsActive` flag, `Continue()` saves inventory before scene reload, `QuitToMenu()` returns to MainMenu.
- **GameController** — debug key handlers (UNITY_EDITOR only).
- **PauseManager** — pause menu, `IsPaused` flag, `QuitToMenu()` saves before scene change.
- **InputManager** — wraps the new Input System actions, exposes `WasPressedThisFrame` and `Held` properties for every binding.

### IsActive Flag Coordination

Five UI states can suspend gameplay:

- `DialogueBox.IsActive`
- `ShopUI.IsActive`
- `PauseManager.IsPaused`
- `GameOverUI.IsActive`
- `MinimapUI` (no IsActive — toggle handled in MinimapUI.Update only)

Every script that reads input must check the first four before processing. This is the **standardized input guard set**:

```csharp
if (DialogueBox.IsActive || ShopUI.IsActive || 
    PauseManager.IsPaused || GameOverUI.IsActive) return;
```

Used by: PlayerController, PlayerShield, PlayerAnimator, NPC, DialogueTrigger, ShopKeeper, BuildingEntrance, MinimapUI, GameController. Any new interactable script must follow this pattern.

---

## Component Subsystems

### Player System

**PlayerController** (656 lines, deferred for split — see Bug-History) — the central player script. Owns:

- Movement and aim
- Mount/dismount and ram damage
- Grapple state machine (4 states: Flying, Latched, PullPlayer, PullTarget)
- Sub-weapon cycling and firing (Boomerang, Bombs, Grapple, Wand, Book)
- Arrow shooting + cooldown
- Inventory and item unlocks
- Inline PlayerPrefs writes for one-time unlocks (intentional hybrid pattern)

Companion components on the Player GameObject:

- **PlayerHealth** — HP/max HP/lives, `TakeDamage(int)` and `TakeDamage(int, Vector2)`, `Heal()`, `IncreaseMaxHealth()`, GameOver trigger
- **PlayerClass** — class tier (Archer/Swordsman/Spearman/Paladin), per-class stat config, `SetClass()`, `UpgradeClass()`
- **PlayerAnimator** — custom 54-frame sprite indexing (no Unity Animator), 3 directions × 3 states (Idle/Walk/Attack) × 6 frames per class
- **PlayerShield** — directional blocking, `BlocksAttackFrom(Vector2 source)`
- **PlayerBuff** (added at runtime) — temporary stat modifiers (Speed, Power, Heal, Resupply). `Initialize()` removes existing same-type buff before applying. OnDestroy restore.
- **Melee** (child GameObject) — invisible hitbox sweep, `DoSwing()` coroutine, class-configurable arc/reach/damage. `meleeEnabled` gates Archer's no-melee behavior.

### Save System

**Hybrid save model** (intentional):

- **Bulk saves** at room transitions and pause→quit via `SaveManager.SaveAll()` — covers all save keys at once.
- **Inline saves** for one-time unlocks (heart upgrade, weapon unlock, class upgrade) write their own keys directly. Don't lose a permanent unlock to a Unity crash before the next room change.

**Save key categories** (see Data-Models for full key list):

- **Run state** — RoomX, RoomY, Lives, HasSave. Wiped by DeleteSave on game over.
- **Persistent unlocks** — SavedRupees, SavedArrows, SavedBombs, SavedMaxHealth, SavedClassTier, Has* flags, EquippedWeaponIndex, HeartUpgradeBought, VisitedRooms. Survive death. Wiped only by DeleteAllData on NewGame.
- **Pickup persistence** — `Heart_<id>`, `Angel_<id>`, `Pickup_<id>`, `Wall_<id>`. Per-instance keys via Inspector-set ID. Survive death. NOT cleared by DeleteAllData (could be added later as a registry pattern).

### Combat & Damage

**IDamageable** interface: `void TakeDamage(int amount)`. Universal damage entry point. Implemented by every enemy plus Destructible and CrackedWall.

**IStunnable** interface: `void Stun(float duration)`. Implemented by stunnable enemies (most goblins, slimes, mages, mummy, knights, FlyingSkull, OrcArcher, OrcChief).

**ShieldKnight directional damage**:
- `TakeDamage(int amount)` — bypass shield (used by AOE)
- `TakeDamage(int amount, Vector2 attackSource)` — directional check, returns bool (true = damaged, false = blocked)
- `IsBlockingFrom(Vector2 source)` — public query for non-damage effects (Boomerang stun, GrapplingHook pull check this before applying)

**Damage flow** for a directional attacker (e.g., Arrow):

1. OnTriggerEnter2D fires
2. Check ShieldKnight branch: if present, call `TakeDamage(damage, attackSource)`. Gate HitFlash on the bool return so block-flash isn't overwritten.
3. Otherwise call `IDamageable.TakeDamage(damage)`, fire HitFlash unconditionally.

**Damage flow** for an AOE attacker (e.g., ExplosionEffect):

1. OverlapCircle (the new API, post-deprecation) returns colliders
2. Dedupe by root GameObject via HashSet (multi-collider enemies = one hit)
3. Call `IDamageable.TakeDamage(damage)` — no source, bypasses shield directionally

### Room System

**RoomManager** owns scene-level position. World grid is integer coordinates × `roomWidth` (18) × `roomHeight` (10).

**Room transition flow**:

1. Player triggers a RoomTransition collider (or BuildingEntrance, SecretTransition)
2. Trigger calls `RoomManager.ChangeRoom(direction, spawnOffset)` or `TeleportToRoom(...)`
3. RoomManager checks `isTransitioning` guard (prevents double-fire from multi-collider triggers)
4. `DestroyRoomLocalProjectiles()` cleans up boomerang and grapple
5. Camera + player move to new room
6. `RoomTracker.MarkVisited(currentRoom)` called
7. `MinimapUI.Instance.RefreshMap()` called
8. `SaveAll()` called
9. `isTransitioning` cleared

### NPC & Dialogue System

**Three trigger components**:

- **NPC** — full NPC with idle animation, facing, range-based prompt
- **DialogueTrigger** — minimal version for signs and one-shot dialogue
- **ShopKeeper** — extends NPC with shop callback

All three use the standardized input guard set.

**Dialogue flow**:

1. Player presses E in range of an NPC
2. `DialogueBox.Instance.Show(lines, onComplete)` called
3. DialogueBox sets `IsActive = true` and `Time.timeScale = 0f`
4. `openFrame = Time.frameCount` recorded
5. Update is gated on `Time.frameCount == openFrame` for one frame (prevents the opening E from skipping line 1)
6. Typewriter advances on E press
7. On final E, `Close()` runs:
   - Restore `Time.timeScale = 1f`
   - Set `IsActive = false`
   - Invoke `onComplete` callback
   - For ShopKeeper: callback opens ShopUI, which has its own openFrame guard

### Shop System

**ShopUI** is opened only via ShopKeeper's dialogue callback, never directly. Three purchases:
- Arrows ×10 for 20 rupees
- Bombs ×5 for 30 rupees
- Heart Upgrade for 100 rupees (one-time, persists via `HeartUpgradeBought` PlayerPrefs key)

**Validate-before-charge**: each Buy method confirms target component exists, can accept the item, and player can afford BEFORE deducting rupees.

### Pickup System

Two pickup categories:

- **Collectible** — consumables (hearts, rupees, arrows, bombs). Capacity-aware: walking over a heart at full HP doesn't consume it. Bomb collectible without bomb bag spawns a live bomb (intentional discovery mechanic).
- **ItemPickup** — permanent unlocks (Boomerang, Bombs, Grapple, Wand, Book, ClassUpgrade). Each instance has a unique `pickupID` written to PlayerPrefs as `Pickup_<id>` to prevent respawn on scene reload.

**HeartContainer** and **GoodAngel** also use the unique-ID pattern (`Heart_<id>`, `Angel_<id>`) to prevent infinite max-HP exploits.

### Weapons & Projectiles

Player projectiles:

- **Arrow** — main ranged attack, F or right-click
- **SwordBeam / SpearBeam / TemplarWave** — class-specific beams fired at full HP. Three files duplicate ~80% of code (refactor deferred).
- **FireBolt** — Wand projectile, increased damage with Book unlock, optional FireTrail
- **FireTrail** — persistent damage zone, per-target cooldown via Dictionary
- **Boomerang** — stuns IStunnable, damages non-stunnable for 1, cuts Destructibles, blocked by walls and CrackedWalls
- **Bomb / ExplosionEffect** — fuse timer, radial blast bypasses shield, damages through walls, breaks CrackedWalls
- **GrapplingHook** — 4-state machine (Flying/Latched/PullPlayer/PullTarget), respects ShieldKnight directional block, `OnDestroy` is single source of truth for cleanup

Enemy projectiles:

- **EnemyArrow** — GoblinArcher and OrcArcher
- **MagicProjectile** — SkeletonMage
- **MummyProjectile** — Mummy

All enemy projectiles destroy themselves on Wall, CrackedWall (won't damage), and Destructible (will damage).

### Enemy System (14 types)

All enemies implement IDamageable. Most implement IStunnable. All have `isDead` guard on TakeDamage and Die to prevent double-destruction artifacts.

**Simple enemies**: Bat, Slime, SlimeSplitter, BoomShroom

**Goblins**: GoblinMaceman (melee), GoblinSpearman (charge attack), GoblinArcher (ranged), GoblinThief (steal/flee)

**Advanced enemies**: SkeletonMage (teleport + ranged), ShieldKnight (directional block), FlyingSkull (flying with internal-wall awareness), Mummy (multi-phase: Underground/Burrowing/Aboveground/Spinning/Stunned/Emerging), OrcArcher (ranged), OrcChief (buffs allies via EnemyBuff, drops PlayerBuff on death)

**Common pattern** (deferred to base-class refactor): wander/chase/attack state machine, originalColor cache, Stun() with stunTimer, Die() → Dropper.Drop() + Destroy. ~70% duplication across 14 files.

### Buff System

Two parallel buff systems with the same shape:

- **PlayerBuff** — added to player by OrcChief death. Types: Speed, Power, Heal, Resupply. `Initialize()` removes same-type existing buff before applying. Different-type buffs survive (Power buff isn't lost when picking up a Heal).
- **EnemyBuff** — added to enemies by OrcChief. Types: Fortify, Haste, Berserk. `ReapplyTint()` called after enemy unstun to preserve buff color through stun cycle.

OrcChief on death:
1. `OnDestroy` removes ally buffs from any still-living enemies
2. `Die()` adds a PlayerBuff component to the player (Initialize handles dedupe)

---

## Cleanup Patterns

**OnDestroy is the single source of truth** for projectile cleanup. Multiple destruction paths (room change, scene unload, player death, normal completion) all funnel through Destroy(), which fires OnDestroy. Custom `Die()` methods miss most of these paths.

Key OnDestroy implementations:

- **GrapplingHook.OnDestroy** — restore enemy physics, release carried collectibles, notify player
- **Boomerang.OnDestroy** — release carried collectibles, call BoomerangReturned (single-fire)
- **PlayerBuff.OnDestroy** — restore stat values to baseline
- **OrcChief.OnDestroy** — remove buffs from still-living allies

---

## Coroutine Discipline

Coroutines that should pause with the game use `WaitForSeconds` (default Time.timeScale).

Coroutines that should run regardless of pause use `WaitForSecondsRealtime`:

- **PlayerHealth.InvincibilityFrames** — blink shouldn't freeze when paused
- **Melee.DoSwing** — swing shouldn't get stuck mid-animation if paused
- **DialogueBox typewriter** — UI advances during pause (timeScale=0)

Tutorials, UI fades, and any animation that should run on the pause menu use Realtime. Gameplay timers use the regular version.

---

## Same-Frame Input Race Pattern

Unity Update order is non-deterministic. The same `WasPressedThisFrame` returns true for every reader in a frame.

**Solutions established:**

- **openFrame check** — DialogueBox and ShopUI store `Time.frameCount` on Show, gate Update on `Time.frameCount == openFrame` for one frame. Prevents the opening E from immediately advancing or closing.
- **wasXActive mirror flag** — BuildingEntrance and ShopKeeper store the previous frame's IsActive state. If it was true and is now false, skip one frame. Prevents the closing E from immediately reopening.
- **Root collider check** — All interactables verify `other.transform == other.transform.root` in OnTrigger callbacks. Weapon/shield colliders entering/exiting the trigger zone don't toggle range state.

Apply these patterns to every new interactable script.

---

## isDead Idempotency Pattern

Every enemy and every destructible has an `isDead` flag.

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

Why: `Destroy(gameObject)` defers to end of frame. Two damage sources hitting the same enemy in one frame call `Die()` twice before Destroy completes. Without the guard: double drops, duplicate slime splits, duplicate buffs spawned by OrcChief, etc.

Apply to every new enemy and every destructible.
