namespace InterviewPractice._05_Patterns;

// ============================================================
// PATTERN: Factory
// Create objects without exposing creation logic to the caller.
// The caller asks "give me a driver" — factory decides WHICH driver.
// Follows OCP: add new browser = add new class, touch nothing else.
// ============================================================

// ---- EXAMPLE ----

public interface IDriverFactory
{
    string CreateDriver(); // returns driver name (simplified — real: IWebDriver)
}

public class ChromeDriverFactory : IDriverFactory
{
    public string CreateDriver() => "ChromeDriver [headless=false]";
}

public class FirefoxDriverFactory : IDriverFactory
{
    public string CreateDriver() => "FirefoxDriver";
}

public class HeadlessChromeFactory : IDriverFactory
{
    public string CreateDriver() => "ChromeDriver [headless=true]";
}

// Resolver — picks the right factory from config
public static class DriverFactoryResolver_Example
{
    public static IDriverFactory Get(string browser) => browser.ToLower() switch
    {
        "chrome"          => new ChromeDriverFactory(),
        "firefox"         => new FirefoxDriverFactory(),
        "chrome-headless" => new HeadlessChromeFactory(),
        _                 => throw new ArgumentException($"Unknown browser: {browser}")
    };
}

// Usage:
// var factory = DriverFactoryResolver_Example.Get("chrome");
// var driver  = factory.CreateDriver();

// ---- TODO: Rewrite from memory ----
// Scenario: You need a factory for API clients.
// Different environments use different base URLs.
//
// 1. Create interface IApiClientFactory with method: string CreateClient()
// 2. Create StagingApiClientFactory → CreateClient() returns "ApiClient[https://staging.api.com]"
// 3. Create ProductionApiClientFactory → CreateClient() returns "ApiClient[https://api.com]"
// 4. Create a resolver "ApiClientFactoryResolver" with static method Get(string env)
//    → "staging" → StagingApiClientFactory
//    → "production" → ProductionApiClientFactory
//    → anything else → throw ArgumentException

// YOUR CODE BELOW:
