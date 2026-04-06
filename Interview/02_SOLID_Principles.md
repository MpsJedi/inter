# SOLID Principles — C# QA Automation Interview Prep

> EPAM interviewers ask about SOLID to see if you think like a software engineer, not just a tester.
> Always connect each principle to a real example from your framework.

---

## S — Single Responsibility Principle (SRP)

**Rule**: A class should have only one reason to change.

**Bad**:
```csharp
public class LoginPage
{
    // UI interaction — OK
    public void Login(string user, string pass) { ... }

    // Logging — NOT this class's job
    public void LogTestResult(string result) { ... }

    // DB access — definitely NOT this class's job
    public User GetUserFromDb(int id) { ... }
}
```

**Good**:
```csharp
public class LoginPage          { public void Login(...) { ... } }
public class Logger             { public void Log(string msg) { ... } }
public class UserRepository     { public User GetUser(int id) { ... } }
```

**In your framework**: Each page class handles only that page's UI. Logging goes to a Logger. Config goes to ConfigReader. Test setup goes to BaseTest.

---

## O — Open/Closed Principle (OCP)

**Rule**: Classes should be open for extension, closed for modification.

**Bad**:
```csharp
public class DriverFactory
{
    public IWebDriver Create(string browser)
    {
        if (browser == "chrome") return new ChromeDriver();
        if (browser == "firefox") return new FirefoxDriver();
        // Every new browser = modify this class
    }
}
```

**Good**:
```csharp
public interface IBrowserFactory
{
    IWebDriver Create();
}

public class ChromeFactory : IBrowserFactory
{
    public IWebDriver Create() => new ChromeDriver();
}

public class FirefoxFactory : IBrowserFactory
{
    public IWebDriver Create() => new FirefoxDriver();
}

// Add Edge? Create EdgeFactory — never touch existing code
public class EdgeFactory : IBrowserFactory
{
    public IWebDriver Create() => new EdgeDriver();
}
```

**In your framework**: New browsers, new API clients, new reporters — add new classes, don't edit old ones.

---

## L — Liskov Substitution Principle (LSP)

**Rule**: A subclass must be substitutable for its base class without breaking behavior.

**Bad** (violates LSP):
```csharp
public class BasePage
{
    public virtual void NavigateTo(string url)
    {
        Driver.Navigate().GoToUrl(url);
    }
}

public class StaticPage : BasePage
{
    // This page can't navigate — throws exception instead
    public override void NavigateTo(string url)
    {
        throw new NotSupportedException("Static pages don't navigate");
    }
}
```

**Good**:
```csharp
// Don't force NavigateTo on pages that can't navigate
public interface INavigable
{
    void NavigateTo(string url);
}

public class LoginPage : BasePage, INavigable
{
    public void NavigateTo(string url) => Driver.Navigate().GoToUrl(url);
}

// StaticPage doesn't implement INavigable — no violation
public class StaticPage : BasePage { }
```

**In your framework**: If a derived page class can't do something the base class promises, use interfaces to segregate those capabilities (also leads into ISP).

---

## I — Interface Segregation Principle (ISP)

**Rule**: Don't force clients to depend on interfaces they don't use. Keep interfaces small and focused.

**Bad**:
```csharp
public interface IPage
{
    void NavigateTo(string url);
    void Login(string user, string pass);     // Not every page has login
    void AddToCart(string item);              // Not every page has cart
    void SubmitContactForm(string message);   // Not every page has contact form
}
```

**Good**:
```csharp
public interface INavigable   { void NavigateTo(string url); }
public interface ILoginable   { void Login(string user, string pass); }
public interface IShoppable   { void AddToCart(string item); }

public class LoginPage : BasePage, INavigable, ILoginable
{
    public void NavigateTo(string url) => Driver.Navigate().GoToUrl(url);
    public void Login(string user, string pass) { ... }
}

public class ProductPage : BasePage, INavigable, IShoppable
{
    public void NavigateTo(string url) => Driver.Navigate().GoToUrl(url);
    public void AddToCart(string item) { ... }
}
```

**In your framework**: Small, role-specific interfaces for pages. Tests depend only on the interfaces they actually use.

---

## D — Dependency Inversion Principle (DIP)

**Rule**: High-level modules should not depend on low-level modules. Both should depend on abstractions.

**Bad**:
```csharp
public class LoginTest
{
    private ChromeDriver _driver;  // hardcoded concrete type

    public LoginTest()
    {
        _driver = new ChromeDriver();  // tightly coupled
    }
}
```

**Good**:
```csharp
public class LoginTest
{
    private readonly IWebDriver _driver;  // abstraction

    // Injected from outside — could be Chrome, Firefox, mock
    public LoginTest(IWebDriver driver)
    {
        _driver = driver;
    }
}

// In setup (e.g. DI container or BaseTest):
IWebDriver driver = new ChromeDriverFactory().Create();
var test = new LoginTest(driver);
```

**In your framework**: Tests receive IWebDriver, ILogger, IConfig via constructor injection (or a DI container like Microsoft.Extensions.DependencyInjection). This makes tests unit-testable and browser-agnostic.

---

## SOLID in One Table

| Principle | One-line rule | Framework example |
|---|---|---|
| SRP | One class, one job | LoginPage only does login UI |
| OCP | Extend, don't modify | Add new browser factory without touching old code |
| LSP | Subtypes are drop-in replacements | All pages safely extend BasePage |
| ISP | Small, focused interfaces | INavigable, ILoginable separate from each other |
| DIP | Depend on abstractions | Tests accept IWebDriver, not ChromeDriver |

---

## Interview Questions & Answers

**Q: Explain SOLID with a real example from your automation framework.**
> "In my framework, each page class follows SRP — LoginPage only handles login UI actions. Driver creation follows OCP — I have separate factory classes per browser so adding Edge doesn't require modifying existing Chrome or Firefox factories. Tests receive IWebDriver via constructor injection, following DIP, which makes them browser-agnostic and easier to maintain. Interface segregation means page interfaces are small — INavigable for pages that support navigation, separate from ILoginable."

**Q: What happens if you violate SRP?**
> "You get a God Class — one class that does everything. When UI changes, you modify the same file that handles logging and DB access. Every change is risky because a bug in one responsibility breaks the others. It becomes impossible to test in isolation."

**Q: How does DIP relate to Dependency Injection?**
> "DIP is the principle — depend on abstractions. Dependency Injection is the mechanism that implements it. DI is how you supply those abstractions to a class from outside. In my framework, the DI container wires up IWebDriver to ChromeDriver, and tests never know which concrete implementation they received."

**Q: What's the difference between ISP and SRP?**
> "SRP is about classes having one reason to change. ISP is about interfaces — don't put unrelated methods in the same interface. A class can have a single responsibility but still violate ISP if it's forced to implement an interface with methods it doesn't need."

**Q: Can SOLID principles conflict with each other?**
> "They can create tension. OCP says don't modify existing code, but SRP might require you to refactor a bloated class. In practice, you apply them with judgment — not as dogma. The goal is maintainable, testable code, not perfect adherence to every rule."

---

## Key Phrases to Use in Interview

- "single reason to change"
- "open to extension, closed to modification"
- "substitutable without breaking behavior"
- "depend on abstractions, not concretions"
- "interface segregation prevents fat interfaces"
