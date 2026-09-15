# Support Prompts

Store a prompt here when handing a scoped task to Claude or another assigned tool. Codex's routine implementation in this app does not need a separate prompt file.

Name files by session, item, and purpose, for example `S05-Z004-Recon-Claude.md`. A saved brief is not proof it was sent or executed. Record actual returns in the session handoff or [Recon](../Recon/).

## Implementation handoff template

```text
TASK: One exact objective
OWNER: Assigned writer and branch
CONTEXT: User request, agreed behavior, pre-existing changes
FILES: Exact files; direct caller/callee extensions if necessary
WORK: Concrete steps grounded in current source
INVARIANTS: Relevant AGENTS.md and decision anchors
VERIFY: Commands/scenarios and expected results
RETURN: Changed files, evidence, remaining limitations
```

For read-only review, use [Audit-Briefs.md](../Audit-Briefs.md). State read-only explicitly; do not mix implementation instructions into an audit brief.
