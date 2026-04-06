namespace InterviewPractice._01_OOP;

// ============================================================
// CONCEPT: Inheritance
// Child class gets behavior from parent class.
// Use when there is a true IS-A relationship.
// LoginPage IS-A BasePage ✓
// Logger IS-A BasePage ✗ (use composition instead)
// ============================================================

// ---- EXAMPLE ----

public class BasePage_Inheritance
{
    protected string Driver = "ChromeDriver";

    public void NavigateTo(string url)
    {
        Console.WriteLine($"[{Driver}] Navigating to {url}");
    }

    public string GetTitle()
    {
        return "Page Title"; // simplified
    }
}

public class DashboardPage : BasePage_Inheritance
{
    // Inherits NavigateTo and GetTitle from BasePage
    // Adds its own behavior:
    public void ClickLogout()
    {
        Console.WriteLine("Clicking logout button");
    }
}

// Usage:
// var page = new DashboardPage();
// page.NavigateTo("https://example.com/dashboard"); // inherited
// page.ClickLogout();                                // own method

// ---- TODO: Rewrite from memory ----
// 1. Create a class "BaseTest"
//    - protected field: Driver (string, value = "TestDriver")
//    - public method: Setup() → prints "Setting up driver: {Driver}"
//    - public method: Teardown() → prints "Quitting driver"
//
// 2. Create a class "LoginTests" that extends BaseTest
//    - public method: TestValidLogin()
//      → calls Setup(), prints "Running valid login test", calls Teardown()
//
// 3. Create a class "ApiTests" that extends BaseTest
//    - public method: TestGetUser()
//      → calls Setup(), prints "Running GET user API test", calls Teardown()

// YOUR CODE BELOW:
