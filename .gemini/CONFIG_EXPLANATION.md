# Gemini Code Assist Configuration Schema Explanation

This document explains all available configuration options in `.gemini/config.yaml`.

## Top-Level Configuration Options

### `have_fun` (boolean)
- **Default**: `false`
- **Description**: Enables fun features such as including a poem in the initial pull request summary
- **Current Setting**: `false` ✅ (Good for professional use)

### `ignore_patterns` (array of strings)
- **Default**: `[]` (empty array)
- **Description**: A list of glob patterns for files and directories that Gemini Code Assist should ignore during code reviews. Files matching any pattern will be skipped.
- **Current Setting**: 
  ```yaml
  - '**/Library/**'
  - '**/Temp/**'
  - '**/Packages/**'
  - '**/ProjectSettings/**'
  - '**/*.meta'
  - '**/*.asset'
  - '**/node_modules/**'
  ```
  ✅ **Good** - Covers Unity-specific build artifacts and dependencies

### `code_review` (object)
Configuration object for code review behavior.

#### `code_review.disable` (boolean)
- **Default**: `false`
- **Description**: When set to `true`, completely disables Gemini from acting on pull requests
- **Current Setting**: `false` ✅ (Code review enabled)

#### `code_review.comment_severity_threshold` (string enum)
- **Default**: `MEDIUM`
- **Accepted Values**: `LOW`, `MEDIUM`, `HIGH`, `CRITICAL`
- **Description**: The minimum severity level of review comments to include. Only comments at or above this threshold will be posted.
  - `LOW`: Includes all comments (most verbose)
  - `MEDIUM`: Includes medium, high, and critical comments
  - `HIGH`: Includes only high and critical comments
  - `CRITICAL`: Includes only critical comments (least verbose)
- **Current Setting**: `LOW` ✅ (Good for comprehensive reviews)

#### `code_review.max_review_comments` (integer)
- **Default**: `-1`
- **Description**: Maximum number of review comments to post. Use `-1` for unlimited comments.
- **Current Setting**: `-1` ✅ (Unlimited comments - good for thorough reviews)

#### `code_review.pull_request_opened` (object)
Configuration for actions when a pull request is opened.

##### `code_review.pull_request_opened.help` (boolean)
- **Default**: `false`
- **Description**: When `true`, posts a help message explaining how to interact with Gemini Code Assist when a PR is opened
- **Current Setting**: `false` ✅ (Good if team is already familiar with Gemini)

##### `code_review.pull_request_opened.summary` (boolean)
- **Default**: `true`
- **Description**: When `true`, posts a summary of the pull request when it's opened
- **Current Setting**: `true` ✅ (Good for PR overview)

##### `code_review.pull_request_opened.code_review` (boolean)
- **Default**: `true`
- **Description**: When `true`, automatically performs a code review when a PR is opened
- **Current Setting**: `true` ✅ (Good for automatic reviews)

##### `code_review.pull_request_opened.include_drafts` (boolean)
- **Default**: `true`
- **Description**: When `true`, enables Gemini functionality on draft pull requests. When `false`, Gemini only acts on ready-for-review PRs.
- **Current Setting**: `true` ✅ (Good for early feedback on drafts)

## Style Guide

**Note**: The style guide is NOT configured in `config.yaml`. Instead, Gemini Code Assist automatically looks for a file named `styleguide.md` in the `.gemini/` directory. This file should contain your coding standards and best practices.

## Current Configuration Assessment

Your current configuration is **well-optimized** for:
- ✅ Comprehensive code reviews (LOW threshold, unlimited comments)
- ✅ Automatic reviews on PR open
- ✅ Early feedback on draft PRs
- ✅ Proper exclusion of Unity build artifacts

**Potential Improvements** (optional):
- Consider changing `comment_severity_threshold` to `MEDIUM` if you find reviews too verbose
- Consider setting `include_drafts` to `false` if you only want reviews on ready PRs

## Missing Configuration Options

Based on the schema, there are **no additional configuration options** beyond what's documented above. The schema is complete and your config uses all relevant options appropriately.

