# Zerenn — Roadmap

**Part of the Zerenn Technical Reference.** What's built, what's next, what's deferred. The forward-looking doc.

This is a living document. Update it after every significant milestone. For historical detail on what changed in past sessions, see `Zerenn-Bug-History.md` (audit findings) and the Git commit history.

---

## Current State (April 2026)

**Status:** Post-audit, pre-content. Codebase is clean, all P1 audit findings are fixed, game compiles and plays through correctly. Documentation is now in place.

**Lines of code:** ~8,000 across 68 C# scripts (after Door.cs and GameOverManager.cs removed during audit).

**Last shipped milestone:** First Full Codebase Audit (April 2026, six batches across three auditors).

---

## Build Order — Current Status

Numbered foundation milestones, in build order. The first 12 are done; the remaining four are content/polish work.

| # | Milestone | Status |
|---|-----------|--------|
| 1 | Project setup | ✅ Done |
| 2 | Player movement | ✅ Done |
| 3 | Camera system | ✅ Done |
| 4 | Tilemap world | ✅ Done |
| 5 | Basic combat | ✅ Done |
| 6 | Enemy AI | ✅ Done (14 enemies) |
| 7 | Health system | ✅ Done |
| 8 | Inventory system | 🟡 Partial (HUD complete, TAB inventory screen not built) |
| 9 | Interactables | 🟡 Partial (doors, NPCs, shops, pickups done — chests + switches pending) |
| 10 | Dungeon system | ⬜ Not started |
| 11 | UI | 🟡 Partial (HUD + minimap done, inventory screen not built) |
| 12 | Save/load system | ✅ Done (audit-hardened) |
| 13 | Sound effects and music | ⬜ Not started |
| 14 | Multiple areas / dungeons | ⬜ Not started |
| 15 | Boss fights | ⬜ Not started |
| 16 | Polish and optimization | 🟡 Ongoing |

---

## Immediate Next Steps (post-audit, post-docs)

These are small fixes the user noted during full game testing. To do BEFORE moving on to bigger content:

- **Camera stuck at 2.5 zoom minimum** — pre-existing issue, not related to refactor. Investigate camera zoom clamp.
- *(other minor issues to be added)*

---

## Near-Term Roadmap

After the immediate next-step polish lands, in roughly the order I'd tackle them:

### 1. TAB Inventory Screen (build 8)

A full-screen modal showing all items, hearts, rupees, weapon slots. Currently the HUD shows live counts but there's no place to "see your stuff." This unlocks several future features:
- Selecting which sub-weapon is equipped (currently cycle-only)
- Showing quest items, keys, found pieces
- Showing class progression visually

### 2. Sound effects (build 13a)

Hit sfx, swing sfx, pickup chimes, footsteps, ambient room loops. No music yet — sfx alone changes the feel of the game enormously.

Probably ElevenLabs sfx generation for one-off effects, free libraries (FreeSound, Pixabay) for loops.

### 3. Music (build 13b)

Per-area background music. The game has no biomes yet but adding biome-aware music early lets each new area feel distinct from the start.

### 4. Death animation (polish)

Player has no death animation currently — instant transition to game-over screen. Adding a brief death anim makes losses feel weighty instead of abrupt.

### 5. Chests and switches (build 9 completion)

- Treasure chests with key items inside
- Floor switches that toggle doors / paths
- Key/lock pairs (foundation for dungeon system)

### 6. Dungeon system (build 10)

The big one. Multi-room dungeons with:
- Map item that reveals dungeon room layout
- Compass item that shows special-room locations (boss, key, treasure)
- Small keys for locked doors
- Boss key for the boss room
- Dungeon-specific enemies and puzzles
- Boss encounter at the end with a major reward

### 7. First boss encounter (build 15)

Tied to dungeon system. Probably:
- Multi-phase pattern-based fight
- Drops a Heart Container (permanent +1 HP, instance-persisted)
- Drops a key story item that gates progression to the next area

### 8. Biomes / world expansion (build 14)

Currently the world is one connected overworld. Expansion plan:
- Forest / starting overworld
- Cave network (already partially scaffolded with secret rooms)
- Mountain area
- Temple / final dungeon

Each biome gets its own enemy palette and music.

### 9. Environmental puzzles (build 9 completion)

Beyond the bushes-near-walls discovery mechanic:
- Block-pushing puzzles
- Sequential switch puzzles
- Light/dark room puzzles
- Ice / slippery floor mechanic

### 10. Audio polish (ambient + UI)

- Footsteps that change with terrain
- UI click/select sfx
- Menu open/close sfx
- Ambient layer per biome (wind, water, cave drips)

---

## Deferred Architectural Refactors

Real P2 findings from the audit that are too big for the audit-fix workflow. Schedule these BEFORE adding bosses or dungeons (they'll be much harder later):

1. ~~**PlayerController split**~~ ✅ Done (Session 03) — split into PlayerController (167), PlayerWeapons (353), PlayerGrapple (123), PlayerMount (134)
2. ~~**Room transition system**~~ ✅ Done (Session 03) — WorldMapData registry, computed spawnOffset, hard-block validation, root-collider checks, special room filtering
3. ~~**EnemyBase + StunnableEnemy base classes**~~ ✅ Done (Session 04) — two abstract base classes, 14 enemies migrated, -550 lines net
4. ~~**Slime/SlimeSplitter merge**~~ ✅ Done (Session 04) — covered by EnemyBase refactor
5. **Three beams base class (PlayerBeam)** — eliminate ~80% duplication in SwordBeam/SpearBeam/TemplarWave
6. **IDirectionalDamageable interface** — eliminate ShieldKnight branch duplication across 6 weapon scripts
7. **Room-based enemy disable** — off-screen rooms keep running enemy AI; disable to save Update budget at scale
8. **InputManager → InputActionAsset migration** — eliminate 19 parallel action declarations across 5 lifecycle methods
9. **Enter/Stay collision dedup** — extract HandlePlayerContact helper across 8 enemies + PlayerController mount

These compound friction as content scales. None of them are blocking, but skipping them now means rework later. Suggested cadence: tackle one or two between content milestones.

---

## Long-Term Open Questions

Things that aren't on the roadmap yet but might be:

- **Gamepad support** — currently keyboard + mouse only. Foundation is there (`UnityEngine.InputSystem` supports gamepad bindings, half are already added). Full pass would unify the bindings and tune for thumbstick aim.
- **Mount/horse system finalization** — currently exists in code but not fully integrated. Damage values may rebalance, feature surface (when do you find a horse, can you call it back, does it have a mount-only ability?) is unspecified.
- **Save slot system** — currently single save per project. Multi-slot would mean restructuring PlayerPrefs key namespacing or migrating to a real file-based save format.
- **Cloud sync / Steam integration** — not on the radar. Would require replacing PlayerPrefs entirely.
- **Mod support** — not on the radar.

---

## What's NOT on the Roadmap

- Multiplayer (single-player only by design)
- Procedural generation (hand-crafted rooms, classic-Zelda style)
- Voice acting / spoken dialogue (text-based)
- Microtransactions / live service (single-purchase indie title)

---

## Version History

| Version | Date | Notes |
|---------|------|-------|
| Pre-1.0 | Jan-Apr 2026 | Building. No public release. |
| Audit 1 | April 2026 | First full codebase audit. Six batches. ~100 findings, all P1s fixed. |
| Docs 1 | April 2026 | Seven-document Technical Reference established. |

This table will grow as the project releases milestones. Format roughly matches DFW's roadmap: version + date + one-line summary.

---

## Document Index

The seven canonical documents that make up the Zerenn Technical Reference. Read in this order if you're new to the project:

1. **Zerenn-Project-Setup.md** — paths, repo, Unity config, code conventions
2. **Zerenn-Features.md** — what's in the game (player-facing inventory)
3. **Zerenn-Architecture.md** — how systems fit together (structural)
4. **Zerenn-Data-Models.md** — what's persisted, all enums, interface contracts
5. **Zerenn-Decisions.md** — why every design and architectural choice was made
6. **Zerenn-Bug-History.md** — every audit finding (fixed/deferred), known issues, lessons learned
7. **Zerenn-Roadmap.md** — this file

Each document has a single job. Don't duplicate content across them — link instead.
