# OOP Principles — C# QA Automation Interview Prep

## The 4 Pillars

---

### 1. Encapsulation
**What it is**: Hiding internal state and exposing only what's necessary via public methods/properties.

**Why it matters**: Prevents external code from directly modifying object state in unexpected ways.

```csharp
// BAD — state is exposed directly
public class LoginPage
{
    public IWebDriver Driver;  // anyone can set this
}

// GOOD — encapsulated
public class LoginPage
{
    private readonly IWebDriver _driver;

    public LoginPage(IWebDriver driver)
    {
        _driver = driver;
    }

    public void Login(string username, string password)
    {
        _driver.FindElement(By.Id("username")).SendKeys(username);
        _driver.FindElement(By.Id("password")).SendKeys(password);
        _driver.FindElement(By.Id("submit")).Click();
    }
}
```

**In your framework**: Page classes encapsulate locators and actions. Tests don't interact with raw Selenium — only with page methods.

---

### 2. Abstraction
**What it is**: Exposing only the relevant details and hiding implementation complexity.

**Why it matters**: Consumers don't need to know HOW something works, only WHAT it does.

```csharp
// Abstract contract — "what can a page do?"
public abstract class BasePage
{
    protected IWebDriver Driver;

    protected BasePage(IWebDriver driver)
    {
        Driver = driver;
    }

    public abstract bool IsLoaded();

    protected IWebElement WaitForElement(By locator)
    {
        // complex wait logic hidden here
        var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(10));
        return wait.Until(ExpectedConditions.ElementIsVisible(locator));
    }
}

// Concrete page — only exposes what tests need
public class LoginPage : BasePage
{
    private readonly By _usernameInput = By.Id("username");

    public LoginPage(IWebDriver driver) : base(driver) { }

    public override bool IsLoaded() => Driver.Url.Contains("/login");

    public void EnterUsername(string username)
    {
        WaitForElement(_usernameInput).SendKeys(username);
    }
}
```

**In your framework**: BasePage is abstraction. Page classes are implementations. Tests only call high-level methods.

---

### 3. Inheritance
**What it is**: A class inherits behavior and state from a parent class.

**Why it matters**: Reuse common behavior across many classes.

```csharp
public class BasePage
{
    protected IWebDriver Driver;

    public BasePage(IWebDriver driver)
    {
        Driver = driver;
    }

    public void NavigateTo(string url) => Driver.Navigate().GoToUrl(url);
}

// LoginPage inherits NavigateTo, Driver from BasePage
public class LoginPage : BasePage
{
    public LoginPage(IWebDriver driver) : base(driver) { }

    public void Login(string user, string pass)
    {
        // use inherited Driver
        Driver.FindElement(By.Id("user")).SendKeys(user);
        Driver.FindElement(By.Id("pass")).SendKeys(pass);
    }
}
```

**In your framework**: `BasePage`, `BaseTest`, `BaseApi` — all tests/pages inherit common setup/teardown/utilities.

**Caution**: Prefer composition over deep inheritance chains. 2-3 levels max.

---

### 4. Polymorphism
**What it is**: The same interface/method behaves differently depending on the actual type at runtime.

**Why it matters**: Lets you write generic code that works with different implementations.

```csharp
// Interface defines the contract
public interface IDriver
{
    IWebDriver Create();
}

public class ChromeDriverFactory : IDriver
{
    public IWebDriver Create() => new ChromeDriver();
}

public class FirefoxDriverFactory : IDriver
{
    public IWebDriver Create() => new FirefoxDriver();
}

// Polymorphism: same method call, different behavior
public class DriverManager
{
    public IWebDriver GetDriver(IDriver factory)
    {
        return factory.Create();  // Chrome or Firefox — doesn't matter here
    }
}
```

**In your framework**: You can swap driver implementations without changing test code. This is also the Factory pattern in action.

---

## How OOP Connects to Everything Else

| OOP Concept | Where Used in Automation |
|---|---|
| Encapsulation | Page classes hide locators and Selenium calls |
| Abstraction | BasePage, BaseTest, IDriver interfaces |
| Inheritance | All pages extend BasePage |
| Polymorphism | Driver factory, browser-agnostic tests |

---

## Interview Questions & Answers

**Q: What are the 4 OOP principles?**
> Encapsulation, Abstraction, Inheritance, Polymorphism. Encapsulation hides internal state. Abstraction exposes only what's needed. Inheritance lets classes reuse behavior from parents. Polymorphism lets different types respond to the same interface differently.

**Q: How do you use OOP in your automation framework?**
> Every page is a class — encapsulation keeps locators private and exposes only actions. BasePage provides shared behavior via inheritance. The driver factory uses polymorphism to create different browsers from the same interface. SOLID principles guide how these classes are structured.

**Q: What's the difference between abstraction and encapsulation?**
> Encapsulation is about *hiding data* — protecting internal state with access modifiers. Abstraction is about *hiding complexity* — showing only a simplified interface. A car: the steering wheel is abstraction (you don't know how it works), but the engine internals being hidden inside the hood is encapsulation.

**Q: When would you use composition over inheritance?**
> When classes share behavior but don't share identity. Instead of inheriting from multiple base classes (which C# doesn't support), inject collaborators. For example, a `ReportHelper` can be composed into multiple test classes rather than inherited, because not all tests are "a type of" ReportHelper.

**Q: What is method overriding vs overloading?**
> Overriding: a subclass provides its own implementation of a virtual/abstract parent method (runtime polymorphism). Overloading: multiple methods with the same name but different parameters in the same class (compile-time). `IsLoaded()` being overridden in each Page class is overriding. `SendKeys(string)` and `SendKeys(string, bool clear)` is overloading.

---

## Common Mistakes to Avoid

- Saying "encapsulation means using private fields" — that's incomplete. It's about the *reason*: protecting integrity.
- Confusing abstract classes and interfaces. In C#: abstract classes can have implementation + state; interfaces are pure contracts (though C# 8+ allows default implementations).
- Inheritance for code reuse alone. Use it only when there's a true "is-a" relationship.
