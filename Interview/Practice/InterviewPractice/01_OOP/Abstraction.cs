namespace InterviewPractice._01_OOP;

// ============================================================
// CONCEPT: Abstraction
// Hide HOW something works. Expose only WHAT it does.
// Abstract classes and interfaces = abstraction tools.
// ============================================================

// ---- EXAMPLE ----

public abstract class BasePage_Example
{
    protected string Driver = "FakeDriver"; // simplified for practice

    // Abstract = subclass MUST implement this
    public abstract bool IsLoaded();

    // Concrete = shared logic hidden from callers
    protected void WaitForElement(string locator)
    {
        Console.WriteLine($"Waiting for element: {locator}");
        // complex wait logic hidden here
    }
}

public class LoginPage_Abstraction : BasePage_Example
{
    public override bool IsLoaded()
    {
        Console.WriteLine("Checking if login page is loaded");
        return true; // simplified
    }

    public void Login(string user, string pass)
    {
        WaitForElement("#username"); // caller doesn't know how waiting works
        Console.WriteLine($"Login with {user}");
    }
}

// ---- TODO: Rewrite from memory ----
// 1. Create an abstract class called "BaseApiClient"
//    - protected field: BaseUrl (string)
//    - abstract method: string GetEndpointName()
//    - concrete protected method: void LogRequest(string endpoint)
//      → prints "Sending request to: {endpoint}"
//
// 2. Create a class "UserApiClient" that extends BaseApiClient
//    - sets BaseUrl = "https://api.example.com"
//    - implements GetEndpointName() → returns "/users"
//    - public method: void FetchUsers() → calls LogRequest(GetEndpointName())

// YOUR CODE BELOW:
