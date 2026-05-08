# Session 02 Handoff — MCP Scene Write Validation

**Date:** May 7, 2026
**Machine:** Desktop

---

## What Happened

Validated Unity MCP scene write capabilities for level building. Identified and documented a freeze-class bug in the MCP package's response serialization. Confirmed a working pattern that both Claude Code and Codex can use.

### MCP Findings

- **`Unity_ManageGameObject` freezes Unity on component add.** The success response triggers Newtonsoft.Json recursive serialization through the Unity object graph (Transform → parent → children → components → GameObjects → Transforms, infinite recursion). Hits a `Matrix4x4.GetLossyScale` assertion and hangs the editor. Reproduced under both CC and Codex.
- **`Unity_RunCommand` is the workaround.** Codex completed the full create-and-wire sequence (create GameObject, parent, set localPosition, add BoxCollider2D, set isTrigger, set size) in under nine seconds via `Unity_RunCommand`. CC re-ran the same sequence with explicit tool guidance and passed cleanly.
- **MCP create's `position` parameter is world-space, not local.** Set `Transform.localPosition` explicitly via `component_properties` or a follow-up modify call.
- **Deep field reads via MCP also hit the recursion wall.** Asking for "all serialized field values" on any component reproduces the same Newtonsoft.Json infinite recursion. Component name reads work fine.
- **Unity AI cloud endpoint deprecated.** `generators.ai.unity.com` returns `ApiNoLongerSupported`. Hosts file entry `0.0.0.0 generators.ai.unity.com` applied on desktop to make the call fail instantly instead of timing out. Trial was already canceled before this session — cancellation does not stop the package from calling the endpoint.

### Codex Audit Fallback

When MCP component reads time out, Codex falls back to reading scene/project files directly:
- `Assets/Scenes/Game.unity` (YAML) for scene state
- `ProjectSettings/Physics2DSettings.asset` for layers and collision matrix
- `ProjectSettings/TagManager.asset` for tags and sorting layers

This is what made the Session 01-followup audit succeed where direct MCP reads failed.

### Files Modified
- `CLAUDE.md` — Added MCP tool selection rule (`Unity_RunCommand` for scene writes, never `Unity_ManageGameObject`) and the world-vs-local position note
- `Docs/Zerenn-Bug-History.md` — New section "Session 02 — May 2026 (MCP Workflow Validation)" with full root cause, fix, and notes
- `Assets/Scenes/Game.unity` — Removed test GameObjects (`MCPTest_Codex`, `MCPTest_CC2`) created during validation. `MCPTest_Empty` was already absent (force-close discarded it before save during the original Test 7 freeze).
- `AGENTS.md` — Briefly loosened to allow Codex MCP writes for the comparison test, then reverted to strict read-only at session close. Net-zero change.

### Test Sequence Run
Tests run in this order, each in its own focused prompt:
1. Active scene name (CC) — passed
2. Root GameObjects list (CC) — passed
3. Children of Canvas (CC) — passed
4. Components on GameState, names only (CC) — passed
5. Components on TransitionRight with full field values (CC) — **FROZE** Unity (Newtonsoft.Json recursion)
6. Create empty GameObject as child of Room_1_0 (CC) — passed, with world-vs-local quirk surfaced
7. Add BoxCollider2D to test object (CC) — **FROZE** Unity (same recursion)
6+7 combined via `Unity_RunCommand` (Codex) — passed in under 9 seconds
6+7 retest via `Unity_RunCommand` (CC) — passed

---

## What's Next

1. **Apply the hosts file fix to the laptop** if MCP work happens there too. Same line: `0.0.0.0 generators.ai.unity.com`.
2. **Clean up `CONVENTIONS.txt` and `ReadMe.txt` at repo root** — carried over from Session 01. `CONVENTIONS.txt` is empty, `ReadMe.txt` is an exact duplicate of `README.md`. Both safe to delete.
3. **Fix `Close-Out.md` Step 9** — currently says session handoffs are not stored as files, but the project instructions and Session 01 precedent both say otherwise. One-line correction.
4. **First level-building task** — workflow is now proven. CC can do MCP scene writes via `Unity_RunCommand`. Pick a room, draft a focused prompt, build the transitions.
5. **Roadmap immediate next step** — starting bomb count shows 10 instead of intended starting value. Verify intended default in design, fix the inspector value or `Start()` initialization.

---

## Known Issues

- `CONVENTIONS.txt` and `ReadMe.txt` at repo root — stale duplicates, still pending cleanup from Session 01
- `Close-Out.md` Step 9 contradicts the existing Sessions/ file pattern
- Hosts file fix applied to desktop only; laptop will need the same if MCP is used there
- `com.unity.ai.assistant` package is on a version that calls a deprecated endpoint. Do NOT update past current version — 2.7.0 has a separate documented bug that gates MCP behind a paid tier.
