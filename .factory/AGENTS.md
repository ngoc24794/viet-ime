# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a BMAD (Business Model and Design) workflow-enabled project for building a TodoApp. The repository currently contains the BMAD planning infrastructure. Application source code will be generated in `_bmad-output/` and potentially migrated to a `src/` directory during implementation.

## BMAD Workflow System

BMAD provides structured workflows for software development. Access them via slash commands:

### Quick Commands (Anytime)
- `/bmad-bmm-quick-spec` - Generate technical specs for small tasks
- `/bmad-bmm-quick-dev` - Execute small implementations without full planning
- `/bmad-bmm-generate-project-context` - Create LLM-optimized project context
- `/bmad-bmm-document-project` - Analyze and document existing codebases
- `/bmad-help` - Get guidance on next steps

### Development Phase Workflows

**1. Analysis Phase:**
- `/bmad-bmm-create-product-brief` - Define product vision and scope
- `/bmad-bmm-market-research`, `/bmad-bmm-domain-research`, `/bmad-bmm-technical-research`
- `/bmad-brainstorming` - Expert-guided ideation

**2. Planning Phase:**
- `/bmad-bmm-create-prd` - Create Product Requirements Document
- `/bmad-bmm-validate-prd` - Validate PRD completeness
- `/bmad-bmm-create-ux-design` - UX design guidance

**3. Solutioning Phase:**
- `/bmad-bmm-create-architecture` - Document technical decisions
- `/bmad-bmm-create-epics-and-stories` - Break down into epics/stories
- `/bmad-bmm-check-implementation-readiness` - Validate alignment

**4. Implementation Phase:**
- `/bmad-bmm-sprint-planning` - Generate sprint plans
- `/bmad-bmm-create-story` - Prepare story for development
- `/bmad-bmm-dev-story` - Execute story implementation
- `/bmad-bmm-code-review` - Review implemented code
- `/bmad-bmm-qa-automate` - Generate automated tests

### Agent Commands
- `/bmad-agent-bmad-master` - Master orchestrator (main entry point)
- `/bmad-agent-bmm-analyst`, `/bmad-agent-bmm-architect`, `/bmad-agent-bmm-dev`
- `/bmad-agent-bmm-pm`, `/bmad-agent-bmm-qa`, `/bmad-agent-bmm-sm`
- `/bmad-agent-bmm-ux-designer`, `/bmad-agent-bmm-tech-writer`

## Directory Structure

```
_bmad/
├── core/           # BMAD core system (agents, workflows, config)
├── bmm/            # BMAD Method Module (SDLC workflows)
│   ├── workflows/  # Phase-specific workflow definitions
│   ├── agents/     # Agent persona definitions
│   └── data/       # Templates and data files
├── _config/        # Manifests and customizations
└── _memory/        # Agent memory and standards

_bmad-output/       # Generated planning artifacts (PRD, architecture, etc.)
docs/               # Project documentation
.claude/commands/   # Claude Code slash command definitions
```

## Configuration

User preferences are defined in `_bmad/core/config.yaml`:
- Communication language: Vietnamese
- Document output language: English
- Output folder: `_bmad-output/`

## Workflow Artifacts

Planning documents are generated in `_bmad-output/`:
- `planning_artifacts/` - PRD, architecture, epics/stories, tech specs
- `project-knowledge/` - Research documents, project context
- `implementation_artifacts/` - Sprint status, story files

## Party Mode

Use `/bmad-party-mode` to load multiple agents for collaborative discussion on complex decisions.
