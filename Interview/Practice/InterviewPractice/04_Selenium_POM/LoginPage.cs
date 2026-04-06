namespace InterviewPractice._04_Selenium_POM;

// ============================================================
// CONCEPT: Page Object Model — Concrete Page
// LoginPage extends BasePage.
// Locators are PRIVATE — tests can't see them.
// Actions return page objects for fluent chaining.
// ============================================================

public class LoginPage : BasePage
{
    // Locators — private, encapsulated
    // Real: private readonly By _username = By.Id("username");
    private const string UsernameLocator = "#username";
    private const string PasswordLocator = "#password";
    private const string SubmitLocator   = "[data-testid='login-btn']";
    private const string ErrorLocator    = ".error-message";

    public LoginPage(string driver) : base(driver) { }

    public override bool IsLoaded()
    {
        // Real: return Driver.Url.Contains("/login");
        Console.WriteLine("Checking if login page is loaded");
        return true;
    }

    // Fluent API — returns 'this' so calls can be chained
    public LoginPage EnterUsername(string username)
    {
        WaitForVisible(UsernameLocator);
        Console.WriteLine($"Entering username: {username}");
        return this; // enables chaining
    }

    public LoginPage EnterPassword(string password)
    {
        WaitForVisible(PasswordLocator);
        Console.WriteLine($"Entering password: ***");
        return this;
    }

    // Returns next page — signals where we go after action
    public DashboardPage ClickLogin()
    {
        WaitForClickable(SubmitLocator);
        Console.WriteLine("Clicking login button");
        WaitForUrl("/dashboard");
        return new DashboardPage(Driver);
    }

    public string GetErrorMessage()
    {
        return WaitForVisible(ErrorLocator);
    }
}

// ---- Usage (how a test calls this) ----
//
// var loginPage = new LoginPage("ChromeDriver");
//
// // Fluent chaining:
// var dashboard = loginPage
//     .EnterUsername("admin")
//     .EnterPassword("secret")
//     .ClickLogin();
//
// Console.WriteLine(dashboard.IsLoaded()); // true

// ---- TODO: Rewrite from memory ----
// Create a "RegistrationPage" that extends BasePage:
//   - Private locators: email, password, confirmPassword, submitButton, successMessage
//   - EnterEmail(string email) → returns this
//   - EnterPassword(string password) → returns this
//   - EnterConfirmPassword(string password) → returns this
//   - ClickRegister() → waits for URL "/welcome", returns new WelcomePage(Driver)
//   - GetSuccessMessage() → returns the success message text
//
// Also create a stub "WelcomePage" that extends BasePage
//   - IsLoaded() returns true

// YOUR CODE BELOW:
