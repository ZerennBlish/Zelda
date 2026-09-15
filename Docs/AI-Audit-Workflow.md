# Zelda Audit Workflow

This is the review workflow for Codex-led work on `Codex`. It adapts the existing multi-AI audit practice to the assignment in [AGENTS.md](../AGENTS.md). Use [Audit-Briefs.md](Audit-Briefs.md) for the brief and [Close-Out.md](Close-Out.md) for session delivery.

## Roles

| Role | Responsibility |
| --- | --- |
| Zerenn | Final decisions on scope, design, and delivery |
| Codex | Implements on this branch, verifies its work, and triages returned findings |
| Claude | Supporting independent auditor; supporting implementer only when separately assigned |
| Opus | Optional design discussion, brief preparation, or second opinion on triage |
| Gemini | Additional read-only review when assigned |
| Separate Codex reviewer | Read-only review when assigned; distinguish from the implementer's self-review |

Implementation authority and audit mode are separate. An audit does not authorize edits. Only one AI writes to the shared working tree/editor at a time.

## Choose a bounded scope

Review the changed behavior, direct callers/callees, and relevant invariants. A wider milestone or system audit is its own explicit task; do not turn every small change into a full-codebase sweep.

Provide the complete agreed request, relevant decisions, exact revision or working-diff snapshot, changed files, pre-existing dirty files, and verification results. Reviewers must know whether they are checking committed code or local changes.

For a larger audit, group by connected systems: player, persistence/world, NPC/UI, combat/projectiles, or enemies. Choose the batch size from the call paths and available context, not an arbitrary file count.

## Give each reviewer the same contract

Use the shared core in [Audit-Briefs.md](Audit-Briefs.md). Additional emphasis can differ, but requirements and scope must match. Include intentional exceptions, such as AOE bypassing ShieldKnight's directional block and Archer having no melee.

Every reviewer stays read-only: no file writes, commits, package changes, Play Mode changes, or scene modifications. Use only safe Unity reads.

A brief that was written but never run is pending, not a completed audit. Additional tools or parallel reviewers are used only when assigned; Claude remains the normal support auditor for this branch.

## Triage findings by evidence

1. Check the exact method, caller path, and reproduction described.
2. Compare against the request, live state, and intentional behavior.
3. Confirm whether the finding is a defect, an unverified lead, a design question, or cleanup.
4. Assign the project's severity and decide the in-scope action.

Use the [AGENTS.md](../AGENTS.md) scale:

- **P0:** crash, data loss, or soft-lock.
- **P1:** functional gameplay bug; fix before the next build.
- **P2:** minor logic problem, stale state, or code quality issue.
- **P3:** style, naming, or low-risk cleanup.

Agreement among reviewers is useful context, not proof. A lone finding with a demonstrated path can be valid. Absence/dead-code claims require searches, and Inspector values can override source defaults.

Zerenn decides unresolved game-design questions. Codex can fix confirmed defects within the existing implementation assignment; an explicitly read-only task ends with findings.

## Apply fixes as implementation work

Group fixes that share the same root cause or files, keeping one coherent objective. State the writer and stop overlapping editor/repo writes before a handoff.

After changes, repeat the checks affected by the fix. Request another independent review when the fix changes the audited behavior substantially or leaves unresolved concerns. Do not rerun broad checks merely for ceremony.

## Record the outcome

Keep durable investigation or audit evidence under [Recon](Recon/) when needed, with a session/item name and revision. Put confirmed gameplay fix history in [Zerenn-Bug-History.md](Zerenn-Bug-History.md), open actions in [Tracked-Items.md](Tracked-Items.md), and design rulings in [Zerenn-Decisions.md](Zerenn-Decisions.md).

The handoff names the reviewer, actual scope, findings disposition, fixes, and remaining risks. Do not claim an independent pass when only Codex's self-review ran.
