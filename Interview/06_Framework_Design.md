# Framework Design — C# QA Automation Interview Prep

> This is the most important topic for EPAM. "Describe your framework" is guaranteed.
> Read this file until you can explain it without looking.

---

## The Full Framework Architecture

```
/MyAutomationFramework
│
├── /Core                          ← Infrastructure (no tests here)
│   ├── /Drivers
│   │   ├── IDriverFactory.cs
│   │   ├── ChromeDriverFactory.cs
│   │   ├── FirefoxDriverFactory.cs
│   │   └── DriverManager.cs
│   ├── /Config
│   │   ├── IConfigReader.cs
│   │   └── AppSettingsReader.cs
│   ├── /Logging
│   │   └── Logger.cs
│   └── /DI
│       └── ServiceContainer.cs
│
├── /Pages                         ← POM layer
│   ├── BasePage.cs
│   ├── LoginPage.cs
│   ├── DashboardPage.cs
│   └── /Components
│       └── NavigationBar.cs
│
├── /Api                           ← API testing layer
│   ├── ApiClient.cs
│   └── /Services
│       ├── UserService.cs
│       └── OrderService.cs
│
├── /Tests                         ← Test classes
│   ├── BaseTest.cs
│   ├── /UI
│   │   └── LoginTests.cs
│   └── /API
│       └── UserApiTests.cs
│
├── /Utils                         ← Helpers
│   ├── WaitHelper.cs
│   ├── ScreenshotHelper.cs
│   └── TestDataBuilder.cs
│
├── /Models                        ← DTOs and test data models
│   └── User.cs
│
├── /Reports                       ← Reporting config
│   └── ExtentReportManager.cs
│
└── appsettings.json               ← Config file
```

---

## 1. Config Reader

Loads test settings (base URL, browser, credentials) from a file — not hardcoded.

```json
// appsettings.json
{
  "baseUrl": "https://staging.example.com",
  "browser": "chrome",
  "headless": false,
  "timeout": 10,
  "credentials": {
    "adminUser": "admin@example.com",
    "adminPass": "password123"
  }
}
```

```csharp
public interface IConfigReader
{
    string Get(string key);
    T Get<T>(string key);
}

public class AppSettingsReader : IConfigReader
{
    private readonly IConfigurationRoot _config;

    public AppSettingsReader()
    {
        _config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables()  // allows CI to override
            .Build();
    }

    public string Get(string key) => _config[key];
    public T Get<T>(string key) => _config.GetValue<T>(key);
}
```

**Key point**: Environment variables override the JSON file, so CI can inject staging/prod config without changing committed code.

---

## 2. Driver Setup

```csharp
// BaseTest.cs — all test classes inherit this
[TestFixture]
public abstract class BaseTest
{
    protected IWebDriver Driver;
    protected IConfigReader Config;

    [SetUp]
    public void Setup()
    {
        Config = new AppSettingsReader();

        var browser = Config.Get("browser");
        var factory = DriverFactoryResolver.Get(browser);

        Driver = factory.Create();
        Driver.Manage().Window.Maximize();
        Driver.Navigate().GoToUrl(Config.Get("baseUrl"));
    }

    [TearDown]
    public void Teardown()
    {
        // Take screenshot on failure
        if (TestContext.CurrentContext.Result.Outcome != ResultState.Success)
        {
            ScreenshotHelper.Take(Driver, TestContext.CurrentContext.Test.Name);
        }
        Driver?.Quit();
    }
}
```

---

## 3. Dependency Injection (DI)

**What it is**: Instead of classes creating their own dependencies, dependencies are provided from outside.

**Why it matters**: Makes code testable, decoupled, and replaceable.

### Basic DI without a container
```csharp
// Without DI — tightly coupled
public class LoginTests
{
    private IWebDriver _driver = new ChromeDriver();  // hardcoded
}

// With DI — dependencies injected
public class LoginTests : BaseTest
{
    private LoginPage _loginPage;

    [SetUp]
    public new void Setup()
    {
        base.Setup();
        _loginPage = new LoginPage(Driver);  // Driver injected from BaseTest
    }
}
```

### With Microsoft.Extensions.DependencyInjection
```csharp
var services = new ServiceCollection();

// Register dependencies
services.AddSingleton<IConfigReader, AppSettingsReader>();
services.AddTransient<IDriverFactory>(sp =>
    DriverFactoryResolver.Get(sp.GetRequiredService<IConfigReader>().Get("browser")));
services.AddScoped<IWebDriver>(sp =>
    sp.GetRequiredService<IDriverFactory>().Create());

var provider = services.BuildServiceProvider();

// Resolve
var driver = provider.GetRequiredService<IWebDriver>();
```

**Scopes**:
- `Singleton` — one instance for the entire app lifetime
- `Scoped` — one instance per scope (one per test in test context)
- `Transient` — new instance every time

---

## 4. Logging

```csharp
// Using Serilog (popular in .NET)
public static class Logger
{
    private static readonly ILogger _logger = new LoggerConfiguration()
        .MinimumLevel.Debug()
        .WriteTo.Console()
        .WriteTo.File("logs/test-.log", rollingInterval: RollingInterval.Day)
        .CreateLogger();

    public static void Info(string message)    => _logger.Information(message);
    public static void Error(string message)   => _logger.Error(message);
    public static void Debug(string message)   => _logger.Debug(message);
    public static void Warning(string message) => _logger.Warning(message);
}

// Usage in page
public DashboardPage ClickLogin()
{
    Logger.Info("Clicking login button");
    WaitForClickable(_submitButton).Click();
    Logger.Info("Login button clicked, waiting for dashboard");
    WaitForUrl("/dashboard");
    return new DashboardPage(Driver);
}
```

---

## 5. Reporting

```csharp
// ExtentReports — popular .NET test reporting library
public class ExtentReportManager
{
    private static ExtentReports _extent;
    private static ExtentTest _test;

    public static void InitReport(string path)
    {
        var reporter = new ExtentSparkReporter(path);
        _extent = new ExtentReports();
        _extent.AttachReporter(reporter);
    }

    public static ExtentTest CreateTest(string testName)
    {
        _test = _extent.CreateTest(testName);
        return _test;
    }

    public static void Flush() => _extent.Flush();
}
```

---

## 6. Screenshot on Failure

```csharp
public static class ScreenshotHelper
{
    public static void Take(IWebDriver driver, string testName)
    {
        var screenshot = ((ITakesScreenshot)driver).GetScreenshot();
        var path = Path.Combine("screenshots", $"{testName}_{DateTime.Now:yyyyMMdd_HHmmss}.png");
        Directory.CreateDirectory("screenshots");
        screenshot.SaveAsFile(path);
        TestContext.AddTestAttachment(path);  // attaches to NUnit report
    }
}
```

---

## The "Describe Your Framework" Answer (Memorize This)

> "My framework is built in C# with NUnit and Selenium WebDriver, following a layered architecture.
>
> The **Core** layer handles infrastructure — driver creation via a Factory pattern, configuration loaded from appsettings.json with environment variable overrides for CI, and logging via Serilog.
>
> The **Pages** layer implements Page Object Model — every page extends a BasePage that encapsulates wait strategies and common utilities. Locators are private, actions are public methods.
>
> The **Tests** layer has a BaseTest class that handles setup and teardown — initializing the driver, navigating to the base URL, and quitting the driver after each test, with a screenshot captured on failure.
>
> I use Dependency Injection so page objects receive the driver from BaseTest, keeping tests decoupled from driver creation. Design patterns include Factory for driver creation, Singleton (with ThreadLocal for parallel safety) for driver management, and Builder for constructing test data.
>
> For API testing, I use RestSharp with a service layer that wraps HTTP calls behind clean methods.
>
> Tests run in CI via a GitHub Actions pipeline with parallel execution configured through NUnit's Parallelizable attribute. Reports are generated with ExtentReports."

---

## Interview Questions & Answers

**Q: How do you handle test data in your framework?**
> "Test data is managed in layers. Static reference data lives in JSON files loaded by the config reader. Dynamic test data is created via Builder pattern before each test and cleaned up in teardown. For API tests I create data via API calls (not UI) to keep tests fast and isolated. I avoid hardcoding data in test methods."

**Q: How do you support multiple environments (dev, staging, prod)?**
> "appsettings.json holds default values. Environment-specific files (appsettings.staging.json) override them. In CI, environment variables override everything. The test selects the environment via a CI parameter, not code changes."

**Q: How do you implement parallel test execution?**
> "I use NUnit's [Parallelizable] attribute at the test class level. Driver management uses ThreadLocal<IWebDriver> so each thread has its own driver instance. Shared state (config, logger) is either read-only or thread-safe. Page objects are instantiated per test — no static mutable state."

**Q: How do you organize tests?**
> "By feature, then by test type within the feature. Login → LoginPositiveTests, LoginNegativeTests. Each test has one primary assertion and follows AAA: Arrange (set up data/navigate), Act (perform UI action), Assert (verify outcome). Test names follow the convention `MethodUnderTest_StateUnderTest_ExpectedBehavior`."

**Q: How do you keep tests independent?**
> "Each test sets up its own state in [SetUp] and tears it down in [TearDown]. Tests don't share state or depend on execution order. No test assumes another test ran before it. If a test needs data, it creates it — via API calls or the Builder pattern — and deletes it afterward."
