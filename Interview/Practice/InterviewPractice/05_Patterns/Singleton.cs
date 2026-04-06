namespace InterviewPractice._05_Patterns;

// ============================================================
// PATTERN: Singleton
// Only ONE instance exists for the lifetime of the app.
// Used for: driver manager, config reader, logger.
//
// IMPORTANT: For parallel tests — use ThreadLocal<T> instead
// of a global singleton, so each thread gets its own instance.
// ============================================================

// ---- EXAMPLE: Basic Singleton (single-threaded) ----

public sealed class ConfigManager
{
    // Lazy<T> — instance created only when first accessed (thread-safe)
    private static readonly Lazy<ConfigManager> _instance =
        new Lazy<ConfigManager>(() => new ConfigManager());

    public static ConfigManager Instance => _instance.Value;

    public string BaseUrl { get; } = "https://staging.example.com";
    public string Browser  { get; } = "chrome";

    private ConfigManager()
    {
        // Private constructor — nobody can do: new ConfigManager()
        Console.WriteLine("ConfigManager created (only happens once)");
    }
}

// Usage:
// var url = ConfigManager.Instance.BaseUrl;   // same instance every time
// var url2 = ConfigManager.Instance.BaseUrl;  // same object, not created again

// ---- EXAMPLE: ThreadLocal Singleton (parallel-safe) ----

public static class DriverManager
{
    // Each thread has its own driver — no sharing
    private static readonly ThreadLocal<string> _driver = new ThreadLocal<string>();

    public static string Driver
    {
        get => _driver.Value;
        set => _driver.Value = value;
    }

    public static void Quit()
    {
        Console.WriteLine($"Quitting {_driver.Value}");
        _driver.Value = null;
    }
}

// Usage:
// Thread 1: DriverManager.Driver = "ChromeDriver-Thread1";
// Thread 2: DriverManager.Driver = "ChromeDriver-Thread2";
// They don't interfere with each other.

// ---- TODO: Rewrite from memory ----
// 1. Create a Singleton class "TestLogger"
//    - Should have a static Instance property (use Lazy<T>)
//    - Private constructor
//    - Method: void Log(string message) → prints "[LOG] {message}"
//    - Method: void Error(string message) → prints "[ERROR] {message}"
//
// 2. In comments, show:
//    - How to call Log() from two different places
//    - Why both calls use the same instance (explain Lazy<T> in 1 sentence)
//
// 3. In comments, explain WHY you'd switch to ThreadLocal for parallel tests

// YOUR CODE BELOW:
