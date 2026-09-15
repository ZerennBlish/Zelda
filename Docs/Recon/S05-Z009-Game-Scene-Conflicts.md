# Session 05 / Z-009 - Game Scene Conflict Investigation

**Date:** 2026-09-14, America/Denver
**Branch / HEAD:** `Codex` / `b91e1b81801fd1f80b4884cd8bbc40ca4295695e`
**Scope:** Initial investigation of the historical camera-zoom report, followed only far enough to identify why the live Game scene could not provide camera state.

## Observations

- Direct `unity_zelda.editor_status` returned `projectPath: C:\Zelda`, `ready`, `compiling: false`, and Play Mode stopped.
- `list_open_scenes` returned one active, loaded, clean scene: `Assets/Scenes/Game.unity`, with `rootCount: 0`.
- A read-only `eval` requesting primitive scene/camera information timed out on the main thread after 10 seconds. It returned no camera measurements. No mutating commands were issued.
- Searches in `Assets/Scripts` found no zoom or orthographic-size adjustment. Camera references there concerned aiming and room positioning. This does not establish what the old zoom report referred to.
- The saved Game scene includes a camera with `orthographic size: 5`, but also unresolved `Updated upstream` / `Stashed changes` conflict markers. Its saved state is not reliable live Inspector evidence while the scene is invalid.

## Git evidence

| Scene revision | Lines | Conflict starts | Conflict ends |
| --- | --- | --- | --- |
| HEAD `b91e1b8` | 34,342 | 79 | 79 |
| `e2e2322` | 30,334 | 0 | 0 |
| `f779536` | 29,371 | 0 | 0 |

`git diff --quiet -- Assets/Scenes/Game.unity` returned 0: the scene matches HEAD and has no uncommitted edits. The defects therefore predate this investigation and the current local documentation/package changes.

The current commit has one parent, `e2e2322`, and subject `Merge conflicts in Game.unity and settings.local.json`. The preceding scene commits are `Rework Game scene, world map, and local settings` and `Add Room_0_-2 to world and Game scene`.

First grep-able conflict anchor: `<<<<<<< Updated upstream` near file line 160. The conflict spans change objects, hierarchy references, and other scene data; simply deleting marker lines is not a valid merge.

## Conclusion and boundary

The committed scene text is invalid and the editor reports an empty loaded Game hierarchy. This blocks a meaningful camera/gameplay baseline. The exact import error and the main-thread timeout cause have not been separately diagnosed.

The previous marker-free scene is available as a recovery source. No candidate scene has been reconstructed or loaded, and no latest-layout choice has been made. Preserve both sets of intended changes and coordinate editor/file state before recovery. Track the action as [Z-009](../Tracked-Items.md); do not overwrite or save the empty live scene as a shortcut.

## Camera startup follow-up - 2026-09-15

Zerenn requested a camera startup fix and confirmed the Unity game camera. The active checkout resolves to `D:\Zelda` through the `C:\Zelda` junction, on `Codex`. Pre-existing local changes were limited to Session-05-Handoff, Unity-MCP-Rules, and Zerenn-Project-Setup.

- Live Unity status: ready, compilation finished, Play mode stopped. Game is loaded, clean, and has zero root objects. The console explicitly reports that `Assets/Scenes/Game.unity` has merge conflicts. A primitive-only Pipeline `eval` independently returned zero roots.
- Resolving all 79 blocks toward Updated upstream leaves two references to missing transform `14986351`; resolving toward Stashed changes leaves many missing references and duplicate room names. Neither blanket choice is a valid repair.
- Prepared an exact copy of the preceding scene from `e2e2322` at `Temp/Z009-Recovery-20260915/Game.e2e2322.unity.txt`, plus a byte-for-byte backup of the current conflicted scene and `validation.json` in that folder. Temp is local recovery storage, not a published handoff.
- Static checks on that candidate: 1,015 serialized objects, unique object IDs, no unresolved local references, no detected parent/child mismatches, and all referenced non-built-in asset GUIDs found. Its 10 named rooms include the secret room. The enabled orthographic camera is tagged MainCamera, has size 5, and is referenced by RoomManager; the player reference also resolves.
- Zerenn explicitly approved restoring `e2e2322` and using Pipeline for this recovery and verification, including the exception to the legacy Unity_RunCommand-only rule and live-editor file-edit restriction. This returned the active scene to the previous layout; newer conflicted room edits remain in the backup and Git history for separate recovery.
- Applied the exact candidate through Pipeline `eval` after checking the stopped, clean, empty Game scene and both SHA-256 hashes. Imported and reopened the scene through Unity; it now has 27 roots and a live MainCamera. Live inspection confirmed camera position `(0, 0, -10)`, orthographic size 5, enabled state, and RoomManager camera/player references.
- Entered Play mode and captured `Temp/Z009-Recovery-20260915/Game-camera-restored.png`; visual inspection confirmed the camera renders the room. The console reported zero current errors. One new missing-script warning originated from `BoomShroom.Explode()` when instantiating its effect; logged separately as Z-010 without expanding this fix.
- A runtime `eval` intended to capture camera/player values and pause timed out after 60 seconds. Do not treat that call as successful or claim runtime field measurements from it. Independent status, screenshot, and console checks succeeded. Stopped Play mode afterward; Unity reports ready, not compiling, and a clean loaded Game scene with 27 roots.
- Camera startup is recovered (Z-009). The historical zoom report (Z-001), the missing-script warning (Z-010), and recovery of newer conflicted layout edits (Z-011) remain separate work. No gameplay C# or package files changed; no independent audit, full gameplay test, commit, or push was performed.
