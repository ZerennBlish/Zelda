# The Legend of Zerenn

A 2D top-down action-adventure game built in Unity, inspired by classic Zelda titles (A Link to the Past, Link's Awakening). Room-based exploration with progression gating through items, abilities, and a class upgrade system.

## About

Legend of Zerenn is a solo-developed game built as both a creative project and a learning vehicle for Unity and C#. The long-term vision ties into the novel *"Eric and the Littles: The Fourfold Crown"* — a middle-grade fantasy about four young cousins with magical abilities.

## Gameplay Overview

The player explores a room-based world, fighting enemies, finding items, and unlocking new abilities that open up new areas and strategies. Combat rewards positioning and strategy over button mashing — different enemies require different approaches.

### Class System

The player progresses through four class tiers, each changing melee style and beam abilities while the bow remains always available:

| Class | Melee | Beam | Bonus |
|-------|-------|------|-------|
| **Archer** | None (bow-only) | None | Starting class |
| **Swordsman** | Wide arc (120°) | Sword beam | Armor (half damage) |
| **Spearman** | Narrow thrust (30°), long reach | Piercing laser | +1 max heart |
| **Paladin** | Massive sweep (180°) | Expanding wave (pierces) | +1 max heart |

Class upgrades are found through exploration — specific items (golden armor, golden spear, Templar banner) trigger each tier change.

### Combat

- **Bow** — Always available, rapid-fire capable. Primary weapon for Archer class. Arrows damage enemies, destructibles, and bushes.
- **Melee** — Invisible hitbox system. Visual attack comes from sprite sheet frames, damage comes from a sweeping collision check. Unlocked at Swordsman tier.
- **Sword Beam** — Fired at full health. Each class has a unique beam: standard projectile (Swordsman), piercing laser (Spearman), expanding wave (Paladin).
- **Shield** — Directional blocking. Hold to raise shield in your facing direction. Blocks attacks within a configurable arc.

### Sub-Weapons

Sub-weapons use an active item slot system — one key cycles between unlocked items, one key uses the active item:

| Item | Function | Progression Gate |
|------|----------|-----------------|
| **Boomerang** | Stuns enemies, collects distant pickups, kills Bats in one hit | Found in world |
| **Bomb Bag** | Place bombs that explode after a fuse. Destroys cracked walls, damages all nearby entities | Found in world |
| **Grappling Hook** | Fires a hook that pulls the player to grapple points/objects, or pulls enemies/pickups to the player | Found in world |
| **Wand** | Fires a fire bolt projectile | Found in world |
| **Book** | Upgrades the Wand — fire bolts gain +1 damage and leave a damaging fire trail | Found in world |

### Enemies

The game features 13+ enemy types, each requiring different strategies:

**Basic**
- **Slime** — Wander/chase. Simple but persistent.
- **SlimeSplitter** — Large → Medium → Small. Splits on death. Only drops loot at smallest size.
- **Bat** — Wander/chase flyer. Killed in one hit by boomerang.

**Goblins**
- **Goblin Maceman** — Chases, then wind-up spin attack with drift.
- **Goblin Spearman** — Pullback telegraph, then high-speed charge. Hits walls and staggers.
- **Goblin Archer** — Keeps distance, flees if you close in, fires arrows.
- **Goblin Thief** — Sneaks up, dashes in, steals rupees, then flees. Kill to recover stolen loot.

**Advanced**
- **Shield Knight** — Has a directional shield. Must be hit from behind or stunned first.
- **Skeleton Mage** — Ranged caster. Teleports away when you get close.
- **Flying Skull** — Aerial. Pulls back, then swoops. Stun with boomerang.
- **BoomShroom** — Explodes on contact or when hit. Punishes reckless melee.
- **Mummy** — Spins and fires projectile spirals. Burrows underground and repositions.
- **Orc Archer** — Patrols, fires dual spread-shot arrows. Flees at close range.
- **Orc Chief** — Mini-boss. Heavy swing attack with telegraph. Buffs all nearby enemies on aggro (Haste/Fortify/Regen). Killing the Chief removes enemy buffs and grants the player a random buff.

### World Design

- **Room-based** — Each room is 18×10 units (one screen). Camera snaps to room boundaries.
- **Doors/Transitions** — Walk through edges or interact with entrances to move between rooms.
- **Secret Rooms** — Bomb cracked walls to reveal hidden areas with teleport triggers.
- **Building Entrances** — Press E to enter/exit buildings, teleporting to a target room.
- **Grapple Points** — Dedicated grapple targets for traversal across gaps.
- **Destructibles** — Bushes, pots, etc. Destroyed by melee, arrows, bombs, or fire. May drop loot.

## Architecture

### Core Patterns

- **IDamageable** — Interface for universal damage routing. All enemies implement it.
- **IStunnable** — Interface for boomerang stun interactions.
- **State Machines** — Complex enemies use enum-based state machines (Wander → Chase → Attack → Recovery, etc.).
- **Singletons** — RoomManager, SaveManager, GameState for global access.
- **Tags** — Player, Enemy, Wall, Destructible, CrackedWall, Pickup, GrapplePoint, Grappable.

### Key Scripts

| Script | Purpose |
|--------|---------|
| `PlayerController.cs` | Movement, input, weapon usage, mount system, active item slot |
| `PlayerClass.cs` | Class tier system, melee config per class, beam prefab assignment |
| `PlayerAnimator.cs` | Sprite-sheet-driven animation (54 frames per class, 6×9 grid) |
| `PlayerHealth.cs` | Health, lives, invincibility frames, armor reduction, game over |
| `PlayerShield.cs` | Directional shield blocking |
| `Melee.cs` | Sweep-based hitbox, sword beam firing at full HP |
| `RoomManager.cs` | Room transitions, camera snapping, player repositioning |
| `SaveManager.cs` | PlayerPrefs-based save/load for room, lives, inventory |
| `GameState.cs` | Rupee tracking, singleton |
| `GameController.cs` | Debug controls (O = refill + unlock all, T = class upgrade, R = full reset) |

### Animation System

Custom sprite-driven animation that bypasses Unity's Animator. Each class has a 54-frame sprite sheet (6 columns × 9 rows):

```
Row 0-2:  Idle    (Down, Right, Up) — 6 frames each
Row 3-5:  Walk    (Down, Right, Up) — 6 frames each
Row 6-8:  Attack  (Down, Right, Up) — 6 frames each
```

`PlayerAnimator.cs` mathematically indexes into the sheet based on current state, direction, and class. Left-facing is handled via `spriteRenderer.flipX`.

### Save System

Uses `PlayerPrefs` to persist:
- Room position and lives
- Rupees, arrows, bombs
- Item unlock flags (boomerang, bombs, grapple, wand, book)
- Max health
- Class tier
- Equipped weapon index

Save triggers on room transitions and pause menu quit. Full reset available via debug key (R).

## Controls

| Action | Keyboard | Mouse | Controller |
|--------|----------|-------|------------|
| Move | WASD / Arrows | — | Left Stick |
| Aim | — | Mouse position | — |
| Melee | Space | Left Click | X |
| Bow | F (hold) | Middle Click | RB (hold) |
| Sub-weapon | — | Right Click | Y |
| Cycle weapon | I | Scroll Wheel | — |
| Shield | Left Shift | — | B |
| Interact | E | — | — |
| Mount/Dismount | M | — | Back |
| Pause | P | — | Start |

### Debug Keys

| Key | Action |
|-----|--------|
| O | Refill health/arrows/bombs, unlock all items, +50 rupees |
| T | Upgrade to next class tier |
| R | Full reset (delete all save data, reload scene) |

## Setup

1. Clone the repository
2. Open in Unity (2D project)
3. Open the `Game` scene
4. Hit Play

The project uses no external packages — everything is built with Unity's built-in systems.

## Design Philosophy

- **Items unlock world design**, not just combat options. Bombs open secret rooms. Boomerang enables remote collection and stun combos. Grappling hook creates traversal puzzles with risk/reward.
- **Enemies require different strategies.** Shield Knights force positioning. Flyers need ranged attacks or boomerang stuns. BoomShrooms punish mindless melee. Thieves demand fast reactions.
- **Teach through play.** Game mechanics are discovered through natural gameplay rather than tutorials.
- **Progression over power creep.** New abilities open new areas and strategies, not just bigger numbers.
- **Risk/reward everywhere.** The grappling hook pulls enemies to you. Mounting gives speed but costs combat options. Orc Chiefs buff nearby enemies but drop player buffs on death.

## Tech Stack

- **Engine:** Unity (2D)
- **Language:** C#
- **Target:** Desktop (keyboard + mouse, controller supported)

## License

This is a personal project. All code and assets are original work unless otherwise noted.