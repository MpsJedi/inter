namespace InterviewPractice._05_Patterns;

// ============================================================
// PATTERN: Builder
// Construct complex objects step by step.
// Avoids giant constructors with many parameters.
// Makes test data setup readable — reads like English.
// ============================================================

// ---- EXAMPLE ----

public class UserModel
{
    public string Username { get; set; }
    public string Password { get; set; }
    public string Email    { get; set; }
    public string Role     { get; set; }
    public bool   IsActive { get; set; }
}

public class UserBuilder
{
    private readonly UserModel _user = new UserModel
    {
        Role     = "viewer",  // sensible defaults
        IsActive = true
    };

    public UserBuilder WithUsername(string username) { _user.Username = username; return this; }
    public UserBuilder WithPassword(string password) { _user.Password = password; return this; }
    public UserBuilder WithEmail(string email)       { _user.Email    = email;    return this; }
    public UserBuilder AsAdmin()                     { _user.Role     = "admin";  return this; }
    public UserBuilder Inactive()                    { _user.IsActive = false;    return this; }

    public UserModel Build() => _user;
}

// Usage:
// var admin = new UserBuilder()
//     .WithUsername("alice")
//     .WithEmail("alice@example.com")
//     .AsAdmin()
//     .Build();
//
// var inactiveViewer = new UserBuilder()
//     .WithUsername("bob")
//     .Inactive()
//     .Build();

// ---- TODO: Rewrite from memory ----
// 1. Create a model class "ProductModel" with properties:
//    - Name (string)
//    - Price (decimal)
//    - Category (string)
//    - InStock (bool)
//
// 2. Create "ProductBuilder" with:
//    - Defaults: Category = "General", InStock = true
//    - Fluent methods: WithName(), WithPrice(), InCategory(), OutOfStock()
//    - Build() method returning ProductModel
//
// 3. Show two usages in comments:
//    - A laptop product at 999.99 in "Electronics" category
//    - An out-of-stock item with just a name

// YOUR CODE BELOW:
