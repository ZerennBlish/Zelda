# Zelda Tracked Items

**Owner:** Codex on `Codex`. **Last reconciled:** Session 09 transfer status, 2026-09-18; gameplay verification remains Session 08.

This is the current open-work queue. The [roadmap](Zerenn-Roadmap.md) retains milestone plans; [decisions](Zerenn-Decisions.md) retain design rationale; handoffs link here instead of maintaining competing task lists.

## Next task

Z-013 is complete: all 42 placed enemies removed, all 12 playable rooms have at least two usable exits, the shop loop and cave returns work, and all 34 directed transitions passed Play-mode checks. No further task is assigned. [Session 08](Sessions/Session-08-Handoff.md) records the implementation; [Session 09](Sessions/Session-09-Handoff.md) owns laptop continuation. Session 07's work is committed in `9f82368`; Session 08's work and evidence are committed in `d829be6`. The desktop working tree was clean before Session 09's documentation update, and the local origin/Codex reference matches HEAD; remote state was not refreshed. Older delivery notes describing these changes as uncommitted are historical. The historical zoom report remains Z-001.

## Status and maintenance

- **Confirmed:** reproduced or established against current source/state; include evidence.
- **Needs verification:** reported previously, with current status not established.
- **Planned:** requested or recorded work whose implementation has not been verified complete.
- **Blocked:** name the specific missing decision or dependency.
- **Deferred:** retained for later consideration, not an active assignment.

Use stable `Z-NNN` IDs; do not renumber or reuse an ID. Record the source, next check, and any dependencies. New items enter a tier immediately. Record completion evidence in the handoff or commit before removing a finished item. Next unused ID: `Z-014`.

## Tier 1 - Correctness and prerequisites

### Z-001 - Camera zoom minimum

- **Status:** Needs verification; no current severity assigned.
- **Source:** [Session 04](Sessions/Session-04-Handoff.md), What's Next and Known Issues.
- **Report:** Camera zoom appeared stuck at a minimum of 2.5.
- **Next check:** Reproduce the zoom report with the recovered scene and trace the clamp/input path. Camera startup now works; live edit-mode inspection confirmed orthographic size 5. The initial source search found no gameplay zoom implementation, so the old minimum-of-2.5 report is not yet explained.

### Z-002 - Enemy rotation behavior

- **Status:** Needs verification; no current severity assigned.
- **Source:** [Session 04](Sessions/Session-04-Handoff.md), Known Issues.
- **Report:** Enemy rotation looked wrong despite Freeze Rotation being enabled.
- **Next check:** Identify the affected enemy and reproduce before attributing it to physics or AI.

### Z-003 - Animation that does not stop

- **Status:** Needs verification; affected object and trigger are not recorded.
- **Source:** [Session 04](Sessions/Session-04-Handoff.md), Known Issues.
- **Next check:** Establish the object, action, and reproduction sequence before choosing a file to change.

### Z-004 - Confirm the approved scene-write route for Pipeline

- **Status:** Blocked on reconciling the existing write policy with the new bridge before a scene-editing task.
- **Source:** [AGENTS.md](../AGENTS.md), Unity write invariant; [Unity MCP Rules](Unity-MCP-Rules.md), Current connection.
- **Evidence:** The legacy policy requires `Unity_RunCommand`. The newly connected Pipeline tool catalog exposes `eval`, not that legacy tool name. Only read-only status calls were validated in Session 05.
- **Task-specific exception:** On 2026-09-15 Zerenn explicitly approved Pipeline for restoring `e2e2322`, reloading, and testing the Game scene. Recovery succeeded with primitive-only output. This bounded approval does not rewrite the general scene-authoring policy.
- **Room-build exception:** Zerenn separately approved Pipeline `eval`/`run_script` for Z-012 and its verification in Session 07. The room build completed through that route, with focused primitive-only results. The general policy remains unchanged for future tasks.
- **Session 08 exception:** Zerenn approved Pipeline eval/run_script for Z-013 scene edits, saving, and Play-mode verification. Implementation and verification completed through that route. This closes the task-specific gate; the general future-task rule remains unchanged.
- **Next check:** Review the Pipeline script-execution contract and agree the equivalent permitted route with Zerenn while preserving the prohibition on full-object serialization. Do not silently substitute a scene-write tool.
- **Boundary:** This does not block source-code or documentation tasks.

## Tier 2 - Content and current-state reconciliation

### Z-006 - Reconcile historical roadmap and technical claims

- **Status:** Needs verification in bounded, system-specific tasks.
- **Source:** April/May 2026 reference documents and [roadmap](Zerenn-Roadmap.md).
- **Next check:** When a system becomes the assigned scope, reconcile its milestone status, code description, and historical issue list against current files and editor state. Do not treat old script counts, test reports, or deferred findings as current facts.
- **Coverage:** Historical inventory, audio, chest/switch, dungeon, boss, and polish plans stay in the roadmap until their current status is checked. Old audit findings remain in Bug History.

## Tier 3 - Deferred design and refactoring

### Z-011 - Recover newer conflicted room-layout edits

- **Status:** Deferred after Zerenn approved the prior 10-room layout for camera recovery.
- **Source:** [Scene recovery evidence](Recon/S05-Z009-Game-Scene-Conflicts.md).
- **Preserved data:** The original conflicted scene's recorded Git-history location is `b91e1b8:Assets/Scenes/Game.unity`. The old local `Temp/Z009-Recovery-20260915/Game.conflicted.unity.txt` backup is absent at Session 09's check. The initial recovery used `e2e2322`; the active scene has since received the Session 07 and 08 changes. Preserve the tracked recovery assets.
- **Next check:** Establish which newer room/layout edits Zerenn wants, then reconstruct them against the working scene. Neither blanket merge-side choice preserves valid references. Temp files are local and may be cleared by Unity; Git is the durable recovery source.

### Z-007 - Shared beam implementation

- **Status:** Deferred; current duplication and need not reassessed.
- **Source:** [Session 04](Sessions/Session-04-Handoff.md), What's Next; [decisions](Zerenn-Decisions.md), Open Design Questions.
- **Next check:** Inspect SwordBeam, SpearBeam, and TemplarWave together when assigned. Preserve their distinct behavior; a base class is a historical proposal, not an automatic requirement.

### Z-008 - Historical open design questions

- **Status:** Deferred; decisions need Zerenn's direction before implementation.
- **Source:** [Zerenn-Decisions.md](Zerenn-Decisions.md), Open Design Questions.
- **Scope:** Mount/ram balancing, mummy emerging/stun behavior, beam hit deduplication, and off-screen enemy activity. The beam refactor itself is Z-007.
- **Next check:** Revisit one question when related work is selected. The decision document retains the exact design context.

## Completed in the current follow-up

### Z-013 - Enemy-free playable world and connected rooms

- **Completed:** Session 08, 2026-09-15, following Zerenn's Pipeline approval.
- **World:** Removed all 42 placed enemies and their two attached shields; preserved all 615 original non-enemy GameObjects, enemy scripts/prefabs, scenery, puzzles, NPCs, and recovery backups. No independent enemy spawner exists; no enemies return after scene reload.
- **Connections:** Retained 26 paired ordinary routes; connected Room_0_1 north <-> shop south and shop north <-> Reedwater south; repaired both cave round trips. All 12 rooms have at least two distinct destinations. Cave entrances retain their puzzle gates; Reedwater also has two ungated exits.
- **Arrivals:** 1.5-unit ordinary arrival inset, same-frame debounce, synchronized Rigidbody placement, safe shop/cave resume offsets, and room perimeter backstops.
- **Evidence:** 320 traversal checks and 79 save/reload/containment checks passed, covering all 34 directed exits and all 12 rooms. Zero remaining enemies/missing scripts/runtime errors. Compilation passes with the two documented warnings in unchanged source. [Exit table and evidence](Recon/Z013-World-Connectivity.md).
- **Delivery:** Implementation and evidence committed in `d829be6`, verified during Session 09. Session 08 ended with the Game scene saved and Play stopped, gameplay preferences/input settings restored; Unity was subsequently closed at Zerenn's request. Current remote/laptop state is not verified. No independent audit or user feel review claimed. No unresolved Z-013 implementation work.

### Z-010 - BoomShroom explosion script reference repaired

- **Completed:** Session 07 follow-up, 2026-09-15, after Zerenn supplied the live warning stack.
- **Cause:** `ToxicExplosion.prefab` referenced nonexistent script GUID `7a924de975f71284e9aaa08f86c924ce`. The existing `ExplosionEffect.cs` uses `f03f28458b68097adb7490a61dee0d30`.
- **Fix:** Reconnected the existing component through Unity's asset APIs and saved the prefab. Preserved the component ID, visuals, Animator, and 0.5-second lifetime; no gameplay C# or scene changes in this follow-up.
- **Evidence:** Reimport reports zero missing scripts. Play-mode direct detonation, damage-triggered detonation with Inspector-style overrides, and `BlinkThenExplode` all passed single-effect, damage-radius, and cleanup checks. No new explosion warnings/errors. [Session 07 follow-up](Sessions/Session-07-Handoff.md#follow-up---boomshroom-explosion-warning-z-010).
- **Delivery:** Local and uncommitted. Test objects removed and temporary background setting restored. Zerenn subsequently stopped Play mode; close-out confirmed the Game scene clean and live Console at zero errors/warnings.

### Z-012 - Complete overworld room and reused secret cave

- **Completed:** Session 07, 2026-09-15. Zerenn's implementation request refined the room into a natural overworld composition with cliffs, water, vegetation, a winding route, and a bush-concealed secret.
- **Result:** `Room_2_0 - Reedwater Hollow`, connected east of Room_1_0 and registered with WorldMap. Reuses the existing angel/fountain cave with a dedicated return; preserves the first-room cracked-wall connection and separate shop.
- **Evidence:** Saved-scene reload, collision connectivity scan, 23 Play-mode traversal checks, repeated secret round trips, 4 combat checks, minimap verification, and camera screenshots. [Verification](Recon/Z012-Reedwater-Hollow-Verification.md).
- **Delivery:** Local changes, not committed or pushed. Zerenn's visual/feel review and an independent audit have not run.

### Z-005 - Adjacent-room content workflow validated

- **Completed:** As part of Z-012. Added a room at an unused coordinate, connected it with direction-based transition prefabs, registered WorldMap, and verified entry, exit, re-entry, camera framing, and saved state. [Session 07](Sessions/Session-07-Handoff.md).

### Z-009 - Game scene and camera startup recovered

- **Completed:** 2026-09-15, with Zerenn's explicit restoration approval.
- **Change:** Restored `Assets/Scenes/Game.unity` exactly from `e2e2322`, retaining the original conflicted version in backup and Git history.
- **Verification:** Static object/reference checks passed; Unity loads 27 roots with the camera and player wired; Play-mode capture visibly renders the room; current console errors were zero. Editor returned to stopped/ready with a clean scene. [Evidence](Recon/S05-Z009-Game-Scene-Conflicts.md).
- **Limits:** Newer layout edits are Z-011; this does not close the historical zoom report. Recovery is included in Session 07's committed baseline `bdb4087`.
