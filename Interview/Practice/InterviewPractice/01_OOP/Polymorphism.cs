namespace InterviewPractice._01_OOP;

// ============================================================
// CONCEPT: Polymorphism
// Same interface/method → different behavior depending on the actual type.
// "Poly" = many, "morph" = forms.
// Runtime decides which version to call based on the actual object.
// ============================================================

// ---- EXAMPLE ----

public interface IDriverFactory_Example
{
    string Create(); // returns browser name (simplified)
}

public class ChromeFactory_Example : IDriverFactory_Example
{
    public string Create() => "ChromeDriver started";
}

public class FirefoxFactory_Example : IDriverFactory_Example
{
    public string Create() => "FirefoxDriver started";
}

public class TestRunner_Example
{
    // Accepts ANY factory — doesn't care which browser
    public void StartTest(IDriverFactory_Example factory)
    {
        var driver = factory.Create(); // Chrome or Firefox — decided at runtime
        Console.WriteLine($"Test started with: {driver}");
    }
}

// Usage:
// var runner = new TestRunner_Example();
// runner.StartTest(new ChromeFactory_Example());   // "Test started with: ChromeDriver started"
// runner.StartTest(new FirefoxFactory_Example());  // "Test started with: FirefoxDriver started"

// ---- TODO: Rewrite from memory ----
// 1. Create an interface "IPageValidator"
//    - method: bool Validate()
//
// 2. Create class "LoginPageValidator" implementing IPageValidator
//    - Validate() prints "Validating login page" and returns true
//
// 3. Create class "DashboardPageValidator" implementing IPageValidator
//    - Validate() prints "Validating dashboard page" and returns true
//
// 4. Create class "ValidationRunner"
//    - method: void Run(IPageValidator validator)
//      → calls validator.Validate() and prints the result
//
// 5. In a comment at the bottom, show how you'd call Run() with both validators

// YOUR CODE BELOW:
