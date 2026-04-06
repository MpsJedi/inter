namespace InterviewPractice._03_CSharp;

// ============================================================
// CONCEPT: LINQ
// Query and transform collections with clean, readable syntax.
// Used in tests to filter elements, assert on lists, transform data.
// ============================================================

public class LinqExercises
{
    public static void RunExamples()
    {
        var users = new List<User>
        {
            new User { Id = 1, Name = "Alice", Role = "admin",  IsActive = true  },
            new User { Id = 2, Name = "Bob",   Role = "viewer", IsActive = true  },
            new User { Id = 3, Name = "Carol", Role = "admin",  IsActive = false },
            new User { Id = 4, Name = "Dave",  Role = "viewer", IsActive = true  },
        };

        // ---- EXAMPLES ----

        // Filter
        var admins = users.Where(u => u.Role == "admin").ToList();
        // → Alice, Carol

        // Transform
        var names = users.Select(u => u.Name).ToList();
        // → ["Alice", "Bob", "Carol", "Dave"]

        // First (throws if not found)
        var firstAdmin = users.First(u => u.Role == "admin");
        // → Alice

        // FirstOrDefault (null if not found — safe)
        var maybe = users.FirstOrDefault(u => u.Id == 99);
        // → null

        // Any — does at least one match?
        bool hasAdmin = users.Any(u => u.Role == "admin"); // true

        // All — do ALL match?
        bool allActive = users.All(u => u.IsActive); // false (Carol is inactive)

        // Count
        int activeCount = users.Count(u => u.IsActive); // 3

        // OrderBy
        var sorted = users.OrderBy(u => u.Name).ToList();
        // → Alice, Bob, Carol, Dave

        // Chain multiple operations
        var activeAdminNames = users
            .Where(u => u.Role == "admin" && u.IsActive)
            .Select(u => u.Name)
            .OrderBy(n => n)
            .ToList();
        // → ["Alice"]
    }
}

public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Role { get; set; }
    public bool IsActive { get; set; }
}

// ---- TODO: Write these LINQ queries yourself ----
// Use the same "users" list from above.
// Write the code, run it mentally (or in a console app), check your answer.

public class LinqTodo
{
    public void Practice()
    {
        var users = new List<User>
        {
            new User { Id = 1, Name = "Alice", Role = "admin",  IsActive = true  },
            new User { Id = 2, Name = "Bob",   Role = "viewer", IsActive = true  },
            new User { Id = 3, Name = "Carol", Role = "admin",  IsActive = false },
            new User { Id = 4, Name = "Dave",  Role = "viewer", IsActive = true  },
        };

        // TODO 1: Get all active users (IsActive == true)
        // var activeUsers = ???

        // TODO 2: Get just the names of all viewers
        // var viewerNames = ???

        // TODO 3: Is there any inactive user?
        // bool hasInactive = ???

        // TODO 4: How many users have role "viewer"?
        // int viewerCount = ???

        // TODO 5: Get the first user with Id > 2 (safe, null if not found)
        // var user = ???

        // TODO 6: Get all user names sorted Z→A (descending)
        // var namesDesc = ???

        // TODO 7: Are ALL users either admin or viewer? (no other roles)
        // bool allValidRoles = ???

        // TODO 8: Get active admins sorted by name, return only their IDs
        // var activeAdminIds = ???
    }

    // ANSWERS (cover this while doing the TODO above):
    // 1: users.Where(u => u.IsActive).ToList()
    // 2: users.Where(u => u.Role == "viewer").Select(u => u.Name).ToList()
    // 3: users.Any(u => !u.IsActive)
    // 4: users.Count(u => u.Role == "viewer")
    // 5: users.FirstOrDefault(u => u.Id > 2)
    // 6: users.Select(u => u.Name).OrderByDescending(n => n).ToList()
    // 7: users.All(u => u.Role == "admin" || u.Role == "viewer")
    // 8: users.Where(u => u.IsActive && u.Role == "admin").OrderBy(u => u.Name).Select(u => u.Id).ToList()
}
