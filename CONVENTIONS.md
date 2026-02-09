# Legend of Zerenn — Project Conventions

Reference doc for AI assistants and contributors. Read this before writing or modifying any code.

## Project Structure

- **Engine:** Unity 2D
- **Language:** C#
- **Project Path:** `~/Desktop/Game/Zelda`
- **No external packages.** Everything uses Unity built-in systems.

## Tags

| Tag | Used For |
|-----|----------|
| Player | The player GameObject |
| Enemy | All enemy types |
| Wall | Solid walls, collision boundaries |
| Destructible | Bushes, pots — breakable objects with optional loot |
| CrackedWall | Bomb-only breakable walls hiding secrets |
| Pickup | Collectible items (hearts, rupees, arrows, bombs) |
| GrapplePoint | Dedicated grapple hook targets (pull player across) |
| Grappable | Environmental objects the hook latches onto (trees, stumps, posts) |

## Interfaces

### IDamageable
All enemies implement this. Use it for universal damage routing.
```csharp
public interface IDamageable
{
    void TakeDamage(int amount);
}
```

### IStunnable
Enemies that can be stunned by the boomerang implement this.
```csharp
public interface IStunnable
{
    void Stun(float duration);
}
```

## Damage Routing Pattern

When dealing damage to enemies, follow this order:

1. **Check for ShieldKnight first** — it has a special `TakeDamage(damage, position)` for directional shield blocking
2. **Then check IDamageable** — standard damage
3. **Apply HitFlash after damage** — `GetComponent<HitFlash>()?.Flash()`
4. **Boomerang is special** — stuns via IStunnable instead of damaging (except Bats, which die in one hit)

```csharp
// Standard damage pattern used by Arrow, SwordBeam, FireBolt, etc.
ShieldKnight knight = other.GetComponent<ShieldKnight>();
if (knight != null)
{
    knight.TakeDamage(damage, transform.position);
    HitFlash flash = other.GetComponent<HitFlash>();
    if (flash != null) flash.Flash();
    Destroy(gameObject);
    return;
}

IDamageable damageable = other.GetComponent<IDamageable>();
if (damageable != null)
{
    damageable.TakeDamage(damage);
    HitFlash flash = other.GetComponent<HitFlash>();
    if (flash != null) flash.Flash();
    Destroy(gameObject);
    return;
}
```

## Enemy Design Pattern

All complex enemies follow this structure:

- **Enum-based state machine** (e.g., `Wander, Chase, Attack, Recovery, Stunned`)
- **Rigidbody2D** for movement (`rb.linearVelocity`)
- **SpriteRenderer** for facing direction (`flipX`) and stun color feedback
- **Dropper component** for loot drops on death — call `dropper.Drop()` in `Die()`
- **Stun implementation** — set state to Stunned, zero velocity, tint with stunColor, countdown timer, restore originalColor on exit
- **Contact damage** — handled in `OnCollisionEnter2D` / `OnCollisionStay2D`, check for stunned state first
- **GoblinThief** — special enemy that steals rupees from the player and returns them on death

```csharp
// Standard enemy Die() pattern
void Die()
{
    Dropper dropper = GetComponent<Dropper>();
    if (dropper != null)
    {
        dropper.Drop();
    }
    Destroy(gameObject);
}
```

## Melee System

- **Melee is invisible.** The visual attack comes from sprite sheet animation frames. The actual damage comes from `Melee.cs` sweeping an invisible hitbox through an arc.
- **Archer class has no melee** — `meleeEnabled = false`
- **Sword beam fires at full HP** — `Melee.cs` checks `playerHealth.currentHealth >= playerHealth.maxHealth`
- Each class has its own beam prefab: SwordBeam, SpearBeam, TemplarWave
- SpearBeam and TemplarWave pierce through enemies (no Destroy on hit). SwordBeam does not.

## Class System

Four tiers managed by `PlayerClass.cs`:

| Class | Melee Arc | Reach | Damage | Beam |
|-------|-----------|-------|--------|------|
| Archer | disabled | — | — | none |
| Swordsman | 120° | 0.7 | 2 | SwordBeam |
| Spearman | 30° | 1.2 | 3 | SpearBeam (pierces) |
| Paladin | 180° | 0.9 | 4 | TemplarWave (pierces + grows) |

- Swordsman gains armor (half damage taken)
- Spearman and Paladin each gain +1 max heart on upgrade
- Class upgrades are triggered by ItemPickup with `ItemType.ClassUpgrade`

## Animation System

**No Unity Animator.** `PlayerAnimator.cs` uses script-driven sprite indexing.

Each class has a 54-frame sprite sheet (6 columns × 9 rows):

```
Row 0 (0-5):   Idle Down
Row 1 (6-11):  Idle Right
Row 2 (12-17): Idle Up
Row 3 (18-23): Walk Down
Row 4 (24-29): Walk Right
Row 5 (30-35): Walk Up
Row 6 (36-41): Attack Down
Row 7 (42-47): Attack Right
Row 8 (48-53): Attack Up
```

- Left-facing is handled by `spriteRenderer.flipX = true` on the Right row
- State offset: Idle=0, Walk=18, Attack=36
- Direction offset: Down=0, Right=6, Up=12
- Frame index = stateOffset + directionOffset + (currentFrame % 6)
- Archer attack animation triggers on bow fire (`IsShooting()`), all other classes trigger on melee swing (`IsSwinging()`)

## Controls

### Movement
- **WASD / Arrow keys / Left stick** — move player
- **Mouse aim** — facing direction follows mouse cursor

### Combat
- **Space / Left Click / X button** — melee attack (blocked while mounted)
- **F / Middle Click / RB (hold)** — fire arrows (hold for rapid fire, blocked while mounted)
- **Right Click / Y button** — use active sub-weapon (blocked while mounted)
- **Scroll wheel / I key** — cycle sub-weapons

### Interaction
- **E** — talk to NPCs, advance dialogue, interact with shop, enter buildings
- **1 / 2 / 3** — purchase items while shop is open
- **Escape** — close shop

### System
- **P / Start (JoystickButton7)** — pause/unpause
- **M / Back (JoystickButton6)** — mount/dismount

## Sub-Weapon System

Active item slot — one key cycles, one key uses:

- **Scroll wheel / I key** cycles between unlocked sub-weapons
- **Right Click / Y button** uses the active weapon
- Weapon list rebuilds when a new item is unlocked (`RebuildWeaponList()`)
- Order is always: Boomerang → Bombs → Grapple → Wand (based on unlock order)
- Equipped index is saved/loaded via PlayerPrefs

## Mount System

- **M / Back button** toggles mount on/off
- Mounted speed is 2.5x normal (`mountedSpeedMultiplier`)
- **Ram damage** — collision with enemies deals 1 damage to enemy and 1 to player, with 0.5s cooldown
- **No combat while mounted** — melee, arrows, and sub-weapons are all blocked
- Player sprite swaps to `horseSprite` while mounted

## Shop System

- **ShopKeeper.cs** — NPC that opens dialogue, then launches ShopUI on dialogue completion
- **ShopUI.cs** — purchase panel with number key selection
- Items: Arrows x10 (20 rupees), Bombs x5 (30 rupees), Heart Upgrade (100 rupees, one-time)
- `ShopUI.IsActive` blocks player input (same pattern as `DialogueBox.IsActive`)

## Room System

- **Room size:** 18 units wide × 10 units tall
- **One room = one screen.** Camera snaps to room center on transition.
- **Room coordinates** are grid-based: room (1, 0) has its center at world position (18, 0)
- **RoomManager.Instance** handles all transitions
- **Door/RoomTransition:** `direction` = which room to move to (relative), `spawnOffset` = where player lands (relative to room center)
- **BuildingEntrance:** `targetRoom` = absolute room coordinates, `spawnOffset` = relative to room center
- **SecretTransition:** same as BuildingEntrance, used for bombed-wall secret areas

## Save System

**PlayerPrefs only.** No JSON, no ScriptableObjects for saves.

Saved values:
- `RoomX`, `RoomY` — current room position
- `Lives` — current lives
- `HasSave` — flag for continue button
- `SavedRupees`, `SavedArrows`, `SavedBombs` — inventory counts
- `SavedMaxHealth` — max heart containers
- `SavedClassTier` — class enum as int
- `HasBoomerang`, `HasBombs`, `HasGrapple`, `HasWand`, `HasBook` — item unlock flags (0/1)
- `EquippedWeaponIndex` — currently selected sub-weapon slot

Save triggers:
- Room transitions (automatic)
- Pause menu quit
- Game over screen

## Singletons

- `RoomManager.Instance` — room transitions, camera control
- `SaveManager.Instance` — save/load via PlayerPrefs, DontDestroyOnLoad
- `GameState.Instance` — rupee count, rupee UI
- `DialogueBox.Instance` — typewriter dialogue box, `DialogueBox.IsActive` for state checks
- `ShopUI.Instance` — shop purchase interface, `ShopUI.IsActive` for state checks
- `PauseManager.IsPaused` — static read-only property, blocks player input when true

## UI Components

- `HealthUI` — heart icons, dynamically adds/removes based on maxHealth
- `LivesUI` — "x 3" style lives counter
- `ArrowUI` — arrow count text
- `BombUI` — bomb count text
- `RupeeUI` — rupee count text
- `ShopUI` — shop purchase panel (arrows, bombs, heart upgrade)

## Buff System

`PlayerBuff.cs` applies temporary or instant effects to the player:

| Buff | Effect | Duration | Tint |
|------|--------|----------|------|
| Speed | 1.5x movement speed | 15s | Yellow |
| Power | 2x melee damage | 15s | Red |
| Heal | Full health restore | Instant | — |
| Resupply | Full arrows + bombs refill | Instant | — |

## Collectibles vs Item Pickups

- **Collectible.cs** — drops from enemies/destructibles (Heart, Rupee, Arrow, Bomb). Bobs in place, collected on player contact.
- **ItemPickup.cs** — one-time world items that unlock abilities (Boomerang, Bombs, Grapple, Wand, Book, ClassUpgrade).

## Dialogue System

- **DialogueBox** is the only dialogue system. `DialogManager` was removed.
- Show dialogue: `DialogueBox.Instance.Show(lines)`
- Check if dialogue is open: `DialogueBox.IsActive`
- Block player input by checking `DialogueBox.IsActive` at top of `Update()`
- NPCs and triggers both use this same pattern (see `DialogueTrigger.cs`, `NPC.cs`)
- Typewriter effect with `charDelay`, press E to skip or advance lines
- `onDialogueComplete` callback for chained interactions (e.g. shop flow)

## Coding Style

- Use singletons (`GameState.Instance`, `DialogueBox.Instance`, etc.) instead of `FindFirstObjectByType<T>()` when available
- Use `FindFirstObjectByType<T>()` only when no singleton exists (not the deprecated `FindObjectOfType`)
- Use `rb.linearVelocity` (not the deprecated `rb.velocity`)
- Prefer `CompareTag("X")` over `tag == "X"`
- Keep scripts focused — one responsibility per MonoBehaviour
- Comments should explain *why*, not *what*
- Use `[Header("Section")]` to organize Inspector fields
- Debug controls go in `GameController.cs`

## Debug Controls

| Key | Action |
|-----|--------|
| O | Refill health/arrows/bombs, unlock all items, +50 rupees |
| T | Upgrade to next class tier |
| R | Full reset (delete all PlayerPrefs, reload scene) |