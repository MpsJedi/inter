namespace InterviewPractice._02_SOLID;

// ============================================================
// O — Open/Closed Principle
// Open for EXTENSION, closed for MODIFICATION.
// Adding a new browser should NOT require changing existing factory code.
// Add a new class — don't touch old ones.
// ============================================================

// ---- EXAMPLE: BAD (violates OCP) ----

public class DriverFactory_Bad
{
    public string Create(string browser)
    {
        // Every new browser = modify this method = risky
        if (browser == "chrome") return "ChromeDriver";
        if (browser == "firefox") return "FirefoxDriver";
        throw new ArgumentException("Unknown browser");
    }
}

// ---- EXAMPLE: GOOD (follows OCP) ----

public interface IBrowserFactory
{
    string Create();
}

public class ChromeFactory : IBrowserFactory
{
    public string Create() => "ChromeDriver started";
}

public class FirefoxFactory : IBrowserFactory
{
    public string Create() => "FirefoxDriver started";
}

// To add Edge: create EdgeFactory — zero changes to existing code
public class EdgeFactory : IBrowserFactory
{
    public string Create() => "EdgeDriver started";
}

// ---- TODO: Rewrite from memory ----
// Scenario: you have a report generator that currently supports HTML and JSON.
// You need to add XML support.
//
// BAD approach (don't do this — just write it to see WHY it's bad):
//   public class ReportGenerator_Bad
//   {
//       public void Generate(string format)
//       {
//           if (format == "html") ...
//           if (format == "json") ...
//           // adding xml = modifying this method = OCP violation
//       }
//   }
//
// GOOD approach — do this:
//   1. Create interface IReportGenerator with method: void Generate()
//   2. Create HtmlReportGenerator implementing it
//   3. Create JsonReportGenerator implementing it
//   4. Create XmlReportGenerator implementing it (the new one — added without touching old code)

// YOUR CODE BELOW:
