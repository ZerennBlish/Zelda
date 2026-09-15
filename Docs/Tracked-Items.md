# Zelda Tracked Items

**Owner:** Codex on `Codex`. **Last reconciled:** Session 06 room-build handoff, 2026-09-15.

This is the current open-work queue. The [roadmap](Zerenn-Roadmap.md) retains milestone plans; [decisions](Zerenn-Decisions.md) retain design rationale; handoffs link here instead of maintaining competing task lists.

## Next task

Zerenn requested a handoff for Codex to build one complete playable room with properly placed repeating wall bricks, scenery, collision, enemies, and working entry/exit setup. This is Z-012; the implementation brief is in [Session 06](Sessions/Session-06-Handoff.md). Room implementation has not started. Camera startup recovery Z-009 is complete; the historical zoom report remains Z-001.

## Status and maintenance

- **Confirmed:** reproduced or established against current source/state; include evidence.
- **Needs verification:** reported previously, with current status not established.
- **Planned:** requested or recorded work whose implementation has not been verified complete.
- **Blocked:** name the specific missing decision or dependency.
- **Deferred:** retained for later consideration, not an active assignment.

Use stable `Z-NNN` IDs; do not renumber or reuse an ID. Record the source, next check, and any dependencies. New items enter a tier immediately. Record completion evidence in the handoff or commit before removing a finished item. Next unused ID: `Z-013`.

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
- **Next check:** Review the Pipeline script-execution contract and agree the equivalent permitted route with Zerenn while preserving the prohibition on full-object serialization. Do not silently substitute a scene-write tool.
- **Boundary:** This does not block source-code or documentation tasks.

## Tier 2 - Content and current-state reconciliation

### Z-012 - Build one complete room with properly placed wall pieces

- **Status:** Planned; Zerenn requested a handoff for implementation. No room changes made yet.
- **Source:** [Session 06 room-build brief](Sessions/Session-06-Handoff.md).
- **Objective:** One finished, playable 18 x 10 room with floor/scenery, solid walls, collision, deliberate enemy placement, player entry, camera framing, and working exits. Use the existing art and gameplay systems.
- **Visual requirement:** Construct wall length from repeated tiles/modular brick pieces at consistent intended proportions and pixel density, with proper corners and openings. Do not stretch wall sprites or use unequal scale to fill gaps.
- **Next work:** Inspect suitable assets and room locations, identify the exact edit scope, implement the complete room, and verify it in Play mode with screenshots. Preserve existing rooms and the recovered camera. Z-004 governs the authoring route; the exact room coordinate/theme were not specified.

### Z-010 - Missing-script warning from BoomShroom explosion

- **Status:** Warning observed; gameplay impact and exact prefab component need verification.
- **Source:** Camera startup playtest, 2026-09-15; console stack points to `BoomShroom.Explode()` at its effect instantiation.
- **Evidence:** Unity logged `The referenced script (Unknown) on this Behaviour is missing!` during Play mode. No current console errors accompanied the camera test.
- **Next check:** Inspect the serialized explosion-effect prefab reference and identify the missing component before choosing a repair. No enemy or prefab changes were made during camera recovery.

### Z-005 - Validate an adjacent-room content workflow

- **Status:** Planned in historical handoff; completion not checked.
- **Source:** [Session 04](Sessions/Session-04-Handoff.md), What's Next.
- **Next check:** Z-012 is the concrete current room-build brief. Validate the adjacent-room workflow as part of that build when its selected location requires it; do not start a second overlapping room task. Depends on Z-004 for MCP scene writes.

### Z-006 - Reconcile historical roadmap and technical claims

- **Status:** Needs verification in bounded, system-specific tasks.
- **Source:** April/May 2026 reference documents and [roadmap](Zerenn-Roadmap.md).
- **Next check:** When a system becomes the assigned scope, reconcile its milestone status, code description, and historical issue list against current files and editor state. Do not treat old script counts, test reports, or deferred findings as current facts.
- **Coverage:** Historical inventory, audio, chest/switch, dungeon, boss, and polish plans stay in the roadmap until their current status is checked. Old audit findings remain in Bug History.

## Tier 3 - Deferred design and refactoring

### Z-011 - Recover newer conflicted room-layout edits

- **Status:** Deferred after Zerenn approved the prior 10-room layout for camera recovery.
- **Source:** [Scene recovery evidence](Recon/S05-Z009-Game-Scene-Conflicts.md).
- **Preserved data:** The original conflicted scene remains in Git at `b91e1b8:Assets/Scenes/Game.unity` and in the local `Temp/Z009-Recovery-20260915/Game.conflicted.unity.txt` backup. The active scene is now the exact `e2e2322` version.
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

### Z-009 - Game scene and camera startup recovered

- **Completed:** 2026-09-15, with Zerenn's explicit restoration approval.
- **Change:** Restored `Assets/Scenes/Game.unity` exactly from `e2e2322`, retaining the original conflicted version in backup and Git history.
- **Verification:** Static object/reference checks passed; Unity loads 27 roots with the camera and player wired; Play-mode capture visibly renders the room; current console errors were zero. Editor returned to stopped/ready with a clean scene. [Evidence](Recon/S05-Z009-Game-Scene-Conflicts.md).
- **Limits:** Newer layout edits are Z-011; this does not close the historical zoom report. Changes remain local and uncommitted.
