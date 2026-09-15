# Zelda Audit Briefs

Use this skeleton for a scoped review. Claude is the supporting auditor for Codex-led implementation. Gemini or a separate Codex reviewer can be included when assigned. Use the same request, revision, scope, and intentional-behavior list for every reviewer in a round.

Fill every placeholder from current evidence before delivering a brief. Keep the audit read-only; implementation fixes are a separate assignment. Save a reusable handoff in [Prompts](Prompts/) only when another tool needs it.

## Shared brief

```text
# Audit - <session/item> - <reviewer>

## TASK
READ ONLY. Audit <one exact change> for correctness and regressions.

## CONTEXT
Project: The Legend of Zerenn, C:\Zelda, branch Codex.
Revision or working-diff snapshot: <exact commit/base and dirty-file scope>.
Complete agreed request: <requirements and accepted clarifications>.
Verification already performed: <actual results and outstanding checks>.

## SCOPE
Read these files:
- @<file>
Follow direct callers/callees only where needed to verify a claim.
Explicitly out of scope: <boundaries>.

## RULES
- READ ONLY: do not edit, create, or delete files.
- Do not change Git state, Unity scenes, Play Mode, or serialized fields.
- Use AGENTS.md, the relevant decisions, and Unity-MCP-Rules.md.
- Never use Unity_ManageGameObject or request a full Unity object graph.
- Verify caller and absence claims with searches. Do not invent missing intent.

## CHECK FOR
- Required behavior and the complete agreed scope.
- Null handling, initialization, destruction, and stale state.
- Relevant input guards, same-frame debounce, and one-frame cooldowns.
- Inspector overrides, save routing, death, pause, and room transitions.
- Regression risk and whether verification actually covers the change.
Concrete anchors for this change: <methods/state transitions/scenarios>.

## INTENTIONAL BEHAVIOR
<Relevant invariants and decisions; include sanctioned exceptions.>

## OUTPUT
Severity: P0 / P1 / P2 / P3
File:
Location: <grep-able method or anchor>
Problem: <reproduction path and evidence>
Why it matters:
Recommended fix: <describe, do not apply>

If none: No issues found. List the scope checked and residual risk.
READ ONLY. Report findings; do not apply fixes.
```

## Reviewer emphasis

- **Claude:** compare the change with actual sibling patterns and trace complete lifecycles.
- **Separate Codex reviewer:** check request-to-diff completeness, unintended edits, and the evidence for each claim.
- **Gemini:** trace concrete cross-system edge cases named in the brief; follow [GEMINI.md](../GEMINI.md) for its output wrapper.

These are emphasis areas, not separate scopes. Do not turn a generic category into a finding without a relevant code path.

## Severity

Use the scale in [AGENTS.md](../AGENTS.md): P0 crash/data loss/soft-lock; P1 functional gameplay bug; P2 minor logic, stale state, or code quality; P3 low-risk cleanup. Judge findings by evidence, not agreement counts.
