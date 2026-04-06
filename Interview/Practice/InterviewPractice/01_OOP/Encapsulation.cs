namespace InterviewPractice._01_OOP;

// ============================================================
// CONCEPT: Encapsulation
// Hide internal state. Only expose what's needed via public methods.
// Private fields + public methods = encapsulation.
// ============================================================

// ---- EXAMPLE (read this, then cover it and do the TODO below) ----

public class LoginPage_Example
{
    // Private — nobody outside can touch this directly
    private readonly string _url = "https://example.com/login";
    private string _currentUser;

    // Public method — controlled access
    public void Login(string username, string password)
    {
        _currentUser = username;
        Console.WriteLine($"Logging in as {username} at {_url}");
    }

    public string GetCurrentUser() => _currentUser;
}

// ---- TODO: Rewrite this from memory ----
// Create a class called "RegistrationPage"
// It should have:
//   - private field: _baseUrl (set to "https://example.com/register")
//   - private field: _lastRegisteredEmail (string)
//   - public method: Register(string email, string password)
//       → sets _lastRegisteredEmail = email, prints "Registering {email}"
//   - public method: GetLastRegisteredEmail() → returns _lastRegisteredEmail

// YOUR CODE BELOW:
