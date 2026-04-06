# Test Stability — C# QA Automation Interview Prep

> This topic separates junior from senior QA engineers.
> Knowing how to PREVENT and FIX flaky tests shows real-world experience.

---

## What is a Flaky Test?

A test that passes sometimes and fails other times with no code change.
Flaky tests are worse than no tests — they erode trust in the test suite.

---

## Root Causes of Flaky Tests

### 1. Timing Issues (Most Common)
Test acts before the page/element is ready.

```csharp
// FLAKY — element might not exist yet
driver.FindElement(By.Id("result")).Text;

// STABLE — wait for element
var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
var result = wait.Until(ExpectedConditions.ElementIsVisible(By.Id("result")));
result.Text;
```

**Fix**: Replace all `Thread.Sleep()` and implicit waits with targeted explicit waits.

---

### 2. Test Order Dependency
Test B depends on data or state created by Test A.

```csharp
// FLAKY — assumes Test A ran first and created the user
[Test]
public void TestB_EditUser() 
{
    // fails if user doesn't exist
    driver.FindElement(By.CssSelector(".user-row")).Click();
}

// STABLE — each test creates its own state
[SetUp]
public async Task Setup()
{
    _testUser = await _apiClient.CreateUserAsync(new UserBuilder().Build());
}

[TearDown]
public async Task Cleanup()
{
    await _apiClient.DeleteUserAsync(_testUser.Id);
}
```

**Fix**: Each test sets up and tears down its own data.

---

### 3. Shared State
Tests share a browser session, database records, or static variables.

```csharp
// FLAKY — static driver shared between parallel tests
public static IWebDriver Driver = new ChromeDriver();

// STABLE — each test thread has its own driver
private static readonly ThreadLocal<IWebDriver> _driver = new ThreadLocal<IWebDriver>();
```

**Fix**: Use ThreadLocal for parallel execution. No static mutable state.

---

### 4. Environment Issues
- External service down or slow
- Network latency spikes
- Different data in different environments

**Fix**: Mock external dependencies in unit tests. Use retry logic for integration tests that hit real services.

---

### 5. Bad Locators
```csharp
// FLAKY — index-based XPath breaks when layout changes
By.XPath("//div[2]/span[3]")

// FLAKY — dynamic CSS class changes with build
By.CssSelector(".btn-abc123")

// STABLE — semantic, stable locator
By.CssSelector("[data-testid='submit-button']")
By.Id("username")
```

---

### 6. Animations and Loading States
```csharp
// FLAKY — tries to click button while it's animating in
driver.FindElement(By.Id("modal-close")).Click();

// STABLE — wait for animation to complete
var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
wait.Until(d => {
    var btn = d.FindElement(By.Id("modal-close"));
    return btn.Displayed && btn.Enabled;
});
driver.FindElement(By.Id("modal-close")).Click();
```

---

## Wait Strategies in Detail

```csharp
public class WaitHelper
{
    private readonly IWebDriver _driver;
    private readonly int _timeoutSeconds;

    public WaitHelper(IWebDriver driver, int timeoutSeconds = 10)
    {
        _driver = driver;
        _timeoutSeconds = timeoutSeconds;
    }

    private WebDriverWait CreateWait(int? timeout = null)
        => new WebDriverWait(_driver, TimeSpan.FromSeconds(timeout ?? _timeoutSeconds));

    public IWebElement WaitForVisible(By locator)
        => CreateWait().Until(ExpectedConditions.ElementIsVisible(locator));

    public IWebElement WaitForClickable(By locator)
        => CreateWait().Until(ExpectedConditions.ElementToBeClickable(locator));

    public bool WaitForInvisible(By locator)
        => CreateWait().Until(ExpectedConditions.InvisibilityOfElementLocated(locator));

    public IWebElement WaitForText(By locator, string text)
        => CreateWait().Until(d => {
            var el = d.FindElement(locator);
            return el.Text.Contains(text) ? el : null;
        });

    public void WaitForUrl(string urlFragment)
        => CreateWait().Until(d => d.Url.Contains(urlFragment));

    public void WaitForPageLoad()
        => CreateWait().Until(d =>
            ((IJavaScriptExecutor)d)
                .ExecuteScript("return document.readyState").ToString() == "complete");

    // Custom wait with fluent polling
    public T WaitFor<T>(Func<IWebDriver, T> condition, int pollMs = 500)
    {
        var wait = new DefaultWait<IWebDriver>(_driver)
        {
            Timeout = TimeSpan.FromSeconds(_timeoutSeconds),
            PollingInterval = TimeSpan.FromMilliseconds(pollMs)
        };
        wait.IgnoreExceptionTypes(typeof(NoSuchElementException),
                                  typeof(StaleElementReferenceException));
        return wait.Until(condition);
    }
}
```

---

## Retry Logic

### Retry for intermittent Selenium exceptions
```csharp
public static class RetryHelper
{
    public static T Execute<T>(Func<T> action, int maxRetries = 3, int delayMs = 1000)
    {
        Exception lastException = null;

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                return action();
            }
            catch (StaleElementReferenceException ex)
            {
                lastException = ex;
                Console.WriteLine($"Attempt {attempt} failed: {ex.Message}. Retrying...");
                Thread.Sleep(delayMs);
            }
        }

        throw new Exception($"Action failed after {maxRetries} attempts", lastException);
    }

    public static void Execute(Action action, int maxRetries = 3, int delayMs = 1000)
        => Execute<object>(() => { action(); return null; }, maxRetries, delayMs);
}

// Usage
var text = RetryHelper.Execute(() =>
    driver.FindElement(By.Id("dynamic-content")).Text
);
```

### NUnit Retry attribute
```csharp
[Test]
[Retry(3)]  // NUnit retries the test up to 3 times on failure
public void FlakyTest()
{
    // NUnit handles the retry automatically
}
```

**Warning**: `[Retry]` is a band-aid. Fix the root cause instead of retrying indefinitely.

---

## Test Isolation Checklist

```
[x] Each test creates its own test data (via API or builder)
[x] Each test cleans up its data in TearDown
[x] No shared browser session between tests
[x] No static mutable state in test classes
[x] Tests don't depend on execution order
[x] No Thread.Sleep — all waits are condition-based
[x] Stable locators (data-testid, semantic IDs)
[x] Tests run in any environment without code changes
```

---

## Debugging Flaky Tests

```csharp
// 1. Log everything to reproduce the issue
Logger.Info($"URL: {driver.Url}");
Logger.Info($"Page title: {driver.Title}");
Logger.Info($"Element present: {driver.FindElements(By.Id("result")).Count > 0}");

// 2. Screenshot at every key step (not just on failure)
public void ClickButton(By locator)
{
    Logger.Info($"Clicking element: {locator}");
    ScreenshotHelper.Take(driver, "before-click");
    WaitForClickable(locator).Click();
    ScreenshotHelper.Take(driver, "after-click");
}

// 3. Capture browser console logs
var logs = driver.Manage().Logs.GetLog(LogType.Browser);
foreach (var entry in logs)
    Logger.Info($"Browser log: [{entry.Level}] {entry.Message}");
```

---

## How to Make Tests Stable — Summary

| Problem | Solution |
|---|---|
| Race conditions | Explicit waits, not Thread.Sleep |
| Stale elements | Re-find after DOM change, fluent wait ignoring StaleElement |
| Order dependency | Each test owns its data (setup/teardown) |
| Shared state in parallel | ThreadLocal driver, no static mutable vars |
| Bad locators | data-testid, semantic IDs |
| Element not scrolled into view | ScrollIntoView before interacting |
| Intercepted click | Wait for overlays to disappear, or JS click |
| Environment flakiness | Retry with exponential backoff, proper timeouts |

---

## Interview Questions & Answers

**Q: How do you handle flaky tests?**
> "First I identify if it's deterministically reproducible or genuinely intermittent. I run it 10-20 times to see the pattern. Then I investigate root cause: is it a timing issue (replace sleep with explicit wait), shared state (isolate with setup/teardown), bad locator (use data-testid), or environment instability (add retry). I fix the root cause — I only use [Retry] as a temporary measure while I investigate."

**Q: What is test isolation and why does it matter?**
> "Test isolation means each test is completely independent — it sets up its own data, doesn't share state with other tests, and cleans up after itself. It matters because without isolation, a failure in one test cascades to others, parallel execution causes race conditions, and tests only pass in a specific order. Isolated tests are reliable, parallelizable, and give precise failure signals."

**Q: What causes StaleElementReferenceException and how do you fix it?**
> "It happens when Selenium has a reference to a DOM element that no longer exists — usually because the page refreshed or the framework re-rendered that component. I fix it by re-finding the element after the DOM update, or using a fluent wait that ignores StaleElementReferenceException and retries until the element is stable."

**Q: How do you reduce test execution time without sacrificing coverage?**
> "Several ways: run tests in parallel with thread-safe driver management; use API calls for setup/teardown instead of UI (creating a user via API is 10x faster than navigating forms); group slow integration tests separately from fast unit tests so fast feedback runs first; prioritize test execution in CI so most-changed areas run first."

**Q: What's the difference between a bug in the app and a flaky test?**
> "A bug reproduces consistently with the same steps. A flaky test fails intermittently under the same conditions. If I see inconsistent results, I first check test infrastructure — waits, state, locators. If the test is solid and still intermittent, it might expose a real race condition in the app, which is actually valuable finding."
