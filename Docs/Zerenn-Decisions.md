# Zerenn — Decisions

**Part of the Zerenn Technical Reference.** The "why" behind every architectural and design choice. When future-you (or any auditor) asks "why is it like this?", the answer should be here.

This is not a feature list (see `Zerenn-Features.md`) or an architecture diagram (see `Zerenn-Architecture.md`). This document captures *intent* — the design decisions that shaped the code, including ones that look like bugs but are actually deliberate.

---

## Combat Design

### Armor only reduces damage of 2+

Swordsman tier and above gets `Mathf.Max(1, damage / 2)`. Damage of 1 stays at 1 (armor doesn't grant immunity to weak hits). Damage of 2+ is halved with a minimum of 1.

**Why:** big hits feel meaningfully softer, but Swordsman+ isn't immortal against trash mobs. Most enemies deal 1 damage, so armor specifically rewards engaging with bigger threats. The original code comment was wrong (claimed it eliminated 1-damage hits); the code itself is correct as designed. Comment was fixed in Batch 1.

### Cracked walls only break from bombs

Beams (Sword/Spear/Templar/FireBolt), arrows, boomerang, and grappling hook all self-destruct on cracked walls but do NOT call `TakeDamage()` on the wall itself. Only `ExplosionEffect` (from bombs and BoomShrooms) applies damage.

**Why:** preserves the bomb-as-key gating across the world. Without this rule, players could sequence-break by shooting beams or grappling through walls. Bombs are a deliberate-investment item — finding the cracked wall is the puzzle, the bomb is the key.

### Bomb explosion bypasses the shield

`ExplosionEffect` calls the no-source `TakeDamage(int)` overload, not the directional one.

**Why:** a radial blast at the player's feet shouldn't behave like a directional projectile. The shield is for facing your attacker, not for being immune to a bomb in your lap. This applies to all AOE damage (ExplosionEffect, FireTrail).

### Bomb explosion damages through walls (classic Zelda)

`Physics2D.OverlapCircleAll` doesn't do line-of-sight. An enemy on the other side of a thin wall takes damage.

**Why:** matches Zelda 1 behavior. Players sometimes use this strategically (lure enemies to the other side of a wall, bomb the wall). It's a feature, not a bug.

### Boomerang cuts Destructibles (classic Zelda)

Boomerang damages bushes, pots, and other destructibles for 1 damage and keeps flying.

**Why:** matches classic Zelda boomerang behavior. Gives the player a way to break grass remotely without committing to melee range.

### Boomerang damages non-stunnable enemies for 1

Goblin Thief, BoomShroom, and any future non-stunnable enemy take 1 damage from a boomerang hit.

**Why:** the boomerang feeling useless against enemies that "shrugged it off" was a UX gap. Stunnable enemies still get stunned (no damage); non-stunnable take chip damage. Either way the boomerang has an effect.

### ShieldKnight directional block applies to all attacks

Damage, stun (Boomerang), and pull (GrapplingHook) all respect the shield's facing arc. Hit ShieldKnight from the front with anything → blocked.

**Why:** consistency. The shield should be the shield, not "the shield except for these specific effects." A player who learns "attack ShieldKnight from behind" should have that lesson apply universally.

### AOE damage bypasses ShieldKnight directional block

`ExplosionEffect` and `FireTrail` call the no-source `IDamageable.TakeDamage(int)` which skips the directional check.

**Why:** radial damage isn't directional. You can't "face" a fire trail or a bomb at your feet. AOE bypassing the shield is the correct symmetry — the shield blocks projectiles and melee, not environmental hazards.

### Enemy projectiles destroy bushes

EnemyArrow, MagicProjectile, and MummyProjectile all stop on Destructibles and break them.

**Why:** matches player-projectile behavior. Avoids the weird case of an enemy arrow phasing through a bush to hit the player. Also creates emergent gameplay — using bushes as cover sometimes gets the bush destroyed for you, exposing what was behind it.

### Enemy projectiles blocked by CrackedWall

Same files as above also stop on CrackedWalls (won't damage them — bombs only).

**Why:** player can use cracked walls as cover from enemy archers. Reinforces the "cracked walls are walls until you bomb them" rule consistently across all damage sources.

### Mount/ram damages both player and enemy 1:1

`ramDamageToPlayer` and `ramDamageToEnemy` both default to 1.

**Why:** unconfirmed final design call, flagged for revisit. Currently a 1:1 trade so the player can't trivially run over enemies. Open question: should mounted player take less damage to incentivize mount use?

### Bomb collectible without bomb bag spawns a live bomb

Pickup-without-bag instantiates a live bombPrefab at the player's feet and detonates.

**Why:** this is a discovery mechanic. Hitting a bush near a wall causes a bomb to drop and explode, revealing cracked walls behind it. Classic Zelda hint pattern. Documented in Collectible.cs to prevent future "fixes."

### GoblinThief escapes with stolen rupees permanently lost

If the thief flees off-screen and the flee timer expires, the thief despawns. The rupees are NOT refunded. They're gone.

**Why:** "you snooze you lose." The thief is a punishment-by-design encounter. Killing the thief in time is the test; failing the test costs you. Without permanent loss, the thief becomes trivial — just walk away and come back later.

### Permanent pickups persist via unique IDs

HeartContainer, GoodAngel, ItemPickup all carry a serialized `*ID` string. PlayerPrefs stores collected IDs as `Heart_<id>`, `Angel_<id>`, `Pickup_<id>`. CrackedWall uses `Wall_<id>`.

**Why:** prevents respawn-and-re-collect exploits across scene reloads. Without persistence, dying and continuing would re-spawn already-collected hearts/angels and let the player stack max HP infinitely. Also prevents progression softlocks (bombed wall coming back when the player is out of bombs).

---

## Save System

### Save split: run-state vs persistent unlocks

`SaveManager.DeleteSave()` only wipes RoomX/RoomY/Lives/HasSave. `SaveManager.DeleteAllData()` wipes everything (used by NewGame and debug FullReset).

**Why:** dying shouldn't reset minimap progress, heart upgrades, or class tier. Death = restart the run, not the save file. Conflating "death cleanup" with "new game wipe" was the root cause of the heart upgrade exploit.

### Save policy: bulk + incremental

`SaveAll()` is the bulk save at room transitions and quit-to-menu. Individual one-time unlocks (heart upgrade, weapon unlock, class upgrade) write their own keys directly.

**Why:** hybrid model. Keep bulk save cheap and predictable, but don't lose a permanent unlock to a Unity crash before the next room change. Pure-bulk saves are fragile; pure-incremental saves thrash the disk. The hybrid is intentional.

### EquippedWeaponIndex saves enum value, not list index

The PlayerPrefs key stores the SubWeapon enum value, not the index into `unlockedWeapons` list.

**Why:** unlocking a new weapon between sessions shifts the list. Saved index 0 (was Wand) might point to Boomerang after Boomerang gets unlocked. Saving the enum value is stable across unlock state changes.

### CrackedWall persistence via wallID

Same pattern as pickups. Once bombed, stays bombed across save/load.

**Why:** otherwise a player who used their last bomb on a wall, died, and continued would find the wall back and have no way through. Progression softlock prevention.

---

## Input & State Discipline

### Standardized input guard set

Every script that reads input checks `DialogueBox.IsActive || ShopUI.IsActive || PauseManager.IsPaused || GameOverUI.IsActive` before processing.

**Why:** five different UI states can suspend gameplay. Without a standardized check, every new interactable script has to remember all four flags and risks missing one. Pattern is documented and applied uniformly.

### Same-frame input debounce: openFrame check

DialogueBox and ShopUI ignore input on the frame they open via `Time.frameCount` comparison.

**Why:** Unity Update order is non-deterministic. The E-press that closed a dialogue can be read by ShopUI in the same frame (closing the shop the dialogue's callback just opened). The E-press that opened a dialogue can be read by DialogueBox the same frame (skipping the first line of typewriter). Debouncing one frame solves both.

### One-frame cooldown after closing nested UI

BuildingEntrance and ShopKeeper use `wasDialogueActive` / `wasShopActive` mirror flags to skip one frame after the inner UI closes.

**Why:** same root cause as openFrame, opposite direction. Closing a shop with E shouldn't immediately reopen the ShopKeeper's dialogue. Closing a dialogue with E shouldn't immediately teleport the player into the building.

### Multi-collider trigger exit fix

All interactables (NPC, DialogueTrigger, ShopKeeper, BuildingEntrance) check `other.transform == other.transform.root` before flipping `playerInRange`.

**Why:** the player has weapon/shield/effect colliders that enter and exit interaction zones independently. Only the body collider should toggle range state. Without this, attacking near an NPC causes the prompt to flicker.

### Escape no longer quits the game

Removed from GameController in Batch 1.

**Why:** original code quit the application on Escape with no confirmation and no save. Single accidental press = lost progress. Quit lives in the pause menu only.

### Debug keys gated behind UNITY_EDITOR

O (refill all items), R (full reset), T (cycle player class). Inputs and handlers are wrapped in `#if UNITY_EDITOR`.

**Why:** shipping builds had R wiping the entire save (including all PlayerPrefs — audio, resolution, etc.) on a single accidental key press. Editor-only is the only safe configuration.

---

## Lifecycle & Cleanup

### GrapplingHook.OnDestroy is the single source of truth for cleanup

Restores enemy physics, releases carried collectibles, notifies player.

**Why:** room change, player respawn, scene tear-down, normal completion all funnel through `Destroy()`. One place owns cleanup, not scattered branches. Custom `Die()` methods miss the non-death paths.

### BoomerangReturned single-fire via OnDestroy

`CatchBoomerang()` doesn't call it directly anymore.

**Why:** OnDestroy fires on every exit path; explicit call from CatchBoomerang would double-fire. State mutation is idempotent today (just sets a bool to false), but any future side effect (sfx, ammo refund, animation cue) would double-fire silently.

### Boomerang and GrapplingHook destroyed on room change

`RoomManager.ChangeRoom`/`TeleportToRoom` calls `DestroyRoomLocalProjectiles()` before moving the camera.

**Why:** otherwise the boomerang chases the player into the new room and can carry pickups across rooms. Active grapples can pull players or enemies through walls during the transition.

### boomerangOut cleared in PlayerController.ResetActionStates

Defensive reset on respawn, even though the active boomerang's OnDestroy will eventually clear it.

**Why:** covers the edge case where a destruction path doesn't fire OnDestroy. Belt-and-suspenders for player input lockout.

### Coroutines under timeScale=0 use WaitForSecondsRealtime

`InvincibilityFrames`, `Melee.DoSwing`.

**Why:** `WaitForSeconds` freezes when paused, leaving the player stuck in the invincibility blink or mid-swing state when they unpause. `Realtime` ticks regardless of timeScale, so UI animations and effects complete correctly even during pause.

### isDead guard pattern across all enemies

Every enemy's `TakeDamage` and `Die` start with an `isDead` check.

**Why:** `Destroy(gameObject)` defers to end of frame. Two damage sources hitting the same enemy in one frame call `Die()` twice before Destroy completes. Result without guard: double drops, duplicate slime splits, duplicate thief refunds, duplicate explosion FX, duplicate PlayerBuff spawns from OrcChief death.

### EnemyBuff cleanup in OrcChief.OnDestroy

Active ally buffs are removed in OnDestroy, not Die.

**Why:** non-Die destruction paths (scene unload, environmental hazard, room change) leave buffs orphaned on still-living allies. OnDestroy fires from all paths.

---

## Architecture Patterns

### Singleton: null-check + Destroy on duplicate

All scene-scoped UI singletons (DialogueBox, ShopUI, MinimapUI) follow the same pattern: `if (Instance != null && Instance != this) { Destroy(gameObject); return; } Instance = this;`. SaveManager additionally has `DontDestroyOnLoad`.

**Why:** before audit, five different singleton patterns existed across the codebase. Some used null-check + DDOL, some overwrote Instance silently, some didn't have any guard. Standardizing eliminates an entire class of "second-instance silently overwrites" bugs.

### SaveManager caches references

Player/PlayerHealth/PlayerClass/GameState/RoomTracker references are cached and reused, not re-found via `FindFirstObjectByType` every save.

**Why:** room transitions trigger SaveAll. Pre-fix, each transition scanned the scene 5 times for these objects. Cached references = single lookup at first use, reused for the lifetime of SaveManager.

### ShopUI caches references

Same pattern, scoped to PlayerController/PlayerHealth.

**Why:** shop purchases scanned the scene 3x per click. Cached at Show() time, reused per buy.

### Shop validates before charging

Each Buy method confirms the target component exists AND can accept the item AND player can afford it BEFORE deducting rupees.

**Why:** a max-arrows player buying arrows should not lose 20 rupees for nothing. Pre-fix, the buy flow was "deduct rupees, then try to add arrows." Now it's "verify everything, then commit."

### Full-capacity collectibles are not consumed

Walking over a heart at full HP / arrows at max / bombs at max leaves the pickup alone. Rupees always collect.

**Why:** consuming for nothing feels broken. The exception is the bomb collectible without the bomb bag — that intentionally spawns a live bomb (see Combat Design).

### OrcChief PlayerBuff award uses Initialize dedupe

On death, OrcChief just calls `AddComponent<PlayerBuff>().Initialize(...)`. PlayerBuff.Initialize handles same-type dedupe internally.

**Why:** previous code explicitly removed the existing PlayerBuff before adding a new one. If the player had a Power buff with time remaining and the new OrcChief buff was Heal/Resupply (which apply instant effects), the player lost their ongoing buff. New behavior: different-type buffs survive, same-type buffs refresh.

### OrcChief buffs only enemies in current room

`BuffNearbyEnemies()` uses `Physics2D.OverlapBox` bounded to the current room dimensions instead of `FindGameObjectsWithTag`.

**Why:** rooms are 18x10. A 12-unit buff radius from a chief near a room edge could buff enemies in the next room. Player wandering into the next room would find pre-buffed enemies they hadn't earned. Room-bounded scan keeps the buff zone honest.

---

## Open Design Questions

These haven't been finalized yet. Marked here so they don't get lost:

- **Mount/ram damage values** — currently 1:1 player vs enemy, may need rebalancing once mount system is fully exercised
- **Mummy stun-vs-emerging asymmetry** — emerging mummy can be damaged but not stunned. May want to grant full immunity during emerge as a telegraph window, or leave as-is.
- **Beam pierce HashSet edge case** — verify the dedup doesn't break intentional pierce-and-return behavior (if any)
- **Three-beam refactor** — base class deferred until after audit completes (see Bug-History deferred refactors)
- **Room-based enemy disable** — off-screen rooms keep running AI; defer until performance becomes an issue at scale
