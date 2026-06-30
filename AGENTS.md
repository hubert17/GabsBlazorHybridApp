# Agent Instructions: Git Command Optimization

These guidelines govern how the AI agent handles Git commands in this workspace to ensure optimal performance, prevent command execution errors, and keep token consumption extremely efficient.

---

## 🚀 General Policy
- **Token-Light Commands:** The agent can execute these automatically (or propose them directly) without asking for manual user intervention.
- **Token-Heavy Commands:** The agent must **NOT** execute these blindly. Instead, the agent must present the command to the user for copy-pasting, along with structured options on how to proceed.

---

## 🟢 Token-Light Git Commands (Allowed Automatically)
The agent is authorized to run or propose the following lightweight commands directly:
* `git status` — To check current changes and file states.
* `git checkout <branch>` / `git checkout -b <new-branch>` — To switch/create branches.
* `git branch` — To list local branches.
* `git restore <file>` / `git reset <file>` — To discard uncommitted changes.
* `git stash` / `git stash pop` — To temporarily shelter changes.
* `git fetch` — To fetch updates from remote.

---

## 🚫 Git Commit / Add Policy (Do NOT run or propose commits)
- **User Managed:** The user manages all git staging, diff reviews, and commits directly in the Visual Studio 2026 Git Changes panel.
- **Protocol:** The agent must **NEVER** execute `git add`, `git commit`, `git push`, or any staging/committing commands. The agent does not need to present git diffs or prompt the user for permission to commit changes.

---

## 🔴 Token-Heavy Git Commands (Requires Structured Offer)
The following commands produce potentially massive outputs that can bloat context, eat tokens, or trigger auth/merge blockages:
* `git log` (without tight restrictions like `-n 5` or `--oneline`)
* `git diff` (across branches, commits, or whole repository without targeting specific files)
* `git blame <file>` (especially on large source files)
* `git show <commit>` (for commits modifying multiple files or large blocks of code)
* `git merge` / `git rebase` / `git pull` / `git push`

### 📋 The Action Protocol for Token-Heavy Commands:
When the agent needs information from or wants to run one of these heavy commands, it **MUST NOT** execute it immediately. Instead, it must print a dedicated interactive prompt to the user containing:
1. **Rationale:** Why this command/information is needed.
2. **Copy-Paste Section:** The exact Git command formatted in a code block.
3. **Execution Choice Prompt:** Present the user with the following three explicit choices:
   - **Option 1 (Manual Run - Recommended):** *"Run the command in your local terminal, then copy-paste only the relevant portion of the output back to me."*
   - **Option 2 (Constrained Run):** *"Let me run an optimized/constrained version of this command (e.g. adding `--oneline -n 5` or restricting to specific files) to save tokens."*
   - **Option 3 (Full Agent Run):** *"Proceed and run the full command anyway. (Warning: high token cost)."*

---

## 🛠️ Formatting Diffs and Logs
- When outputting diffs, always prefer target file paths to limit output sizes: `git diff -- <file-path>`.
- When retrieving commit history, default to: `git log --oneline -n 5`.

---

## 🔨 Build and Debug Verbosity Control
To optimize token usage and keep build feedback concise:
- **Initial Build/Debug Runs:** When there is a high likelihood of a successful run, initially limit or reduce verbosity.
  - For .NET CLI commands (`dotnet build`, `dotnet run`, `dotnet test`), append `-v q` (quiet) or `-v m` (minimal) verbosity flags (e.g., `dotnet build -v m`).
  - For Docker / Docker Compose, run in standard or quiet modes rather than verbose debugging modes.
- **On Failure:** If the build or debug run fails, rerun with normal/detailed verbosity (e.g., `-v n` or `-v d`) to output verbose logging for troubleshooting.

---

## 🎨 Component Styling Policy
- **Prefer Standard MudBlazor Styling:** Avoid introducing custom styling/custom CSS classes/custom `<style>` tags when standard MudBlazor styling options (e.g., helper classes, components, variables, elevation, padding attributes) can achieve the same outcome.
- **Goal:** Minimize custom CSS to facilitate smooth, future upgrades of MudBlazor and CodeBeam extensions without breaking overrides.
