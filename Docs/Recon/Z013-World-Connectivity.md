# Z-013 - Enemy removal and world connectivity

## Preflight - 2026-09-15

- Desktop checkout `C:/Zelda`, branch `Codex`, revision `9f82368`.
- Fresh fetch of `origin/Codex`: `0 0` ahead/behind.
- Pre-existing untracked recovery files: `Assets/_Recovery/0 (2).unity` and its `.meta`. Preserve all recovery assets and existing content.
- Connected `unity_zelda`: Unity 6000.3.9f1, `C:/Zelda`, Game scene saved/clean, Play stopped.
- MainMenu loads Game; these are the two enabled build scenes. Recovery scenes, the old `ZeldaUnity.unity`, and package/template samples are not part of the playable world.
- Game contains 12 registered room roots: ten overworld screens, one shop, one cave.
- 42 placed EnemyBase instances, including seven outside room parents. No separate enemy-spawner component exists among the live scene's script types. SlimeSplitter is the enemy-creation path in gameplay source; removing its placed instance also removes that spawning source. Enemy scripts and prefab assets remain reusable.
- All 26 ordinary RoomTransition components have reciprocal neighboring destinations. Inspector inspection supersedes an earlier incomplete inventory.
- Shop: one return exit to Room_0_1, no entrance anywhere, no second exit.
- Cave: two returns, but the old Room_0_0 return lands at the south room transition instead of beside the north cracked-wall entrance.
- Reedwater Hollow: west route and the north bush-concealed cave route. Its puzzle must remain intact.

## Implemented

1. Removed all 42 EnemyBase instances, including the seven outside room parents. The 44 deleted GameObjects are those enemies plus two attached shields. All 615 original non-enemy GameObjects survive. Enemy source and prefab assets are unchanged. No separate spawner exists; the placed SlimeSplitter spawning source was removed with the enemies, and remaining droppers reference pickups only.
2. Retained the finite overworld grid and its 26 paired ordinary cardinal exits. Added a 1.5-unit arrival inset, same-frame transition guard, and Rigidbody position/velocity synchronization.
3. Connected Room_0_1 north to shop south, and shop north to Reedwater south, with reciprocal returns. This creates a loop through existing rooms without adding or relocating room roots. Boundary rooms keep two or three exits; four exits everywhere would require additional rooms or wraparound.
4. Repaired both cave round trips, preserving the cracked wall, cuttable bush, angel, fountain, and decorations. The original home connection now uses the cave's south door; Reedwater uses the cave's north door. Both returns land beside the corresponding overworld entrance.
5. Opened a narrow south passage through Reedwater's cliff/backstop. Added a north shop doorway clear of furniture, a matching front entrance in Room_0_1, and doorway/path visuals. All other room content remains.
6. Added perimeter backstops to all 12 rooms, behind the exit triggers, preventing off-screen movement that bypasses transitions. The existing model keeps room roots active; verified each destination root is active and its camera centered.
7. Added WorldMap resumeSpawnOffset data: shop and cave use local `(2,-2)`, clear of the table and fountain. Other rooms resume at their existing center. No save keys or save format changed.

## Scene-write policy gate

Zerenn explicitly approved Pipeline `eval`/`run_script` for this task after the documented Unity_RunCommand mismatch was explained. All scene/asset edits, saving, and runtime verification used that route with primitive-only output. The general future-task policy remains unchanged.

## Results

### Final room exits

All 34 directed routes below passed movement-driven traversal. Coordinates identify ordinary room roots. Cave entrances retain their original puzzle gates; either gate makes the cave reachable when cleared. Both cave exits lead out to usable overworld landings. Reedwater has two ungated exits in addition to its concealed cave route.

| Room | North | South | East | West | Special routes / open-exit count |
| --- | --- | --- | --- | --- | --- |
| Room_-1_-1 | (-1,0) | - | (0,-1) | - | 2 |
| Room_0_-1 | (0,0) | - | (1,-1) | (-1,-1) | 3 |
| Room_1_-1 | (1,0) | - | - | (0,-1) | 2 |
| Room_-1_0 | (-1,1) | (-1,-1) | (0,0) | - | 3 |
| Room_0_0 | (0,1) | (0,-1) | (1,0) | (-1,0) | 4; additional gated north cave entrance |
| Room_1_0 | (1,1) | (1,-1) | (2,0) | (0,0) | 4 |
| Room_-1_1 | - | (-1,0) | (0,1) | - | 2 |
| Room_0_1 | Shop (0,100) | (0,0) | (1,1) | (-1,1) | 4 |
| Room_1_1 | - | (1,0) | - | (0,1) | 2 |
| Room_2_0 | Cave (-200,0), gated | Shop (0,100) | - | (1,0) | 2 open + 1 gated |
| BuildingInterior1 (0,100) | (2,0) | (0,1) | - | - | 2 |
| Room_Secret_0_2 (-200,0) | (2,0) | (0,0) | - | - | 2 |

### Special-route landings

| Route | Destination-local landing |
| --- | --- |
| Room_0_1 north -> shop south | (0,-3.2) |
| Shop south -> Room_0_1 north | (0,3.2) |
| Shop north -> Reedwater south | (0,-3.15) |
| Reedwater south -> shop north | (1.6,3.15) |
| Room_0_0 cracked wall -> cave south | (0,-3.2) |
| Cave south -> Room_0_0 cracked-wall area | (-4,3.6) |
| Reedwater bush -> cave north | (0,3.1) |
| Cave north -> Reedwater bush area | (-4.5,1.5) |

### Verification

- **320 traversal checks passed:** all 34 directed exits tested through virtual New Input System movement, using the real InputManager, PlayerController, physics callbacks, and RoomManager. Checks cover correct destination and landing, no solid/exit-trigger overlap, correct camera, active destination root, visited state, and no immediate bounce. The harness teleports only to a safe setup position before walking the source room route. [Raw results](Z013-Traversal-Results.json).
- **Room connectivity:** every room has at least two distinct destinations; reciprocal graph traversal reaches all 12 rooms after clearing the existing cave gates. Each room's exit approaches share a connected area in a 0.25-unit collision grid checked with the scaled player foot collider. All room perimeters are covered by collision. The uncut bush blocks entry; existing damage methods clear the wall/bush in Play mode for the transition tests. Weapon-specific puzzle combat was not retested.
- **79 resume/containment checks passed:** reload Game from a saved position in all 12 rooms; verify room, safe position, camera, retained exits, and zero enemies. Actual movement against four formerly open border directions stays inside the room. Minimap retains ten overworld cells; the shopkeeper and cave angel remain present after reload. [Raw results](Z013-Resume-Results.json).
- **Saved-scene inspection:** 636 GameObjects, zero enemies including inactive instances, zero missing scripts. All 615 original non-enemy GameObjects preserved; 21 new objects are doorway/path/wall additions and the 12 perimeter-backstop groups. Existing recovery file hashes are unchanged.
- **Compilation/console:** Unity compile completed without errors. No runtime console errors. Unchanged-source compile warnings remain for ExplosionEffect's obsolete ContactFilter2D API and SkeletonMage's unused currentState; Unity AI account connectivity warnings also occurred. No unrelated fixes made.
- **Test cleanup:** all 17 captured gameplay preference keys restored and compared successfully. Virtual devices removed, bindings restored, background/input settings restored. Play-mode scene changes were discarded. Editor returned to stopped, saved Game scene clean, normal startup camera and player position retained.
- **Self-review:** final doorway/camera captures inspected below. No independent audit or user feel review is claimed. No implementation item remains unresolved within Z-013; no commit/push requested.

The first traversal run exposed a prefab override that retained the old cave-arrival X coordinate after reload. Corrected it using SerializedObject and recorded the prefab override, reloaded to verify `(0,3.1)`, then reran the entire traversal suite. The first failed run is not counted as passing evidence.

### Visual evidence

- [Reedwater's south shop passage](Images/Z013-Reedwater-Shop-Passage.png)
- [Shop north and south exits](Images/Z013-Shop-Two-Exits.png)
- [Cave north and south returns](Images/Z013-Cave-Returns.png)

Temporary authoring, inspection, verification scripts, the pre-edit scene/map copies, object manifests, preference snapshot, and failed first-run report remain under `Temp/Z013-World`. Temp is not transferred through Git. Recovery assets and the original scene backup are preserved.
