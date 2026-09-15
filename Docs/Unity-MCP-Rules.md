# Unity MCP Working Rules

These rules apply when using Unity MCP for Zelda. The original safety observations below came from the legacy Unity AI Assistant bridge. Do not assume another bridge uses the same tool names or serialization behavior.

## Current connection - Codex app

- Use `unity_zelda`, pinned to `C:\Zelda`, through Unity CLI and `com.unity.pipeline`.
- Read `editor_status` and confirm the returned project path before editor work. The separate `unity` server can point at another project.
- Session 05 verified direct MCP status: editor ready, compilation finished, project `C:\Zelda`. No scene write was tested.
- Pipeline exposes `eval` and purpose-built editor commands. The current AGENTS.md scene-write rule still requires the legacy name `Unity_RunCommand`. Reconcile that policy before substituting a write route; see tracker Z-004 in [Tracked-Items.md](Tracked-Items.md).
- Never use `Unity_ManageGameObject` or request full Unity object-graph serialization. Keep reads focused and return primitive values or component names.
- Explicit audits do not change editor state. For implementation verification, discover the connected bridge's supported compilation and diagnostic commands before using them.

The sections below retain the legacy bridge's observations and restrictions. Endpoint and billing statements are historical reports, not current service checks; do not edit the hosts file or change package versions based only on these notes.

---

## Tool Selection

**For scene write operations — creating GameObjects, adding components, setting field values — use `Unity_RunCommand` exclusively.**

**Do NOT use `Unity_ManageGameObject` for write operations.** Its return path serializes the full Unity object graph through Newtonsoft.Json, which recursively walks Transform → parent → children → components → GameObjects → Transforms infinitely. Hits a `Matrix4x4.GetLossyScale` assertion failure and freezes Unity. Reproduces consistently. Forces editor force-close.

---

## Position Parameter Quirk

**MCP create's `position` parameter is world-space, not local.**

When creating a child GameObject, passing `position: (0, 0, 0)` lands it at world origin, not at the parent's local origin. To place it correctly relative to its parent:
- Set `Transform.localPosition` explicitly via `component_properties` on the create call, OR
- Follow up the create with a modify call that sets `Transform.localPosition`

---

## Read Limits

**Component name reads via MCP work. Deep field-value reads do not.**
Asking for the components attached to a GameObject (names only) is fast and reliable. Asking for "all serialized field values" on any component reproduces the same Newtonsoft.Json infinite recursion that crashes write operations. Only request component names through MCP. For field values, fall back to reading source files directly.

---

## Read Fallbacks

When MCP cannot read what you need, read the underlying files instead:
- Scene state: `Assets/Scenes/<SceneName>.unity` (YAML, plain text)
- Layers + collision matrix: `ProjectSettings/Physics2DSettings.asset`
- Tags + sorting layers: `ProjectSettings/TagManager.asset`
- Input bindings: `ProjectSettings/InputManager.asset` or the active Input System asset

These are all text-readable and contain the source-of-truth data.

---

## Unity AI Cloud Endpoint

The `com.unity.ai.assistant` package calls `https://generators.ai.unity.com`, which Unity has deprecated. The endpoint returns `ApiNoLongerSupported`. Canceling the Unity AI trial does NOT stop the package from calling the endpoint. The retries can stack with MCP traffic and contribute to editor freezes.

**Workaround:** Block the endpoint at the hosts file level so calls fail instantly.

`C:\Windows\System32\drivers\etc\hosts`:
```
0.0.0.0 generators.ai.unity.com
```

**Do NOT update `com.unity.ai.assistant` past whatever version currently works.** Version 2.7.0 has a separate documented bug that gates MCP behind a paid Unity tier.

---

## Verification Pattern

After any MCP scene write:
1. List the components on the modified GameObject (names only)
2. Confirm the expected component is present
3. Visually verify in the Unity editor (Inspector field values, scene view position)

Do not chain multiple write operations without verification between them. If a freeze happens, you want to know exactly which call caused it.

---

## Test Prompt Sizing

Long compound prompts that ask MCP to do many serial reads in one shot will hang or truncate. Break MCP work into focused single-task prompts. One operation, one verification, then the next prompt.
