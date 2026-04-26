# Zerenn — Features

**Part of the Zerenn Technical Reference.** What's in the game. Player abilities, weapons, enemies, NPCs, shop, save/continue, minimap, room system.

This is the player-facing inventory. Architecture lives in `Zerenn-Architecture.md`. Persistence lives in `Zerenn-Data-Models.md`. The "why" lives in `Zerenn-Decisions.md`.

---

## Game Overview

**The Legend of Zerenn** is a top-down 2D action-adventure in the style of *The Legend of Zelda: A Link to the Past*. Solo-developed by Bald Guy & Company Games. PC platform (keyboard + mouse), Unity engine.

The world is a grid of rooms (18×10 units each, 16:9 aspect). The player explores rooms, fights enemies, collects items, upgrades their class, and unlocks new abilities. Secret rooms exist behind cracked walls (bombable). Buildings have separate interior coordinates.

---

## Player

### Movement

- 4-directional movement (WASD or arrow keys)
- Mouse cursor controls aim direction (sprite faces mouse for ranged attacks)
- Sprint is built into base move speed (no separate sprint mechanic)
- Movement velocity uses Rigidbody2D so collisions respect physics

### Player Classes

The player progresses through four class tiers, picked up via in-world `ClassUpgrade` items:

#### Archer (starting class)
- Bow only (no melee)
- Arrows damage destructibles
- Lowest base health
- No armor

#### Swordsman (tier 1 unlock)
- Unlocks melee — short reach, narrow swing arc
- +1 max heart bonus (one-time, granted on first reach)
- Armor: halves damage of 2+ hits, minimum 1 (1-damage hits unaffected — see Decisions doc)
- Fires `SwordBeam` at full HP

#### Spearman (tier 2 unlock)
- Longer melee reach
- +1 max heart bonus
- Fires `SpearBeam` at full HP (longer range than SwordBeam)

#### Paladin (tier 3 unlock)
- Widest swing arc
- +1 max heart bonus
- Fires `TemplarWave` at full HP (widest sweep, pierces multiple enemies)

**Notes:**
- `SetClass()` cannot downgrade — picking up a Swordsman item as a Paladin is a no-op (Batch 3 fix)
- Heart bonuses use `bonusesAppliedUpTo` tracking to prevent double-counting
- Class progression is permanent (saved to PlayerPrefs as `SavedClassTier`)

### Health System

- Starting max HP: 3 hearts
- Lives: 3 (game over screen on full death)
- Hearts are visual half-hearts (each "heart" = 2 HP)
- Permanent max HP can increase via:
  - Class upgrades (+1 heart per Swordsman/Spearman/Paladin tier)
  - HeartContainer pickups (+1 heart, one-time per instance)
  - GoodAngel encounters (configurable per instance)
  - Shop heart upgrade (+1 heart, 100 rupees, one-time per save)
- Invincibility frames after taking damage (sprite blinks, can't be hit again)
- Death triggers `GameOverUI` — Continue saves persistent unlocks, QuitToMenu returns to MainMenu

### Shield

- Directional block: faces the direction the player is facing
- Held with the Block input
- Blocks projectiles and melee from the front arc
- Does NOT block:
  - AOE damage (ExplosionEffect, FireTrail) — radial damage isn't directional
  - Bomb explosions at the player's feet
- ShieldKnight has its own version of this — see Enemies section

### Melee (Swordsman+)

- Triggered by Attack input
- Class-configurable arc, reach, damage
- `Melee.DoSwing()` coroutine sweeps an invisible hitbox
- `meleeEnabled = false` for Archer
- HashSet-based dedupe prevents same enemy taking multiple hits per swing

### Bow (all classes)

- F key or right-click to fire
- Rapid-fire (cooldown-gated)
- Arrows damage Destructibles (bushes, pots) and Enemies
- Stops on Walls and CrackedWalls (bombs only break cracked walls — see Decisions)

### Class Beams (Swordsman+)

Fires automatically when sword/spear/wave is swung at full HP:
- **SwordBeam** — short range, single-hit, destroys self on enemy hit
- **SpearBeam** — longer range, pierces multiple enemies (HashSet dedupe)
- **TemplarWave** — widest visual, expands as it travels, pierces multiple enemies

All three duplicate ~80% of code (refactor deferred — see Bug-History).

### Sub-Weapons

Cycled with Q (or scroll wheel), used with the SubWeapon input. The cycle order is determined by which sub-weapons have been unlocked.

#### Boomerang
- Stuns IStunnable enemies
- Damages non-stunnable enemies for 1 (BoomShroom, GoblinThief)
- Cuts Destructibles (bushes/pots) for 1 damage and keeps flying — classic Zelda
- Carries Collectibles back to the player (hearts, rupees, arrows, bombs, items)
- Returns to the player; one boomerang at a time
- Blocked by Walls and CrackedWalls
- Blocked by ShieldKnight's directional shield

#### Bombs
- Place a bomb at the player's position
- Fuse timer with blink effect, then explosion
- Bomb at the player's feet damages the player too (ExplosionEffect uses no-source TakeDamage — bypasses shield)
- Bombs damage through walls (classic Zelda — `OverlapCircleAll` doesn't do line-of-sight)
- Bombs break CrackedWalls (the only weapon that does)
- Limited inventory (`maxBombs`)
- "Bomb Bag" item required to carry bombs (otherwise bomb pickups spawn a live bomb at the player's feet — discovery mechanic)

#### Grappling Hook
- Latches onto tagged objects: `GrapplePoint` (pulls player) or `Grappable` (pulls target to player)
- Pulls player to a static GrapplePoint
- Pulls enemies/pickups to the player — pulled enemies become temporarily kinematic during the pull
- Blocked by Walls and CrackedWalls
- Blocked by ShieldKnight's directional shield
- 4-state machine: Flying (outbound), Latched (transitioning), PullPlayer, PullTarget
- `OnDestroy` is the single source of truth for cleanup (restores enemy physics, releases carried items, notifies player)

#### Wand (with optional Book upgrade)
- Fires `FireBolt` projectile
- Without Book: standard fire damage
- With Book: increased damage + leaves a `FireTrail` (lingering damage zone with per-target cooldown)

### Mount/Ram (Horse, optional)

- Mountable horse (M key to mount/dismount)
- Sprite swaps to a mounted horse sprite
- Movement speed multiplier while mounted
- Disables melee while mounted
- Ram damage: contact while mounted damages enemies AND player 1:1 (currently)
- Open design question: damage values may rebalance once mount system is fully exercised

### Buffs (PlayerBuff component, runtime-added)

OrcChief drops a random PlayerBuff on death:

- **Speed** — movement multiplier (timed)
- **Power** — melee damage multiplier (timed)
- **Heal** — instant heal to full
- **Resupply** — instant arrows + bombs refill

Same-type buff applied while one is active refreshes; different-type buffs coexist (Power buff isn't lost when picking up a Heal).

---

## Combat & Damage

### Damage Interfaces

- `IDamageable.TakeDamage(int)` — universal entry point. Implemented by all enemies, Destructible, CrackedWall.
- `IStunnable.Stun(float)` — implemented by most enemies (not Bat, BoomShroom, GoblinThief).
- ShieldKnight has additional `TakeDamage(int, Vector2)` overload that returns bool (true = damaged, false = blocked) plus a public `IsBlockingFrom(Vector2)` query for non-damage effects.

### Hit Feedback

- `HitFlash` component on every enemy: white flash on damage taken
- ShieldKnight's block animation has its own flash (preserved through the audit fix that prevents hit-flash from overwriting block-flash)
- Numeric damage display: not implemented (potential future feature)

### Death

- All enemies have `isDead` guard on TakeDamage and Die (Batch 5/6 audit fix)
- Drops handled by `Dropper` component (configurable drop tables per enemy)
- Most enemies drop rupees + occasional hearts/arrows/bombs

---

## Enemies (14 types)

### Simple

#### Bat
- Flying enemy
- Wander/chase pattern
- One-shot by Boomerang (special case — see Decisions)
- 1 contact damage

#### Slime
- Wander/chase
- Stunnable
- Contact damage
- Drops rupees on death

#### SlimeSplitter
- Like Slime but splits into smaller slimes on death
- 95% duplicate code with Slime (refactor deferred)
- Three sizes: Large → Medium → Small

#### BoomShroom
- Walks toward player
- Self-detonates on Player, Wall, or CrackedWall contact (Batch 5 fix added Wall/CrackedWall)
- Explosion uses `ExplosionEffect` with BoomShroom's tuned radius/damage
- Single damage source (direct damage path was removed in audit)

### Goblins

#### GoblinMaceman
- Melee enemy
- Spin attack telegraph
- Stunnable
- Standard wander/chase

#### GoblinSpearman
- Charge attack with pullback telegraph
- Stunnable
- Pullback uses `rb.MovePosition` (not transform writes — Batch 5 fix)

#### GoblinArcher
- Ranged enemy, fires `EnemyArrow`
- Patrol pattern instead of wander
- Stunnable

#### GoblinThief
- Wander → Sneak → Dash → Steal → Flee
- Steals rupees from player on contact
- If allowed to escape (flee timer expires off-screen), rupees are PERMANENTLY LOST — "you snooze you lose" design intent
- Despawns after flee timeout (no return to wander)
- Not stunnable (boomerang does 1 damage instead — Batch 5 fix)

### Advanced / Mini-bosses

#### SkeletonMage
- Ranged enemy, fires `MagicProjectile`
- Teleports when player gets too close (validated against walls — Batch 6 fix)
- Stunnable

#### ShieldKnight
- Directional block — front arc deflects damage AND stun AND pull
- Boomerang stun, GrapplingHook pull, and projectile damage all check `IsBlockingFrom`
- Block flash visual feedback when damage is deflected
- Stunnable from non-blocking angles
- Most complex single enemy (~350 lines)

#### FlyingSkull
- Flies above terrain
- Pullback → Swoop attack pattern
- Now respects internal walls (Batch 6 fix — was phasing through them)
- Reads room dimensions from RoomManager (single source of truth)

#### Mummy
- Multi-phase: Underground → Burrowing → Aboveground → Spinning → Stunned → Emerging
- Spins to fire MummyProjectiles in a radial pattern
- Burrows underground and re-emerges at a validated position (no wall clipping — Batch 6 fix)
- Collider gated until Emerging scale ≥ 0.5 (no invisible-mummy contact damage)
- Skips contact damage while Stunned (Batch 6 fix)

#### OrcArcher
- Like GoblinArcher but tougher, fires two arrows in spread pattern
- Stunnable

#### OrcChief
- Mini-boss
- Buffs nearby enemies in the SAME ROOM via `EnemyBuff` (room-bounded scan — Batch 6 fix)
- On death, drops a PlayerBuff to the player (random type)
- `OnDestroy` removes ally buffs on still-living enemies (audit fix)
- Players keep ongoing PlayerBuffs of different types when killing OrcChief (Batch 6 fix — was being stripped)

### Enemy Buffs (EnemyBuff component, runtime-added by OrcChief)

- **Fortify** — instant +HP
- **Haste** — movement multiplier
- **Berserk** — damage multiplier

`ReapplyTint()` keeps the buff color visible through stun cycles.

---

## NPCs & Dialogue

### NPC Types

- **NPC** — full NPC with idle animation, facing, range-based prompt, dialogue trigger
- **DialogueTrigger** — minimal version for signs and one-shot dialogue
- **ShopKeeper** — extends NPC with shop callback

### Dialogue System

- Triggered by E key when in range of an NPC/sign/shopkeeper
- `DialogueBox` is a singleton modal overlay
- Typewriter effect (one character at a time)
- Pauses time (`Time.timeScale = 0f`)
- Multi-line support with E to advance
- Restores timeScale on close, then fires `onDialogueComplete` callback (used by ShopKeeper to open the shop)
- Same-frame input race protected via `openFrame` check (prevents the opening E from skipping line 1)

### Shop System

Triggered when ShopKeeper's dialogue closes.

**Items for sale:**
- Arrows ×10 — 20 rupees
- Bombs ×5 — 30 rupees
- Heart Upgrade — 100 rupees, **one-time per save** (persists via `HeartUpgradeBought` PlayerPrefs key)

**Mechanics:**
- Number keys (1, 2, 3) to buy each item
- E or Escape to close shop
- Validates target component, capacity, and rupee count BEFORE deducting (no losing rupees buying max-arrow refill)
- Heart upgrade survives death (key not cleared by `DeleteSave`, only by `DeleteAllData`)
- ShopUI ignores input on opening frame (Batch 3 fix — was opening then immediately closing)

---

## World & Navigation

### Room System

- Each room is 18×10 units (16:9 aspect)
- World is a grid of integer room coordinates
- Camera snaps to rooms on transition (no scrolling between)
- Room transitions:
  - **RoomTransition** — standard between-rooms trigger
  - **BuildingEntrance** — exterior to interior coordinates (~0, 100 range)
  - **SecretTransition** — used for secret rooms behind bombed walls (~-500 range)
- All transitions go through `RoomManager.ChangeRoom` or `TeleportToRoom`
- `isTransitioning` guard prevents double-fire (multi-collider triggers, multi-frame race)
- Room-local projectiles (Boomerang, GrapplingHook) are destroyed on transition (Batch 4 fix)

### Cracked Walls

- Visually distinct walls that block normal movement
- Only break from bomb explosions (ExplosionEffect)
- Beams, arrows, boomerang, grappling hook all stop on them but don't damage them
- Once broken, persists across save/load via per-instance `wallID` PlayerPrefs key (Batch 2 fix)
- Reveals a `revealedArea` GameObject (typically a passage or a transition trigger)

### Destructibles

- Bushes, pots, etc.
- Implement IDamageable
- Drop random items on death (handled by Dropper)
- `isDead` flag prevents double-drop on multi-hit (Batch 2 fix)
- Damaged by all player projectiles, boomerang, melee, AOE, and enemy projectiles

### Pickup Persistence

Permanent pickups (HeartContainer, GoodAngel, ItemPickup, CrackedWall) carry a unique ID set in the Inspector. Once collected/destroyed, the ID is written to PlayerPrefs and the instance won't respawn on scene reload. See Decisions doc for the per-instance ID pattern.

### Minimap

- Tab key to toggle
- Grid of visited rooms (RoomTracker maintains the HashSet)
- Visited state persists across save/load (`VisitedRooms` PlayerPrefs string)
- Refreshes when RoomManager notifies it on room change
- Initial refresh deferred until RoomManager has loaded saved position (Batch 3 fix)
- Hidden when paused, in dialogue, in shop, or on game-over (Batch 3 input guard)

---

## Save & Continue

### Save Triggers

- **Bulk save** at every room transition and pause→quit
- **Inline save** at one-time unlock events (heart upgrade purchase, item unlock, weapon cycle, max HP change, class change)
- **Death save** when player clicks Continue on game-over screen — preserves persistent unlocks, resets run state

See `Zerenn-Data-Models.md` for the full save key map and lifecycle.

### Continue / New Game

- **MainMenu Continue button** — enabled if `HasSave = 1` exists, loads the saved state
- **MainMenu New Game button** — calls `SaveManager.DeleteAllData()` then loads Game scene
- **Game Over Continue** — preserves rupees, max HP, class, item unlocks, heart-upgrade-bought; resets RoomX/RoomY/Lives
- **Pause → Quit to Menu** — saves current state, returns to MainMenu

### Heart Upgrade Persistence

Once bought (100 rupees from shop), `HeartUpgradeBought` survives death. Player keeps the +1 max heart and the shop shows it as sold out on subsequent reloads. Wiped only by `DeleteAllData` on NewGame.

(Pre-fix: this was the heart upgrade exploit — `DeleteSave` wiped the key, letting the player re-buy on every continue.)

---

## UI

### HUD (always visible during gameplay)

- **HealthUI** — heart row showing current/max HP
- **LivesUI** — lives counter
- **RupeeUI** — rupee count
- **ArrowUI** — current/max arrows
- **BombUI** — current/max bombs

### Modals (suspend gameplay)

- **DialogueBox** — typewriter dialogue overlay
- **ShopUI** — shop modal with three items
- **PauseManager** — pause menu (P key) with Save & Quit / Continue / Resume
- **GameOverUI** — Continue / Quit to Menu

### Toggleable

- **MinimapUI** — Tab key toggle, grid view of visited rooms

### Debug (UNITY_EDITOR only)

- **O** — refill all consumables (arrows, bombs, hearts)
- **R** — full reset (calls `DeleteAllData`, reloads scene)
- **T** — cycle player class (Archer → Swordsman → Spearman → Paladin → Archer)

---

## What's Not Built Yet

- Boss encounters
- Dungeon key/lock system
- Environmental puzzles beyond bushes-near-walls
- Audio / music
- Death animation
- Inventory UI (TAB screen showing all items)
- Fire mechanic expansion (FireTrail is the only fire interaction currently)
- Multiple biomes / world expansion
- Gamepad support (keyboard + mouse only currently)

These are roadmap items, not bugs. See `Zerenn-Roadmap.md`.
