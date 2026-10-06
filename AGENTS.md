# Zelloa — Agent Entry Point

This file is the entry point for any AI coding agent working on the Zelloa repository.

## 1. Mandatory Reading

Before modifying the repository, read:

1. `/docs/01-Zelloa-Product-Specification.md`
2. `/docs/02-Zelloa-Technical-Architecture.md`
3. `/docs/03-Zelloa-Development-Plan.md`
4. `/docs/04-Zelloa-AI-Instructions.md`
5. `/docs/05-Zelloa-Payment-Integration.md`
6. `/docs/06-Zelloa-API-Specification.md`
7. `/docs/development/CURRENT-STATE.md`

These documents contain the product, architecture, development process, engineering rules, payment model, API conventions, and current development state.

Do not duplicate or reinterpret their rules in this file.

---

## 2. Authority

When information conflicts, use this order:

1. Product Specification
2. Technical Architecture
3. Development Plan
4. AI Development Instructions
5. Payment Integration
6. API Specification
7. Current Development State
8. Existing implementation

If code conflicts with normative documentation, do not silently adapt either side.

Report the conflict before proceeding.

---

## 3. Starting a Development Session

At the beginning of every session:

1. Read the documents listed above.
2. Read `CURRENT-STATE.md`.
3. Inspect the repository.
4. Inspect `git status`.
5. Identify uncommitted changes.
6. Determine the current phase.
7. Determine the last completed Gate.
8. Compare the actual repository state with `CURRENT-STATE.md`.
9. Run relevant validation when necessary.
10. Determine the next task allowed by the current phase.

Never assume context from previous AI conversations.

The repository must contain enough information to resume development without conversation history.

---

## 4. Current Development State

`/docs/development/CURRENT-STATE.md` is the operational handoff between sessions.

It records:

- current phase;
- phase status;
- last completed phase;
- completed work;
- work in progress;
- pending work;
- migrations;
- build/test status;
- known issues;
- approved decisions made during development;
- next recommended task.

`CURRENT-STATE.md` does not override normative documentation.

Always verify it against the repository.

---

## 5. Scope

Work only on the current phase defined by:

`03-Zelloa-Development-Plan.md`

and identified by:

`CURRENT-STATE.md`.

Do not automatically implement future phases.

Suggestions for future work are allowed.

Suggestions are not authorization to implement them.

---

## 6. Structural Decisions

If implementation requires a structural, architectural, security, financial, data-model, external-integration, or product decision not already defined in the documentation:

STOP.

Present:

- problem;
- available options;
- pros and cons;
- recommendation;
- impact.

Wait for explicit approval before implementing the decision.

For trivial, local, reversible implementation details already consistent with the documentation, proceed using normal engineering judgment.

---

## 7. Documentation

Documents `01` through `06` are normative.

Do not modify normative documentation simply to match generated code.

If an approved decision changes the project:

1. update the relevant normative document;
2. update `CURRENT-STATE.md` when appropriate;
3. only then implement the change.

---

## 8. Before Ending a Session

Before considering a development session complete:

1. inspect the changes made;
2. run all applicable builds and tests;
3. resolve failures related to the current work;
4. update `/docs/development/CURRENT-STATE.md`;
5. record remaining work;
6. record blockers or known issues;
7. record the exact recommended next task.

The goal is that another AI session can safely continue from the repository alone.

---

## 9. Phase Completion

When a phase Gate is reached, use the checkpoint format defined in:

`03-Zelloa-Development-Plan.md`

and

`04-Zelloa-AI-Instructions.md`.

After presenting the checkpoint:

**STOP.**

Do not start the next phase without explicit authorization.

---

## 10. Core Rule

The repository is the project's persistent memory.

Conversation history is not.

Every development session must leave the repository in a state that another session can understand and safely continue.