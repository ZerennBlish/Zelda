# Opus - Zelda Support Guide

This guide is for Opus/Claude.ai supporting the Codex-led `Codex` branch. It is a Zelda-local adaptation; it is not a universal file to copy over other projects.

Read [AGENTS.md](../AGENTS.md), [About-Me.md](About-Me.md), and [Start-Here.md](Start-Here.md) for the current assignment, preferences, and document map.

## Role

Zerenn directs the project. Codex leads implementation, debugging, testing, and delivery here. Claude has equal implementation authority but supports this branch through audits, design discussion, or explicitly assigned implementation work.

Opus can help make a design concrete, prepare a scoped support brief, or review a disputed finding. Work in this app does not depend on an Opus prompt before Codex can implement.

Do not instruct Codex to return to its former read-only-only role. Explicit audit tasks remain read-only for every reviewer.

## Design and support briefs

Read current files and the relevant decisions before specifying an anchor or proposed edit. Prefer a concrete behavior and success criteria over invented line numbers or a rewrite of an unfamiliar system.

Use [Prompts](Prompts/) for a durable implementation handoff and [Audit-Briefs.md](Audit-Briefs.md) for read-only reviews. Each handoff names the recipient, branch, scope, agreed behavior, and verification.

Do not carry another project's stack, hooks, model settings, or Git restrictions into Zelda. Do not prescribe app commands or model settings from an old session without verifying that they apply.

## Audit support

Follow [AI-Audit-Workflow.md](AI-Audit-Workflow.md). Claude's normal support role here is independent review. Keep the full request and intentional-behavior list in the brief. Findings require evidence and use the P0-P3 scale in AGENTS.md.

Return the result to Codex/Zerenn for triage. A design decision goes to Zerenn. An audit remains a report; implementation is a separate assignment.

## Coordination and continuity

Keep one writer per working tree/editor. Before supporting implementation, confirm the assigned files and writer handoff. Do not modify the same files while Codex is implementing.

[Close-Out.md](Close-Out.md) owns the close-out sequence. [Tracked-Items.md](Tracked-Items.md) owns open work. The latest [session handoff](Sessions/) records actual verification and remaining work.

Claude.ai project knowledge is an optional support snapshot. Live repo files and current user instructions take precedence over stale uploads.
