# Zerenn — Data Models

**Part of the Zerenn Technical Reference.** Every persisted value, every enum, every interface contract. The "what's saved where and what reads it" doc.

This is the file that prevents save key drift. When adding a new save value, update this doc first, then the code. When debugging "why didn't this persist?", check this doc.

---

## Storage Backend

All persistence uses **Unity PlayerPrefs**. Keys are typed (Int, String) — no JSON, no SQLite, no file I/O. PlayerPrefs is sufficient at this scale (~16 keys + per-instance pickup keys).

**On Windows**, PlayerPrefs is stored at `HKEY_CURRENT_USER\Software\Unity\UnityEditor\<Company>\<ProductName>` in the registry. Production builds use a similar key under the company/product names.

**Trade-offs of PlayerPrefs:**
- Pros: zero setup, atomic writes per `PlayerPrefs.Save()`, type-safe getters, free across all platforms
- Cons: registry-based on Windows (not file-portable), no schema migration system, no encryption, all-or-nothing wipe via `DeleteAll()`

For Zerenn's scope (single-player, no cloud sync, no mod support), PlayerPrefs is the right choice. If the game ever needs cloud saves or modding, this is the layer that gets replaced.

---

## Save Key Map

Every PlayerPrefs key in the codebase. Format:

- **Key** — exact string used in PlayerPrefs.SetInt/GetInt
- **Type** — Int unless noted
- **Written by** — file(s) that call SetInt
- **Read by** — file(s) that call GetInt
- **Cleared by** — which delete path wipes it

### Run State Keys

These reset on game over via `SaveManager.DeleteSave()`. They represent "where the player was in this run."

| Key | Type | Default | Written by | Read by | Cleared by |
|-----|------|---------|------------|---------|------------|
| `RoomX` | Int | 0 | SaveManager.SaveAll | SaveManager.GetSavedRoomX, RoomManager.Start | DeleteSave, DeleteAllData |
| `RoomY` | Int | 0 | SaveManager.SaveAll | SaveManager.GetSavedRoomY, RoomManager.Start | DeleteSave, DeleteAllData |
| `Lives` | Int | 3 | SaveManager.SaveAll | SaveManager.GetSavedLives, PlayerHealth.Start | DeleteSave, DeleteAllData |
| `HasSave` | Int | 0 | SaveManager.SaveAll, GameOverUI.SaveInventory | MainMenuController.Start (continue button), SaveManager.HasSaveData | DeleteSave, DeleteAllData |

`HasSave = 1` means a save file exists. `MainMenuController.Start` reads it to enable/disable the Continue button. The audit added the GameOverUI.SaveInventory write to prevent alt-F4-after-Continue from losing the save.

### Persistent Unlock Keys

These survive death. Wiped only by `SaveManager.DeleteAllData()` (NewGame button or debug FullReset). They represent "what the player has earned."

| Key | Type | Default | Written by | Read by | Cleared by |
|-----|------|---------|------------|---------|------------|
| `SavedRupees` | Int | 0 | SaveManager.SaveAll, GameOverUI.SaveInventory | GameState.Start | DeleteAllData |
| `SavedArrows` | Int | 0 | SaveManager.SaveAll | PlayerController.Start | DeleteAllData |
| `SavedBombs` | Int | 0 | SaveManager.SaveAll | PlayerController.Start | DeleteAllData |
| `SavedMaxHealth` | Int | 3 | SaveManager.SaveAll, GameOverUI.SaveInventory, PlayerHealth.IncreaseMaxHealth, PlayerHealth.DecreaseMaxHealth | PlayerHealth.Start | DeleteAllData |
| `SavedClassTier` | Int | 0 (Archer) | SaveManager.SaveAll, GameOverUI.SaveInventory, PlayerClass.SaveClass | PlayerClass.Start | DeleteAllData |
| `EquippedWeaponIndex` | Int | 0 (first available) | SaveManager.SaveAll, PlayerController.CycleWeapon | PlayerController.Start | DeleteAllData |
| `HeartUpgradeBought` | Int | 0 | ShopUI.BuyHeart | ShopUI.Start | DeleteAllData |
| `VisitedRooms` | String | "" | RoomTracker.SaveVisitedRooms (called by SaveAll) | RoomTracker.LoadVisitedRooms | DeleteAllData |

Notes:
- `SavedClassTier` stores the `ClassTier` enum value: 0=Archer, 1=Swordsman, 2=Spearman, 3=Paladin
- `EquippedWeaponIndex` stores the `SubWeapon` enum value (NOT a list index — fixed in Batch 1)
- `VisitedRooms` is a comma-separated string of `x,y` coordinates; e.g., `"0,0,1,0,1,1,2,1"` means rooms (0,0), (1,0), (1,1), (2,1) have been visited

### Item Unlock Flags

Boolean-as-Int flags (1 = unlocked, 0 = locked). Permanent unlocks. Wiped only by `DeleteAllData`.

| Key | Type | Default | Written by | Read by | Cleared by |
|-----|------|---------|------------|---------|------------|
| `HasBoomerang` | Int (bool) | 0 | SaveManager.SaveAll, PlayerController.UnlockItem, GameOverUI.SaveInventory | PlayerController.Start | DeleteAllData |
| `HasBombs` | Int (bool) | 0 | SaveManager.SaveAll, PlayerController.UnlockItem, GameOverUI.SaveInventory | PlayerController.Start | DeleteAllData |
| `HasGrapple` | Int (bool) | 0 | SaveManager.SaveAll, PlayerController.UnlockItem, GameOverUI.SaveInventory | PlayerController.Start | DeleteAllData |
| `HasWand` | Int (bool) | 0 | SaveManager.SaveAll, PlayerController.UnlockItem, GameOverUI.SaveInventory | PlayerController.Start | DeleteAllData |
| `HasBook` | Int (bool) | 0 | SaveManager.SaveAll, PlayerController.UnlockItem, GameOverUI.SaveInventory | PlayerController.Start | DeleteAllData |

`PlayerController.UnlockItem` writes these inline at unlock time so a Unity crash before the next room transition doesn't lose the unlock. Documented as part of the intentional hybrid save policy (see Decisions doc).

### Pickup Persistence Keys (per-instance)

Dynamically constructed at runtime via `"<prefix>_<id>"` where `<id>` is a serialized string set in the Inspector on each prefab instance.

| Prefix | Set by (component) | Set when | Read by | Cleared by |
|--------|---------------------|----------|---------|------------|
| `Heart_<id>` | HeartContainer | OnTriggerEnter2D after applying max HP | HeartContainer.Start | NOT cleared by DeleteAllData (intentional — could be added) |
| `Angel_<id>` | GoodAngel | When gift is granted | GoodAngel.Start | NOT cleared by DeleteAllData |
| `Pickup_<id>` | ItemPickup | After pickup logic completes | ItemPickup.Start | NOT cleared by DeleteAllData |
| `Wall_<id>` | CrackedWall | When health <= 0 (after bomb damage) | CrackedWall.Start | NOT cleared by DeleteAllData |

**Why these aren't cleared on NewGame:** the per-instance ID system means there's no central registry of which IDs exist. Clearing all `Heart_*` keys would require a registry or a key-prefix scan. For now, NewGame leaves these keys behind. A NewGame after extensive play accumulates dead pickup keys in PlayerPrefs (small storage cost, no functional issue). Future improvement: registry pattern in SaveManager.

**Inspector setup required**: every HeartContainer, GoodAngel, ItemPickup, and CrackedWall prefab instance needs a unique ID string set in its Inspector field. Without an ID, the persistence is dormant and the pickup will respawn on scene reload.

---

## Save Lifecycle

### Bulk Save (`SaveManager.SaveAll`)

Called from:
- `RoomManager.ChangeRoom` and `TeleportToRoom` (every room transition)
- `PauseManager.QuitToMenu` (player quits to main menu)

Writes ALL run-state keys + ALL persistent unlock keys + EquippedWeaponIndex. Single `PlayerPrefs.Save()` call at the end commits to disk.

### Inline Saves (specific keys, in-place)

Triggered by specific events:
- `PlayerController.UnlockItem` writes the corresponding `Has*` flag immediately
- `PlayerController.CycleWeapon` writes `EquippedWeaponIndex` immediately
- `PlayerHealth.IncreaseMaxHealth` and `DecreaseMaxHealth` write `SavedMaxHealth`
- `PlayerClass.SaveClass` (called from upgrade/downgrade) writes `SavedClassTier`
- `ShopUI.BuyHeart` writes `HeartUpgradeBought`
- `HeartContainer/GoodAngel/ItemPickup/CrackedWall` OnTriggerEnter (or equivalent) write their per-instance key

These are intentional — see Decisions doc for the hybrid save policy rationale.

### Death Save (`GameOverUI.SaveInventory`)

When the player clicks Continue on the game-over screen:
1. `SaveManager.DeleteSave()` wipes run state (RoomX, RoomY, Lives, HasSave)
2. `GameOverUI.SaveInventory` writes:
   - `SavedRupees` (current rupee count)
   - `SavedMaxHealth` (preserves heart upgrades)
   - `SavedClassTier` (preserves class progression)
   - All `Has*` item unlock flags
   - `HasSave = 1` (so MainMenu's Continue button stays enabled if the player alt-F4s)
3. Scene reloads (Game scene), state loads from PlayerPrefs

This is why the heart upgrade exploit was possible pre-fix: `DeleteSave()` used to wipe `HeartUpgradeBought` along with run state. Splitting `DeleteSave` (run-state-only) from `DeleteAllData` (everything) closed the exploit.

### Full Wipe (`SaveManager.DeleteAllData`)

Called by:
- `MainMenuController.NewGame` (player clicks New Game)
- `GameController.FullReset` (debug R key, UNITY_EDITOR only)

Both have a fallback to `PlayerPrefs.DeleteAll()` if SaveManager hasn't loaded yet (cold launch into MainMenu scene).

Wipes everything in DeleteSave PLUS:
- All persistent unlock keys
- All `Has*` flags
- `EquippedWeaponIndex`
- `VisitedRooms`
- `HeartUpgradeBought`

Does NOT wipe `Heart_*`, `Angel_*`, `Pickup_*`, `Wall_*` per-instance keys.

---

## Enums

### ClassTier (PlayerClass.cs)

```csharp
public enum ClassTier
{
    Archer = 0,
    Swordsman = 1,
    Spearman = 2,
    Paladin = 3
}
```

Stored as Int via `(int)ClassTier`. Each tier grants:
- **Archer** (0): bow only, no melee. Arrows damage destructibles.
- **Swordsman** (1): unlocks melee, +1 max heart, armor (halves 2+ damage)
- **Spearman** (2): longer melee reach, +1 max heart, SpearBeam
- **Paladin** (3): widest swing arc, +1 max heart, TemplarWave

`SetClass(tier)` is no-op when `tier <= currentClass` (downgrade prevention added in Batch 3). `bonusesAppliedUpTo` tracks which tier's max-HP bonus has already been granted to prevent double-counting.

### SubWeapon (PlayerController.cs)

```csharp
public enum SubWeapon
{
    Boomerang = 0,
    Bombs = 1,
    Grapple = 2,
    Wand = 3
}
```

Stored as Int. Saved value is the enum, not a list index — `EquippedWeaponIndex` survives unlock-order changes between sessions.

`Book` is NOT a SubWeapon. It's a passive upgrade that modifies FireBolt's damage and enables FireTrail. Stored as `HasBook` flag.

### PlayerBuff.BuffType (PlayerBuff.cs)

```csharp
public enum BuffType
{
    Speed,    // Movement speed multiplier (timed)
    Power,    // Melee damage multiplier (timed)
    Heal,     // Instant heal-to-full
    Resupply  // Instant arrows/bombs refill
}
```

Heal and Resupply apply effects at Initialize and immediately Destroy themselves. Speed and Power are timed buffs that restore values OnDestroy.

### EnemyBuff.BuffType (EnemyBuff.cs)

```csharp
public enum BuffType
{
    Fortify,  // +HP (instant)
    Haste,    // Movement multiplier
    Berserk   // Damage multiplier
}
```

Applied by OrcChief to nearby enemies in the same room. Removed when OrcChief dies (via OnDestroy).

### GrapplingHook.HookState (GrapplingHook.cs)

```csharp
private enum HookState
{
    Flying,      // Outbound to target
    PullPlayer,  // Pulling player to a GrapplePoint
    PullTarget   // Pulling enemy/pickup to player
}
```

`HookState.Missed` was removed during audit — it was unreachable (miss path destroyed the hook directly).

### GoblinThief.State (GoblinThief.cs)

```csharp
private enum State
{
    Wander,
    Sneak,
    Dash,
    Flee
}
```

`State.Steal` was removed during audit — the steal action was never a state, just a method call between Dash and Flee. After Flee timeout, the thief despawns (intentional design — see Decisions doc).

---

## Interfaces

### IDamageable

```csharp
public interface IDamageable
{
    void TakeDamage(int amount);
}
```

Implemented by:
- All 14 enemies
- Destructible (bushes, pots)
- CrackedWall
- ShieldKnight (also has overload — see below)

The single-argument overload bypasses ShieldKnight's directional block. Used by AOE damage (ExplosionEffect, FireTrail).

**ShieldKnight extension** (not part of the interface, but a public method on the class):

```csharp
public bool TakeDamage(int amount, Vector2 attackSource)
{
    // Returns true if damage applied, false if blocked
}

public bool IsBlockingFrom(Vector2 source)
{
    // Public query for non-damage effects (Boomerang stun, GrapplingHook pull)
}
```

Directional callers (Arrow, FireBolt, SwordBeam, SpearBeam, TemplarWave, Melee) check for `ShieldKnight` via GetComponent first, call the overload, gate HitFlash on the bool return. Non-damage effects (Boomerang stun, GrapplingHook pull) check `IsBlockingFrom` before applying.

### IStunnable

```csharp
public interface IStunnable
{
    void Stun(float duration);
}
```

Implemented by most enemies (not Bat, BoomShroom, GoblinThief). Boomerang calls this; non-IStunnable enemies fall back to `IDamageable.TakeDamage(1)`.

---

## In-Memory Game State

Not persisted; lives in singleton fields and component fields.

### GameState
- `int rupees` — current rupee count

### RoomManager
- `Vector2 currentRoom` — current room coordinate (saved via SaveAll)
- `bool isTransitioning` — guard against double room transitions
- `float roomWidth, roomHeight` — 18x10 constants

### RoomTracker
- `HashSet<Vector2Int> visitedRooms` — populated from `VisitedRooms` PlayerPrefs string on load

### PlayerController (runtime fields not persisted)
- `bool isMounted, isShooting, isGrappling, isPullingPlayer`
- `bool boomerangOut`
- Various timers (shootCooldown, attackTimer, etc.)

### PlayerHealth (runtime fields not persisted, reloaded each session)
- `int currentHealth` — derived from maxHealth on Start
- `int currentLives` — derived from save on Start

### PlayerClass (runtime fields not persisted)
- `int bonusesAppliedUpTo` — tracks which class tiers' max-HP bonus has been granted

### Pickup-side runtime state
- HeartContainer, GoodAngel, ItemPickup, CrackedWall: each has its own runtime state. Persistence is via the per-instance ID PlayerPrefs key.

---

## World Constants

### Room Dimensions
- `roomWidth = 18f`, `roomHeight = 10f` — set on RoomManager
- 16:9 aspect ratio
- World grid: integer coordinates (room positions multiplied by these constants for world space)

### Player Stats (defaults, configurable in Inspector)
- `maxHealth` starting value: 3
- `maxLives` starting value: 3
- `moveSpeed`: typically 5
- `maxArrows` and `maxBombs`: tunable per design

### Game-Wide
- `Time.timeScale = 0f` while DialogueBox, ShopUI, PauseManager, or GameOverUI is active. Coroutines that need to run during pause use `WaitForSecondsRealtime`.

---

## Common Anti-Patterns to Avoid

When extending the data model:

- **Don't add a save key without updating both `SaveAll` AND `DeleteAllData`.** Drift between the two = either keys that never persist correctly or keys that survive new game inappropriately.
- **Don't write PlayerPrefs from a script that doesn't already do so.** The hybrid save policy is intentional but extending it requires a Decisions doc entry.
- **Don't store enum values as strings.** `(int)MyEnum.Value` is the convention. String keys are for `VisitedRooms` only (variable-length data).
- **Don't add a per-instance ID key without documenting the prefix in this file.** Future audits need to know what keys to expect.
- **Don't bypass SaveManager for run-state writes.** Inline writes are for permanent unlocks only.
