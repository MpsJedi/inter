namespace InterviewPractice._02_SOLID;

// ============================================================
// S — Single Responsibility Principle
// A class should have ONE reason to change.
// If a class does logging + UI interaction + DB access → it has 3 reasons to change.
// Split it into 3 classes.
// ============================================================

// ---- EXAMPLE: BAD (violates SRP) ----

public class LoginPage_Bad
{
    public void Login(string user, string pass)
    {
        Console.WriteLine($"Logging in as {user}"); // UI action ✓
    }

    // NOT this class's job:
    public void Log(string message) => Console.WriteLine($"[LOG] {message}"); // logging ✗
    public string ReadConfig(string key) => "value";                           // config ✗
}

// ---- EXAMPLE: GOOD (follows SRP) ----

public class LoginPage_Good
{
    public void Login(string user, string pass)
    {
        Console.WriteLine($"Logging in as {user}");
    }
}

public class Logger_SRP
{
    public void Log(string message) => Console.WriteLine($"[LOG] {message}");
}

public class ConfigReader_SRP
{
    public string Read(string key) => "value";
}

// ---- TODO: Rewrite from memory ----
// You have this BAD class — split it into proper SRP-compliant classes:
//
// public class TestHelper_Bad
// {
//     public void ClickButton(string id) { Console.WriteLine($"Clicking {id}"); }
//     public void SaveScreenshot(string path) { Console.WriteLine($"Saving to {path}"); }
//     public string GetBaseUrl() { return "https://example.com"; }
//     public void SendReport(string to) { Console.WriteLine($"Sending report to {to}"); }
// }
//
// Create:
//   - ButtonHelper   → ClickButton()
//   - ScreenshotHelper → SaveScreenshot()
//   - ConfigHelper   → GetBaseUrl()
//   - ReportSender   → SendReport()

// YOUR CODE BELOW:
