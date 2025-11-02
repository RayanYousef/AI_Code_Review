# AI Code Review Setup Guide

This repository uses multiple AI code review tools that all reference `Assets/AGENTS.md` for coding standards.

## Tools Configured

### ✅ 1. Qodo (PR-Agent)
**Status:** Fully configured with native AGENTS.md support

**Setup:**
1. Add to GitHub Marketplace: https://github.com/apps/qodo-merge
2. Set secrets in repository settings:
   - `OPENAI_KEY`: Your OpenAI API key
3. Configuration files:
   - `.pr_agent.toml` - Main configuration
   - `.github/workflows/qodo-pr-agent.yml` - GitHub Action

**Commands:**
- `/review` - Full code review
- `/improve` - Code suggestions
- `/describe` - PR description

### ✅ 2. CodeRabbit
**Status:** Already configured

**Setup:**
- Already installed and configured via `.coderabbit.yaml`
- References `**/AGENTS.md` automatically

### ⚙️ 3. Graphite Agent
**Status:** Manual setup required

**Setup:**
1. Visit https://graphite.dev/
2. Install Graphite Agent from their marketplace
3. Connect to your GitHub repository
4. Graphite will automatically detect AGENTS.md in repository root
5. The workflow `.github/workflows/graphite-review.yml` adds context to PRs

**Features:**
- AI-powered code reviews
- Automatic guideline detection
- Inline suggestions

### ⚙️ 4. Gemini Code Assist
**Status:** Requires Google Cloud setup

**Setup:**
1. Enable Gemini Code Assist: https://developers.google.com/gemini-code-assist
2. Set up GitHub integration
3. Add secret to repository:
   - `GEMINI_API_KEY`: Your Gemini API key
4. Configuration files:
   - `.gemini/settings.json` - Settings
   - `.github/workflows/gemini-review.yml` - GitHub Action

**Note:** Gemini Code Assist for GitHub is in preview

### ❓ 5. GitHub Copilot / Codex
**Status:** Limited PR review capabilities

**Setup:**
- GitHub Copilot is primarily for code generation, not PR reviews
- For PR reviews, consider using GitHub Copilot Chat with custom instructions
- Add AGENTS.md reference in PR templates

## Coding Guidelines Location

All tools reference: `Assets/AGENTS.md`

This file contains:
- SOLID principles with examples
- C# naming conventions
- Unity-specific best practices
- Command Query Separation
- Zero Garbage Collection patterns
- VContainer dependency injection

## Required GitHub Secrets

Add these secrets in: **Settings → Secrets and variables → Actions**

| Secret Name | Used By | Required |
|------------|---------|----------|
| `OPENAI_KEY` | Qodo | ✅ Yes |
| `GEMINI_API_KEY` | Gemini | Optional |
| `GITHUB_TOKEN` | All | Auto-provided |

## Testing the Setup

1. Create a test PR with intentional violations:
   ```csharp
   public class test {  // Wrong: lowercase class name
       public void Update() {  // Wrong: public Unity method
           var x = 5;  // Wrong: using var
       }
   }
   ```

2. Each tool should flag violations referencing AGENTS.md

3. Expected output format:
   ```
   VIOLATION: Naming Convention
   LINE(S): 1
   ISSUE: Class name should use PascalCase
   SOLUTION: public class Test {
   GUIDELINE: AGENTS.md Section 1 - Naming Conventions
   ```

## Tool Comparison

| Feature | Qodo | CodeRabbit | Graphite | Gemini |
|---------|------|------------|----------|--------|
| Native AGENTS.md | ✅ | ✅ | ⚠️ | ⚠️ |
| Custom Instructions | ✅ | ✅ | ✅ | ✅ |
| SOLID Checking | ✅ | ✅ | ✅ | ✅ |
| Unity Patterns | ⚙️ | ⚙️ | ⚙️ | ⚙️ |
| Free Tier | ✅ | ✅ | ⚠️ | ⚠️ |

Legend:
- ✅ Full support
- ⚙️ Via custom config
- ⚠️ Limited/requires setup

## Maintenance

When updating coding standards:
1. Edit `Assets/AGENTS.md`
2. All tools will automatically use updated guidelines
3. No need to update tool-specific configs (except for major changes)

## Troubleshooting

**Tools not reading AGENTS.md:**
- Ensure file is at `Assets/AGENTS.md` (matches filePatterns in configs)
- Check that file is committed to repository
- Verify tool has repository read permissions

**Violations not detected:**
- Review custom instructions in tool configs
- Ensure API keys are valid
- Check GitHub Actions logs for errors

## Additional Resources

- [Qodo Documentation](https://qodo-merge-docs.qodo.ai/)
- [CodeRabbit Configuration](https://docs.coderabbit.ai/)
- [Graphite Setup](https://graphite.dev/docs)
- [Gemini Code Assist](https://developers.google.com/gemini-code-assist)
- [AGENTS.md Standard](https://agentsmd.io/)

