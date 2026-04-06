# Selenium + Page Object Model — C# QA Automation Interview Prep

---

## Selenium WebDriver — Core Concepts

### What it is
Selenium WebDriver is a browser automation API. It controls browsers (Chrome, Firefox, Edge) programmatically via the WebDriver protocol.

### Basic Setup in C#
```csharp
// NuGet: Selenium.WebDriver, Selenium.WebDriver.ChromeDriver
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

var driver = new ChromeDriver();
driver.Navigate().GoToUrl("https://example.com");
driver.Quit();
```

---

## Locator Strategies

```csharp
// By priority (most stable to least stable):
By.Id("username")                         // most stable — unique per page
By.CssSelector("#username")              // fast, flexible
By.CssSelector(".btn-primary")           // by class
By.CssSelector("form > button[type='submit']")  // structural
By.XPath("//button[@data-testid='submit']")     // when CSS can't reach it
By.Name("email")
By.LinkText("Sign In")
By.PartialLinkText("Sign")
By.TagName("h1")                         // least stable — avoid for specific elements
By.ClassName("error-message")            // fragile if multiple classes

// data-testid is the gold standard for automation-friendly locators
By.CssSelector("[data-testid='login-button']")
```

**Interview tip**: Always mention that you prefer `data-testid` attributes when working with developers, as they don't change with styling/restructuring.

---

## Element Interactions

```csharp
var element = driver.FindElement(By.Id("username"));

element.Click();
element.SendKeys("admin");
element.Clear();
element.Submit();

string text = element.Text;
string value = element.GetAttribute("value");
string placeholder = element.GetAttribute("placeholder");
bool isDisplayed = element.Displayed;
bool isEnabled = element.Enabled;
bool isSelected = element.Selected;  // for checkboxes/radio

// Multiple elements
IReadOnlyList<IWebElement> items = driver.FindElements(By.CssSelector(".item"));
```

---

## Wait Strategies (Critical Topic)

### Three types of waits

**1. Implicit Wait** — global setting, waits up to N seconds for any FindElement call
```csharp
driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
// Applied globally — every FindElement waits up to 10s if not found immediately
```
Problem: Mixes with explicit waits and creates unpredictable timing. Use only one approach.

**2. Explicit Wait** — wait for a specific condition on a specific element
```csharp
var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

// Wait until element is visible
var element = wait.Until(ExpectedConditions.ElementIsVisible(By.Id("result")));

// Wait until element is clickable
var btn = wait.Until(ExpectedConditions.ElementToBeClickable(By.Id("submit")));

// Wait until text appears
wait.Until(d => d.FindElement(By.Id("status")).Text == "Success");

// Wait until URL contains
wait.Until(d => d.Url.Contains("/dashboard"));
```

**3. Fluent Wait** — explicit wait with polling interval and ignored exceptions
```csharp
var wait = new DefaultWait<IWebDriver>(driver)
{
    Timeout = TimeSpan.FromSeconds(15),
    PollingInterval = TimeSpan.FromMilliseconds(500)
};
wait.IgnoreExceptionTypes(typeof(NoSuchElementException));

var element = wait.Until(d => d.FindElement(By.Id("dynamic-content")));
```

**Best practice**: Use explicit waits. Avoid implicit waits (they interact badly with explicit waits). Build a `WaitHelper` or put waits in `BasePage`.

---

## Page Object Model (POM)

### What it is
A design pattern where each web page (or component) is represented as a class.
- Locators live in the page class
- Actions live as methods in the page class
- Tests call page methods — never raw Selenium

### Why it matters
- **Maintainability**: UI changes = update one class, not every test
- **Readability**: Tests read like specifications
- **Reusability**: Same page methods used across multiple tests

### Structure
```
/Pages
  BasePage.cs
  LoginPage.cs
  DashboardPage.cs
  ProductPage.cs
/Tests
  LoginTests.cs
  ProductTests.cs
```

### BasePage
```csharp
public abstract class BasePage
{
    protected IWebDriver Driver;
    protected WebDriverWait Wait;

    protected BasePage(IWebDriver driver)
    {
        Driver = driver;
        Wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
    }

    protected IWebElement WaitForVisible(By locator)
        => Wait.Until(ExpectedConditions.ElementIsVisible(locator));

    protected IWebElement WaitForClickable(By locator)
        => Wait.Until(ExpectedConditions.ElementToBeClickable(locator));

    protected void WaitForUrl(string urlPart)
        => Wait.Until(d => d.Url.Contains(urlPart));

    public abstract bool IsLoaded();
}
```

### LoginPage
```csharp
public class LoginPage : BasePage
{
    // Locators — private, encapsulated
    private readonly By _usernameInput = By.Id("username");
    private readonly By _passwordInput = By.Id("password");
    private readonly By _submitButton  = By.CssSelector("[data-testid='login-btn']");
    private readonly By _errorMessage  = By.CssSelector(".error-message");

    public LoginPage(IWebDriver driver) : base(driver) { }

    public override bool IsLoaded() => Driver.Url.Contains("/login");

    // Actions — return page objects for fluent chaining
    public LoginPage EnterUsername(string username)
    {
        WaitForVisible(_usernameInput).Clear();
        WaitForVisible(_usernameInput).SendKeys(username);
        return this;
    }

    public LoginPage EnterPassword(string password)
    {
        WaitForVisible(_passwordInput).SendKeys(password);
        return this;
    }

    public DashboardPage ClickLogin()
    {
        WaitForClickable(_submitButton).Click();
        WaitForUrl("/dashboard");
        return new DashboardPage(Driver);
    }

    public string GetErrorMessage()
        => WaitForVisible(_errorMessage).Text;
}
```

### Test using POM
```csharp
[TestFixture]
public class LoginTests
{
    private IWebDriver _driver;
    private LoginPage _loginPage;

    [SetUp]
    public void Setup()
    {
        _driver = new ChromeDriver();
        _driver.Navigate().GoToUrl("https://example.com/login");
        _loginPage = new LoginPage(_driver);
    }

    [Test]
    public void ValidLogin_RedirectsToDashboard()
    {
        var dashboard = _loginPage
            .EnterUsername("admin")
            .EnterPassword("secret")
            .ClickLogin();

        Assert.That(dashboard.IsLoaded(), Is.True);
    }

    [Test]
    public void InvalidLogin_ShowsError()
    {
        _loginPage
            .EnterUsername("wrong")
            .EnterPassword("wrong")
            .ClickLogin();  // stays on login page

        Assert.That(_loginPage.GetErrorMessage(), Does.Contain("Invalid credentials"));
    }

    [TearDown]
    public void Teardown() => _driver.Quit();
}
```

---

## Page Factory (Alternative to manual By locators)

```csharp
using OpenQA.Selenium.Support.PageObjects;

public class LoginPage : BasePage
{
    [FindsBy(How = How.Id, Using = "username")]
    private IWebElement _usernameInput;

    [FindsBy(How = How.Id, Using = "password")]
    private IWebElement _passwordInput;

    public LoginPage(IWebDriver driver) : base(driver)
    {
        PageFactory.InitElements(driver, this);  // initializes the fields
    }
}
```

**Note**: PageFactory is available but less popular now. Manual `By` locators give more control and are easier to add explicit waits to.

---

## Common Selenium Problems & Solutions

| Problem | Cause | Solution |
|---|---|---|
| `StaleElementReferenceException` | DOM refreshed after finding element | Re-find element or use explicit wait |
| `ElementClickInterceptedException` | Another element is on top | Scroll into view, wait for overlay to disappear |
| `NoSuchElementException` | Element doesn't exist yet | Add explicit wait |
| `ElementNotInteractableException` | Element exists but not visible/enabled | Wait for visibility/clickability |
| Flaky tests | Race conditions, timing issues | Replace implicit waits with explicit, stabilize selectors |

```csharp
// Scroll element into view
((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", element);

// Click via JS (when normal click intercepted)
((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", element);

// Handle StaleElement with retry
public IWebElement FindWithRetry(By locator, int retries = 3)
{
    for (int i = 0; i < retries; i++)
    {
        try { return driver.FindElement(locator); }
        catch (StaleElementReferenceException) { Thread.Sleep(500); }
    }
    throw new Exception($"Element {locator} not found after {retries} retries");
}
```

---

## Interview Questions & Answers

**Q: What is Page Object Model and why do you use it?**
> "POM is a design pattern where each page of the application is a class. Locators and UI actions are defined inside the page class, and tests only call those methods. I use it because it separates test logic from UI interaction — if a locator changes, I update it in one place rather than across dozens of tests. It also makes tests more readable because they describe user behavior, not Selenium calls."

**Q: What's the difference between implicit and explicit waits?**
> "Implicit waits apply globally to every FindElement call — the driver retries for up to N seconds. Explicit waits target a specific condition on a specific element, like waiting until a button is clickable. I use only explicit waits because mixing both causes unpredictable behavior — if implicit is 10s and explicit is 5s, Selenium may wait up to 15s."

**Q: How do you handle StaleElementReferenceException?**
> "It happens when the DOM refreshes after I've already found a reference to an element. I handle it by re-finding the element after the refresh, or wrapping the interaction in an explicit wait condition. For dynamic content, I use fluent wait with StaleElementReferenceException ignored so it retries until the element is stable."

**Q: How do you choose locators?**
> "By stability: ID first, then CSS selectors, XPath as a last resort. I prefer data-testid attributes agreed on with developers — they don't change with styling. I avoid XPath wherever possible because they're brittle and slow. I never use absolute XPath like `/html/body/div[2]/form`."

**Q: How do you structure your POM for components shared across pages (e.g., header, navbar)?**
> "I create separate classes for reusable components — like NavigationBar or SearchWidget. Page classes can then compose these components rather than inheriting them. The NavBar class has its own locators and methods. Any page that has a navbar gets it as a property."
