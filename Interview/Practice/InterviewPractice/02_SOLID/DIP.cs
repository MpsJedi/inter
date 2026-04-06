namespace InterviewPractice._02_SOLID;

// ============================================================
// D — Dependency Inversion Principle
// Depend on ABSTRACTIONS, not on concrete implementations.
// Tests should accept IWebDriver, not ChromeDriver.
// This makes code swappable and testable.
// ============================================================

// ---- EXAMPLE: BAD (violates DIP) ----

public class LoginTest_Bad
{
    private string _driver; // imagine this is ChromeDriver

    public LoginTest_Bad()
    {
        _driver = "ChromeDriver"; // hardcoded — tightly coupled
    }

    public void Run()
    {
        Console.WriteLine($"Running test with {_driver}");
    }
}

// ---- EXAMPLE: GOOD (follows DIP) ----

public interface IDriver
{
    string Name { get; }
    void Start();
}

public class ChromeDriver_DIP : IDriver
{
    public string Name => "Chrome";
    public void Start() => Console.WriteLine("Chrome started");
}

public class FirefoxDriver_DIP : IDriver
{
    public string Name => "Firefox";
    public void Start() => Console.WriteLine("Firefox started");
}

public class LoginTest_Good
{
    private readonly IDriver _driver; // depends on abstraction

    // Injected from outside — doesn't know or care which browser
    public LoginTest_Good(IDriver driver)
    {
        _driver = driver;
    }

    public void Run()
    {
        _driver.Start();
        Console.WriteLine($"Running login test with {_driver.Name}");
    }
}

// Usage:
// var test = new LoginTest_Good(new ChromeDriver_DIP());
// var test = new LoginTest_Good(new FirefoxDriver_DIP()); // swap — zero code change in test

// ---- TODO: Rewrite from memory ----
// 1. Create interface ILogger with method: void Log(string message)
// 2. Create ConsoleLogger implementing ILogger → prints to console
// 3. Create FileLogger implementing ILogger → prints "Writing to file: {message}"
// 4. Create class "ApiTest" that:
//    - accepts ILogger via constructor (not a concrete type)
//    - has method RunTest() that logs "Running API test" using the injected logger
// 5. Show in comments how to create ApiTest with ConsoleLogger and with FileLogger

// YOUR CODE BELOW:
