# Design Patterns — C# QA Automation Interview Prep

> These 4 patterns appear in almost every production-grade test framework.
> Know how to implement them AND explain WHY you'd use each one.

---

## 1. Factory Pattern

### What it is
A Factory creates objects without exposing the creation logic to the caller.
The caller asks for "a driver" — the factory decides which driver to create.

### Why use it in automation
- Decouple driver creation from test logic
- Support multiple browsers without changing test code
- Centralize configuration (headless, options, capabilities)

```csharp
// The abstraction — what every factory must deliver
public interface IDriverFactory
{
    IWebDriver Create();
}

// Concrete implementations
public class ChromeDriverFactory : IDriverFactory
{
    private readonly bool _headless;

    public ChromeDriverFactory(bool headless = false)
    {
        _headless = headless;
    }

    public IWebDriver Create()
    {
        var options = new ChromeOptions();
        if (_headless) options.AddArgument("--headless");
        options.AddArgument("--no-sandbox");
        options.AddArgument("--disable-dev-shm-usage");
        return new ChromeDriver(options);
    }
}

public class FirefoxDriverFactory : IDriverFactory
{
    public IWebDriver Create()
    {
        var options = new FirefoxOptions();
        return new FirefoxDriver(options);
    }
}

// A resolver — picks the right factory based on config
public static class DriverFactoryResolver
{
    public static IDriverFactory Get(string browser) => browser.ToLower() switch
    {
        "chrome"  => new ChromeDriverFactory(),
        "firefox" => new FirefoxDriverFactory(),
        "edge"    => new EdgeDriverFactory(),
        _         => throw new ArgumentException($"Unsupported browser: {browser}")
    };
}

// Usage in BaseTest
var factory = DriverFactoryResolver.Get(config["browser"]);
var driver = factory.Create();
```

**OCP connection**: Adding a new browser = new factory class. No modification to existing code.

---

## 2. Singleton Pattern

### What it is
Ensures only ONE instance of a class exists throughout the application lifetime.
All consumers share the same instance.

### Why use it in automation
- One WebDriver instance per test thread (prevents double-init)
- Shared config reader loaded once
- Shared logger or report manager

```csharp
// Thread-safe Singleton using Lazy<T>
public sealed class DriverManager
{
    private static readonly Lazy<DriverManager> _instance =
        new Lazy<DriverManager>(() => new DriverManager());

    public static DriverManager Instance => _instance.Value;

    private IWebDriver _driver;

    private DriverManager() { }  // private constructor — can't new this up

    public IWebDriver GetDriver(IDriverFactory factory)
    {
        if (_driver == null)
        {
            _driver = factory.Create();
        }
        return _driver;
    }

    public void QuitDriver()
    {
        _driver?.Quit();
        _driver = null;
    }
}

// Usage
var driver = DriverManager.Instance.GetDriver(new ChromeDriverFactory());
```

### Parallel test caution
In parallel test execution, a global singleton per process breaks thread isolation.
Use `[ThreadStatic]` or `ThreadLocal<T>` for per-thread driver instances:

```csharp
public class DriverManager
{
    [ThreadStatic]
    private static IWebDriver _driver;

    public static IWebDriver Driver
    {
        get => _driver;
        set => _driver = value;
    }

    public static void Quit()
    {
        _driver?.Quit();
        _driver = null;
    }
}
```

**Singleton interview trap**: Interviewers often ask "is Singleton good for parallel tests?" — the answer is NO for a process-wide singleton. Use ThreadLocal or inject drivers per-test.

---

## 3. Builder Pattern

### What it is
Constructs complex objects step by step. The same build process can create different representations.
Common in test data creation.

### Why use it in automation
- Create test data objects with many optional fields cleanly
- Avoid constructors with 10+ parameters
- Make test setup readable

```csharp
// The object being built
public class User
{
    public string Username { get; set; }
    public string Password { get; set; }
    public string Email    { get; set; }
    public string Role     { get; set; }
    public bool   IsActive { get; set; }
}

// The builder
public class UserBuilder
{
    private readonly User _user = new User
    {
        Role     = "viewer",   // sensible defaults
        IsActive = true
    };

    public UserBuilder WithUsername(string username)
    {
        _user.Username = username;
        return this;
    }

    public UserBuilder WithPassword(string password)
    {
        _user.Password = password;
        return this;
    }

    public UserBuilder WithEmail(string email)
    {
        _user.Email = email;
        return this;
    }

    public UserBuilder AsAdmin()
    {
        _user.Role = "admin";
        return this;
    }

    public UserBuilder Inactive()
    {
        _user.IsActive = false;
        return this;
    }

    public User Build() => _user;
}

// In tests — reads like English
[Test]
public void AdminUser_CanAccessSettings()
{
    var adminUser = new UserBuilder()
        .WithUsername("alice")
        .WithPassword("secure123")
        .WithEmail("alice@example.com")
        .AsAdmin()
        .Build();

    // Use adminUser in test...
}

[Test]
public void InactiveUser_CannotLogin()
{
    var inactiveUser = new UserBuilder()
        .WithUsername("bob")
        .WithPassword("pass")
        .Inactive()
        .Build();
}
```

**Tip**: You can also use Builder for building HTTP requests, API payloads, or browser options.

---

## 4. Facade Pattern

### What it is
Provides a simplified interface to a complex subsystem.
The facade hides the complexity — callers don't need to know how the subsystem works.

### Why use it in automation
- Wrap complex HttpClient setup behind a clean API client
- Simplify multi-step UI flows (login → navigate → perform action) into one call
- Abstract reporting setup

```csharp
// Complex subsystem — lots of setup
public class ApiClient
{
    private readonly HttpClient _http;
    private readonly string _baseUrl;
    private string _authToken;

    public ApiClient(string baseUrl)
    {
        _baseUrl = baseUrl;
        _http = new HttpClient();
        _http.DefaultRequestHeaders.Add("Accept", "application/json");
    }

    public async Task AuthenticateAsync(string username, string password)
    {
        var payload = JsonSerializer.Serialize(new { username, password });
        var content = new StringContent(payload, Encoding.UTF8, "application/json");
        var response = await _http.PostAsync($"{_baseUrl}/auth/login", content);
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync();
        _authToken = JsonDocument.Parse(body).RootElement.GetProperty("token").GetString();
        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _authToken);
    }

    public async Task<T> GetAsync<T>(string endpoint)
    {
        var response = await _http.GetAsync($"{_baseUrl}{endpoint}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>();
    }

    public async Task<HttpResponseMessage> PostAsync<T>(string endpoint, T body)
    {
        var json = JsonSerializer.Serialize(body);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        return await _http.PostAsync($"{_baseUrl}{endpoint}", content);
    }
}

// Facade — hides the subsystem complexity
public class UserApiService
{
    private readonly ApiClient _client;

    public UserApiService(ApiClient client)
    {
        _client = client;
    }

    public async Task<User> GetUserAsync(int id)
        => await _client.GetAsync<User>($"/users/{id}");

    public async Task<HttpResponseMessage> CreateUserAsync(User user)
        => await _client.PostAsync("/users", user);
}

// Test — only talks to the facade
[Test]
public async Task GetUser_ReturnsCorrectUser()
{
    var client = new ApiClient("https://api.example.com");
    await client.AuthenticateAsync("admin", "password");

    var service = new UserApiService(client);
    var user = await service.GetUserAsync(1);

    Assert.That(user.Id, Is.EqualTo(1));
}
```

---

## Pattern Summary & When to Use

| Pattern | Use when... | Automation use case |
|---|---|---|
| Factory | Creating objects whose type varies | Browser driver creation |
| Singleton | One shared instance needed | Driver manager, config, logger |
| Builder | Complex object with many optional fields | Test data, HTTP request bodies |
| Facade | Simplifying a complex subsystem | API client, multi-step UI flow |

---

## Interview Questions & Answers

**Q: Which design patterns do you use in your framework and why?**
> "I use Factory for driver creation — it decouples browser choice from test code and supports multiple browsers via config. Singleton (with ThreadLocal for parallel safety) manages driver lifecycle. Builder builds test data objects cleanly, especially for API payloads with many optional fields. Facade wraps HttpClient setup so API tests only call service-level methods without dealing with auth headers and serialization manually."

**Q: What's the difference between Factory and Abstract Factory?**
> "Factory creates one type of product. Abstract Factory creates families of related products. In a test framework, Factory creates a driver. Abstract Factory would create an entire suite — driver + config + reporter — all matched to a specific environment. In practice, I use simple Factory plus DI for this rather than the full Abstract Factory."

**Q: Is Singleton always a good idea?**
> "No. A global Singleton is an anti-pattern for parallel test execution because all threads share the same driver instance, causing race conditions. I use ThreadLocal<IWebDriver> or inject a driver per-test via DI, which gives Singleton-like efficiency within a thread but isolation between threads."

**Q: How does the Builder pattern differ from just using a constructor?**
> "A constructor with 10 parameters is hard to read and error-prone — parameter order matters. Builder gives named, fluent steps and defaults, so tests read like English. `new UserBuilder().AsAdmin().WithUsername("alice").Build()` is self-documenting. You can also add validation inside Build() that would be awkward in a constructor."

**Q: Where would you use the Facade pattern?**
> "Wherever a subsystem has complicated setup that tests shouldn't care about. API testing is the clearest case — HttpClient requires auth headers, base URLs, serialization, error handling. A facade like `UserApiService` exposes clean methods like `GetUser(id)` and hides all that. Tests read what they test, not how HTTP works."
