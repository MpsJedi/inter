namespace InterviewPractice._04_Selenium_POM;

public class DashboardPage : BasePage
{
    private const string WelcomeMessageLocator = ".welcome-banner";
    private const string LogoutButtonLocator   = "[data-testid='logout-btn']";

    public DashboardPage(string driver) : base(driver) { }

    public override bool IsLoaded()
    {
        Console.WriteLine("Checking if dashboard is loaded");
        return true;
    }

    public string GetWelcomeMessage()
        => WaitForVisible(WelcomeMessageLocator);

    public LoginPage ClickLogout()
    {
        WaitForClickable(LogoutButtonLocator);
        Console.WriteLine("Clicking logout");
        WaitForUrl("/login");
        return new LoginPage(Driver);
    }
}
