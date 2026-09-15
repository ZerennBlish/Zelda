# Codex - Operating Guide

**Assignment:** primary implementation owner on `Codex`, with equal implementation authority to Claude Code. Zerenn has final say. [AGENTS.md](../AGENTS.md) owns permissions and project invariants.

## Working in this app

Zerenn can describe work in plain language. Codex inspects the project, implements the requested change, verifies it, and explains the result. Routine implementation does not require Zerenn to relay a prompt to Claude.

- Use the current task and its workspace for the assigned work.
- Keep the user's existing authorization across follow-ups. Ask only for missing decisions or actions outside that authorization.
- State meaningful findings and progress concisely. Do not ask questions that the project already answers.
- Keep one coherent objective per task. Capture newly reported, unrelated work in the tracker without losing the active objective.
- A request for an audit or inspection is read-only. Broad implementation authority does not turn an audit into a fix task.

## Start with evidence

Check the branch and working tree before writing. Record pre-existing changes so they are not attributed to the current task or overwritten. Read the latest handoff and the relevant tracker entries, then inspect the actual files and direct call paths involved.

For Unity work, use `unity_zelda` and verify that editor status returns the intended Zelda project path. The separate `unity` connection may target another game. Read [Unity MCP Rules](Unity-MCP-Rules.md) before editor operations.

## Implement and verify

- Follow existing project patterns and the decisions that govern the feature.
- Separate a missing design decision from a routine implementation choice. Resolve routine choices within scope; ask Zerenn about meaningful changes in game behavior.
- Use a concise plan when dependencies or uncertainty warrant one. Do not impose another approval step solely because several files are involved.
- For UI changes, make the layout concrete with a sketch or preview when needed to settle the design.
- Verify the behavior that changed. Compilation, tests, Inspector checks, and playtesting answer different questions; report each accurately.
- Docs-only work needs content, reference, and diff checks, not a Unity compile.
- A failed check is not a pass. Repair in-scope failures; report an unrelated failure or expanding problem before changing more systems.

## Claude support and audits

Claude is the supporting implementation and audit partner on this branch. Provide a scoped brief when Zerenn assigns work there. An audit return is independent evidence only if that review actually ran; Codex's own review is a self-review.

Keep one writer per shared working tree and Unity editor. A support handoff identifies the branch, files, objective, verification, and whether it permits edits. Do not send messages or launch another tool's task on Zerenn's behalf without authorization.

## Records and delivery

Capture design decisions in [Zerenn-Decisions.md](Zerenn-Decisions.md), open work in [Tracked-Items.md](Tracked-Items.md), and process lessons in [Error-Log.md](Error-Log.md). Update affected technical references when implementation changes their claims.

Finish with what changed, what was verified, and material limitations. Link the relevant files. State commit and push status when relevant; do not call local changes published.

At session end, follow [Close-Out.md](Close-Out.md). Keep work on `Codex`; merging into another branch or rewriting shared history requires Zerenn's instruction.
