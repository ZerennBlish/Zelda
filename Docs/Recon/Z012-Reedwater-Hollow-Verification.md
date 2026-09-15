# Z-012 - Reedwater Hollow Verification

**Date:** 2026-09-15. **Scene:** `Assets/Scenes/Game.unity`. **Root:** `Room_2_0`.

## Scope and method

Checked the authored room, its new eastward connection from `Room_1_0`, reused `Room_Secret_0_2`, WorldMap registration, and direct gameplay dependencies. Authoring used the task-specific Pipeline exception approved by Zerenn. Runtime checks used temporary scripts outside Assets; the player moved through the existing New Input System actions. Enemies were disabled only during the geometry test, then tested separately with their normal behavior.

## Recorded results

| Check | Result |
| --- | --- |
| Saved scene reload | Room, prefab overrides, mask, map entry, and both connections retained |
| Original scene objects | All 1,015 original object blocks retained; only two parent child lists and scene roots changed |
| New room missing scripts | 0 |
| Walkable samples connected to entry | 1,472 |
| Isolated clear samples | 0 |
| Closed-edge gaps | 0 |
| Enemy start overlap with solid terrain | 0 |
| WorldMap ordinary `(2,0)` entry | Present |
| RoomTracker visited and MinimapUI cell | Present in Play mode |
| Final console errors / warnings | 0 / 0 |
| Original gameplay preferences restored | Exact comparison passed for snapshot keys and VisitedRooms |

### Traversal - 23 checks passed

1. Walk from Room_1_0 into west entry: player `(28,0)`, camera `(36,0,-10)`.
2. Entry clearing.
3. Path fork.
4. Upper path.
5. North pond bank.
6. East path bend.
7. East pond bank.
8. Southeast bend.
9. South pond bank.
10. Southwest bend.
11. Loop rejoins main path.
12. Water blocks north-to-south movement: stop near local `(3,0.07)`.
13. North cliff blocks movement: stop near local `(0,2.77)`.
14. East cliff blocks movement: stop near local `(7.47,0)`.
15. South cliff blocks movement: stop near local `(-2,-3.92)`.
16. West cliff blocks movement outside entry: stop near local `(-7.47,1.7)`.
17. Normal exit: player `(26,0)` in Room_1_0.
18. Normal re-entry: player `(28,0)` in Room_2_0.
19. Uncut bush blocks the secret trigger: player stops near local `(-4.5,2.09)`.
20. Archer arrow destroys the bush using the existing Destructible/Arrow behavior.
21. Secret entrance reaches original cave: room `(-200,0)`, player `(-3595,-3.1)`.
22. Added cave return reaches Room_2_0: player `(31.5,1.65)`.
23. Return remains stable without immediately triggering the entrance again.

A separate five-check cave test verified settled arrival, return, stability, re-entry, and a second return.

### Combat - 4 checks passed

- Slime moved toward the player and was defeated by arrows; arrow inventory decreased.
- Spearman entered its telegraph/charge sequence.
- Spearman was defeated with arrows in the authored encounter space.
- Player remained alive and controllable.

### Visual and persistence checks

- [Final game-camera overview](Images/Z012-Reedwater-Hollow.png): actual Play mode with the concealed bush and original enemy starts.
- [Original cave and added return](Images/Z012-Cave-Return.png).
- Camera captures exclude Screen Space Overlay UI; minimap integration was checked directly in the live component.
- New terrain uses repeated artwork with matching edge/corner slices. Camera inspection at fractional and integer pixel scales verified the tile sampling fix.
- SpriteMask and clipped perimeter colliders keep the new room inside its footprint. Existing scene content and the first-room secret link are preserved.
- No project-authored test assemblies were found. No player build, full campaign test, independent audit, or human feel review was performed.

The default Git whitespace check reports Unity's empty-value trailing spaces. Documentation checks pass; ignoring only end-of-line blank warnings leaves no diff-check errors. No raw serialized-scene cleanup was performed.
