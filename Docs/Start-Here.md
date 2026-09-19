# The Legend of Zerenn - Documentation

This is the entry point for work on the `Codex` branch in the Codex desktop app. Zerenn directs the project; Codex leads implementation; Claude provides support and audits.

This documentation follows the organization of `C:\IdBidOnThat\Docs`, adapted for Zelda and this assignment. These are Zelda-local documents. They do not sync to another project's docs or to a shared template folder.

## Start a task

1. Read [AGENTS.md](../AGENTS.md) for branch ownership, permissions, and game invariants.
2. Read [About-Me.md](About-Me.md), [Codex.md](Codex.md), and [Workflow.md](Workflow.md).
3. Read the most recent handoff in [Sessions](Sessions/) and [Tracked-Items.md](Tracked-Items.md). The current handoff is [Session 09](Sessions/Session-09-Handoff.md): laptop continuation from committed revision `d829be6`, including Session 08's completed enemy removal and connectivity work. The desktop began this handoff with a clean working tree. Remote/laptop synchronization remains for Zerenn to verify; the new handoff documentation is local until committed and published.
4. Read the technical references relevant to the requested task. Confirm live files and Unity state before relying on a historical description.

If Zerenn has already specified the task, begin that task. A tracker is context, not permission to select a different objective.

## Moving between desktop and laptop

Use GitHub's `Codex` branch to transfer committed project files and these docs between clones. Under Zerenn's current instructions, Git is read-only for Codex: Zerenn performs publication and synchronization. Use [Session 09](Sessions/Session-09-Handoff.md#laptop-continuation) for the current transfer requirements and [Workflow](Workflow.md#switch-computers-or-start-a-new-chat) for the checks. Remote control of the desktop still operates on the desktop's files.

For a new chat in the destination clone, use:

```text
Verify this Zelda clone on Codex using read-only Git, preserving all local work. Read AGENTS.md, Docs/Start-Here.md, the latest session handoff, and Docs/Tracked-Items.md. Report any synchronization Zerenn needs to perform and any current blocker before gameplay changes. No new gameplay task is assigned by the handoff itself.
```

The docs provide context when read; they do not send a notification to another chat. Unity MCP registration is local to each computer and needs its own path verification.

## Where information belongs

| Document | Owns |
| --- | --- |
| [AGENTS.md](../AGENTS.md) | Codex authority, branch assignment, project invariants |
| [Codex.md](Codex.md) | How Codex executes work and reports evidence in this app |
| [About-Me.md](About-Me.md) | Zerenn's communication and working preferences |
| [Workflow.md](Workflow.md) | The development loop and why the steps matter |
| [Close-Out.md](Close-Out.md) | The session close-out sequence |
| [Tracked-Items.md](Tracked-Items.md) | The current open-work queue and item status |
| [Zerenn-Decisions.md](Zerenn-Decisions.md) | Agreed design choices and their rationale |
| [Error-Log.md](Error-Log.md) | Process failures and the lessons they produced |
| [Audit-Briefs.md](Audit-Briefs.md) | Reusable, scoped audit brief |
| [AI-Audit-Workflow.md](AI-Audit-Workflow.md) | Independent review, triage, and follow-up |
| [Opus.md](Opus.md) / [CLAUDE.md](../CLAUDE.md) / [GEMINI.md](../GEMINI.md) | Instructions for supporting tools |
| [Sessions](Sessions/) | Dated work summaries, verification, and handoffs |
| [Prompts](Prompts/) | Handoff prompts when another tool is assigned work |
| [Recon](Recon/) | Bounded investigation notes and evidence |

## Technical references

- [Project Setup](Zerenn-Project-Setup.md): engine, paths, branches, tool connections, build setup.
- [Unity MCP Rules](Unity-MCP-Rules.md): connection checks and editor safety.
- [Architecture](Zerenn-Architecture.md): systems and their relationships.
- [Data Models](Zerenn-Data-Models.md): save keys and persistence contracts.
- [Features](Zerenn-Features.md): game behavior and player-facing systems.
- [Roadmap](Zerenn-Roadmap.md): milestone plans and historical completion notes.
- [Bug History](Zerenn-Bug-History.md): prior gameplay defects, fixes, and audit history.
- [Stability Playbook](Zerenn-Stability-Playbook.md): project-specific failure prevention.

## Keeping context accurate

Zerenn's current instructions take precedence over older role descriptions. `AGENTS.md` owns the assignment on this branch; specialist documents own the topics in the table. Fix a conflict where it lives instead of adding a second competing rule.

Sessions 03 and 04 and the dated game reference sections are historical evidence. They do not prove that a bug still exists, that a milestone is still incomplete, or that a test passes today. The tracker labels carried items accordingly.

The repo is the working documentation source for Codex. Claude.ai uploads are optional support handoffs, requested separately; they are not a prerequisite for completing a task here.
