# CI/CD + Git — C# QA Automation Interview Prep

---

## Git — Core Concepts

### Rebase vs Merge (Most Common Interview Question)

Both integrate changes from one branch into another. The difference is history.

**Merge** — creates a merge commit, preserves full history
```
      A---B---C  feature
     /         \
D---E---F---G---H  main (after merge)
```
```bash
git checkout main
git merge feature
```
- Pros: Non-destructive, full history preserved
- Cons: Messy history with many merge commits

**Rebase** — moves commits to the tip of the target branch, rewrites history
```
              A'--B'--C'  feature (rebased)
             /
D---E---F---G  main
```
```bash
git checkout feature
git rebase main
```
- Pros: Clean, linear history
- Cons: Rewrites commit history — NEVER rebase shared/public branches

**Rule of thumb**:
- `merge` for integrating into main (preserves context)
- `rebase` for keeping a feature branch up-to-date with main (before PR)
- Never rebase a branch someone else is working on

---

### Common Git Commands

```bash
# Branching
git checkout -b feature/login-tests      # create and switch to new branch
git branch -d feature/login-tests        # delete local branch

# Staging and committing
git add src/Tests/LoginTests.cs          # stage specific file
git add -p                               # interactive staging (review hunks)
git commit -m "Add login test cases"

# Syncing
git fetch origin                         # download remote changes (don't merge)
git pull origin main                     # fetch + merge
git pull --rebase origin main            # fetch + rebase (cleaner)

# Viewing history
git log --oneline --graph --all          # visual branch history
git diff main...feature/login-tests      # what changed on the feature branch

# Stashing
git stash                                # save uncommitted changes temporarily
git stash pop                            # restore them

# Undoing
git reset --soft HEAD~1                  # undo last commit, keep changes staged
git reset --hard HEAD~1                  # undo last commit, discard changes (DESTRUCTIVE)
git revert <commit-hash>                 # create new commit that undoes a commit (safe)
```

---

### Branching Strategy

```
main          ←─── production-ready code only
  └── develop ←─── integration branch (all features merge here)
        └── feature/login-tests   ←─── your work
        └── feature/api-tests
        └── fix/flaky-dashboard-test
```

**PR Flow**:
1. Create feature branch from develop
2. Write tests, commit
3. Rebase on latest develop before PR
4. Open PR → code review → CI runs tests
5. Merge into develop
6. On release: develop → main

---

## CI/CD Pipelines

### What CI/CD is
- **Continuous Integration (CI)**: Automatically build and test code when changes are pushed
- **Continuous Delivery (CD)**: Automatically deploy tested code to environments

**Why it matters for QA automation**: Tests run automatically on every pull request. You catch regressions before they reach production.

---

### GitHub Actions Example

```yaml
# .github/workflows/tests.yml
name: Automation Tests

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main, develop ]
  schedule:
    - cron: '0 6 * * *'      # also run daily at 6am

jobs:
  test:
    runs-on: ubuntu-latest

    steps:
      - name: Checkout code
        uses: actions/checkout@v3

      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'

      - name: Restore dependencies
        run: dotnet restore

      - name: Build
        run: dotnet build --no-restore

      - name: Install Chrome
        uses: browser-actions/setup-chrome@v1

      - name: Run tests
        run: dotnet test --no-build --logger "trx;LogFileName=results.trx"
        env:
          BASE_URL: ${{ secrets.STAGING_URL }}
          BROWSER: chrome
          HEADLESS: true

      - name: Upload test results
        uses: actions/upload-artifact@v3
        if: always()           # upload even if tests fail
        with:
          name: test-results
          path: '**/results.trx'

      - name: Upload screenshots
        uses: actions/upload-artifact@v3
        if: failure()          # only upload screenshots if tests fail
        with:
          name: screenshots
          path: '**/screenshots/'
```

---

### Azure DevOps Pipeline Example

```yaml
# azure-pipelines.yml
trigger:
  branches:
    include:
      - main
      - develop

pool:
  vmImage: 'ubuntu-latest'

steps:
  - task: DotNetCoreCLI@2
    displayName: 'Restore'
    inputs:
      command: restore

  - task: DotNetCoreCLI@2
    displayName: 'Build'
    inputs:
      command: build

  - task: DotNetCoreCLI@2
    displayName: 'Run Tests'
    inputs:
      command: test
      arguments: '--logger trx --results-directory $(Agent.TempDirectory)'
    env:
      BASE_URL: $(StagingUrl)
      BROWSER: chrome
      HEADLESS: true

  - task: PublishTestResults@2
    displayName: 'Publish Test Results'
    inputs:
      testResultsFormat: 'VSTest'
      testResultsFiles: '$(Agent.TempDirectory)/**/*.trx'
    condition: always()
```

---

## Parallel Test Execution

### NUnit Parallelizable Attribute
```csharp
// Run all tests in this assembly in parallel
[assembly: Parallelizable(ParallelScope.All)]

// Or at class level
[TestFixture]
[Parallelizable(ParallelScope.Children)]  // parallel within the class
public class LoginTests : BaseTest
{
    [Test]
    public void Test1() { ... }

    [Test]
    public void Test2() { ... }
}
```

### Thread-safe driver management for parallel tests
```csharp
public static class DriverManager
{
    private static readonly ThreadLocal<IWebDriver> _driver =
        new ThreadLocal<IWebDriver>();

    public static IWebDriver Driver
    {
        get => _driver.Value;
        set => _driver.Value = value;
    }

    public static void Quit()
    {
        _driver.Value?.Quit();
        _driver.Value = null;
    }
}

// BaseTest uses DriverManager
[SetUp]
public void Setup()
{
    DriverManager.Driver = new ChromeDriverFactory().Create();
}

[TearDown]
public void Teardown()
{
    DriverManager.Quit();
}
```

### What NOT to share between parallel tests
- Static mutable variables (use ThreadLocal or instance variables)
- Shared database state (tests must create/own their data)
- Shared browser sessions
- Files with same name (use unique names per test)

---

## Interview Questions & Answers

**Q: What's the difference between git merge and git rebase?**
> "Merge creates a merge commit and preserves the original history of both branches. Rebase moves my commits to the tip of the target branch and rewrites history to appear linear. I use rebase to keep my feature branch up-to-date with main before opening a PR — it gives a cleaner history. But I never rebase a branch that other people are working on because rewriting shared history causes conflicts for everyone."

**Q: How do your tests run in CI?**
> "On every pull request, the CI pipeline runs the full test suite against the staging environment. Tests run headless using environment variables for base URL and credentials — nothing is hardcoded. Failed tests upload screenshots as artifacts. The PR cannot be merged if tests fail. We also have a nightly scheduled run against production data (read-only tests only)."

**Q: How do you run tests in parallel?**
> "With NUnit's Parallelizable attribute. The key is thread safety — each thread must have its own WebDriver instance. I use ThreadLocal<IWebDriver> in DriverManager so tests don't share a driver. Page objects are instantiated per test. There's no static mutable state. Test data is created fresh per test, never shared."

**Q: How do you handle secrets (passwords, API keys) in CI?**
> "Never hardcoded in the repo. Stored as CI secrets (GitHub Secrets / Azure DevOps variables) and injected as environment variables at pipeline runtime. The config reader picks them up via `AddEnvironmentVariables()`. The code only knows the key name — the value comes from the environment."

**Q: What's git cherry-pick?**
> "It applies a specific commit from one branch to another, without merging the whole branch. Useful when a bug fix needs to be applied to multiple release branches without merging all the other commits."

**Q: What does `git reset --soft` vs `--hard` do?**
> "Both undo commits. --soft keeps the changes staged — useful when you want to redo a commit with a better message or combine commits. --hard discards everything — dangerous because you lose work. I almost always use --soft or git revert (which creates a new undoing commit, safe on shared branches)."
