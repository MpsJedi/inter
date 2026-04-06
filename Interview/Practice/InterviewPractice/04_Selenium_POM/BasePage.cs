namespace InterviewPractice._04_Selenium_POM;

// ============================================================
// CONCEPT: Page Object Model — BasePage
// Every page class inherits from here.
// BasePage handles: driver access, wait utilities, common actions.
// Tests never touch raw Selenium — only page methods.
// ============================================================

// NOTE: This file uses simplified types (no real Selenium NuGet installed).
// In a real project: IWebDriver, By, WebDriverWait, ExpectedConditions.
// The STRUCTURE and PATTERN is what matters here — not the types.

public abstract class BasePage
{
    // In real code: protected IWebDriver Driver;
    protected string Driver;

    protected BasePage(string driver)
    {
        Driver = driver;
    }

    // Every page must define how to check it loaded
    public abstract bool IsLoaded();

    // Shared wait utilities — hidden from tests
    protected string WaitForVisible(string locator)
    {
        // Real: new WebDriverWait(Driver, TimeSpan.FromSeconds(10))
        //           .Until(ExpectedConditions.ElementIsVisible(By.CssSelector(locator)))
        Console.WriteLine($"[WAIT] Waiting for visible: {locator}");
        return locator; // simplified
    }

    protected string WaitForClickable(string locator)
    {
        Console.WriteLine($"[WAIT] Waiting for clickable: {locator}");
        return locator;
    }

    protected void WaitForUrl(string urlFragment)
    {
        Console.WriteLine($"[WAIT] Waiting for URL to contain: {urlFragment}");
    }
}

// ---- TODO: Rewrite from memory ----
// Create your own BasePage (simplified, no real Selenium needed):
//   - protected Driver field (string for now)
//   - Constructor accepting driver
//   - abstract bool IsLoaded()
//   - protected WaitForElement(string locator) → prints wait message
//   - protected void NavigateTo(string url) → prints "Navigating to {url}"

// YOUR CODE BELOW:
