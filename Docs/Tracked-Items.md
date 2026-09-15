# Zelda Tracked Items

**Owner:** Codex on `Codex`. **Last reconciled:** Session 05, 2026-09-14.

This is the current open-work queue. The [roadmap](Zerenn-Roadmap.md) retains milestone plans; [decisions](Zerenn-Decisions.md) retain design rationale; handoffs link here instead of maintaining competing task lists.

## Next task

Zerenn invited Codex to propose the starting point. Initial camera investigation found Z-009: the active Game scene contains committed merge conflicts and Unity reports zero root objects. Recovering a valid scene is the first recommended priority; camera and other gameplay investigation follows that baseline.

## Status and maintenance

- **Confirmed:** reproduced or established against current source/state; include evidence.
- **Needs verification:** reported previously, with current status not established.
- **Planned:** requested or recorded work whose implementation has not been verified complete.
- **Blocked:** name the specific missing decision or dependency.
- **Deferred:** retained for later consideration, not an active assignment.

Use stable `Z-NNN` IDs; do not renumber or reuse an ID. Record the source, next check, and any dependencies. New items enter a tier immediately. Record completion evidence in the handoff or commit before removing a finished item. Next unused ID: `Z-010`.

## Tier 1 - Correctness and prerequisites

### Z-009 - Recover Game scene from committed merge conflicts

- **Status:** Confirmed invalid scene text; P0 gameplay baseline blocker.
- **Source:** [Session 05 scene investigation](Recon/S05-Z009-Game-Scene-Conflicts.md).
- **Evidence:** `Assets/Scenes/Game.unity` matches HEAD `b91e1b8` and contains 79 opening and 79 closing Git conflict markers. The live Game scene reports zero root objects. The preceding scene at `e2e2322` has no conflict markers.
- **Next step:** Compare the conflicted scene with preserved versions and identify the intended room changes before reconstructing a valid scene. Validate loading, expected hierarchy, and object references before resuming camera work.
- **Boundary:** No scene save, replacement, or restoration has been performed. An older marker-free revision is a recovery source, not proof that overwriting with it preserves the latest work. Coordinate any recovery with the live editor and the current write policy in Z-004.

### Z-001 - Camera zoom minimum

- **Status:** Needs verification; no current severity assigned.
- **Source:** [Session 04](Sessions/Session-04-Handoff.md), What's Next and Known Issues.
- **Report:** Camera zoom appeared stuck at a minimum of 2.5.
- **Next check:** Resolve Z-009 first, then reproduce with the current camera and Inspector settings and trace the clamp/input path. The initial source search found no gameplay zoom implementation; the saved camera size is 5, so the old minimum-of-2.5 report is not yet explained.

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
- **Next check:** Review the Pipeline script-execution contract and agree the equivalent permitted route with Zerenn while preserving the prohibition on full-object serialization. Do not silently substitute a scene-write tool.
- **Boundary:** This does not block source-code or documentation tasks.

## Tier 2 - Content and current-state reconciliation

### Z-005 - Validate an adjacent-room content workflow

- **Status:** Planned in historical handoff; completion not checked.
- **Source:** [Session 04](Sessions/Session-04-Handoff.md), What's Next.
- **Next check:** Confirm current room content and the chosen room with Zerenn before scene edits. Depends on Z-004 for MCP scene writes.

### Z-006 - Reconcile historical roadmap and technical claims

- **Status:** Needs verification in bounded, system-specific tasks.
- **Source:** April/May 2026 reference documents and [roadmap](Zerenn-Roadmap.md).
- **Next check:** When a system becomes the assigned scope, reconcile its milestone status, code description, and historical issue list against current files and editor state. Do not treat old script counts, test reports, or deferred findings as current facts.
- **Coverage:** Historical inventory, audio, chest/switch, dungeon, boss, and polish plans stay in the roadmap until their current status is checked. Old audit findings remain in Bug History.

## Tier 3 - Deferred design and refactoring

### Z-007 - Shared beam implementation

- **Status:** Deferred; current duplication and need not reassessed.
- **Source:** [Session 04](Sessions/Session-04-Handoff.md), What's Next; [decisions](Zerenn-Decisions.md), Open Design Questions.
- **Next check:** Inspect SwordBeam, SpearBeam, and TemplarWave together when assigned. Preserve their distinct behavior; a base class is a historical proposal, not an automatic requirement.

### Z-008 - Historical open design questions

- **Status:** Deferred; decisions need Zerenn's direction before implementation.
- **Source:** [Zerenn-Decisions.md](Zerenn-Decisions.md), Open Design Questions.
- **Scope:** Mount/ram balancing, mummy emerging/stun behavior, beam hit deduplication, and off-screen enemy activity. The beam refactor itself is Z-007.
- **Next check:** Revisit one question when related work is selected. The decision document retains the exact design context.
