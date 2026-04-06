# C# Deep Dive — QA Automation Interview Prep

> Focus: async/await, LINQ, Collections, Delegates & Events
> These are the C# features most likely to come up in live coding or technical questions.

---

## 1. async / await

### What it is
A way to write non-blocking code that looks like synchronous code.
The `async` keyword marks a method as asynchronous. `await` suspends execution of that method until the awaited task completes — without blocking the thread.

### Why it matters in automation
- API test calls are I/O-bound (HTTP requests) — async makes them faster
- You don't block the test runner thread while waiting for HTTP responses
- Many modern .NET libraries (HttpClient, Playwright) are async-native

```csharp
// Synchronous — blocks the thread while waiting
public string GetUserSync()
{
    var response = httpClient.GetStringAsync("https://api.example.com/users/1")
                             .Result;  // .Result blocks — AVOID this
    return response;
}

// Asynchronous — thread is free while waiting
public async Task<string> GetUserAsync()
{
    var response = await httpClient.GetStringAsync("https://api.example.com/users/1");
    return response;
}
```

### Rules to know
- `async` method must return `Task`, `Task<T>`, or `void` (avoid `async void` except for event handlers)
- `await` can only be used inside `async` methods
- Avoid `.Result` and `.Wait()` — they can cause deadlocks
- `ConfigureAwait(false)` — avoids capturing synchronization context in library code

```csharp
// NUnit async test
[Test]
public async Task ApiReturnsUser()
{
    var client = new ApiClient();
    var user = await client.GetUserAsync(1);
    Assert.That(user.Id, Is.EqualTo(1));
}
```

### Common mistake
```csharp
// DEADLOCK RISK in ASP.NET / UI context
var result = GetUserAsync().Result;  // blocks current thread, async continues on same thread = deadlock

// CORRECT
var result = await GetUserAsync();
```

---

## 2. LINQ (Language Integrated Query)

### What it is
LINQ lets you query and transform collections using a SQL-like syntax directly in C#.
Two styles: **method syntax** (fluent) and **query syntax**.

### Why it matters in automation
- Filter test data from lists or databases
- Assert conditions on collections of elements
- Transform API response objects

```csharp
var users = new List<User>
{
    new User { Id = 1, Name = "Alice", Role = "admin" },
    new User { Id = 2, Name = "Bob",   Role = "viewer" },
    new User { Id = 3, Name = "Carol", Role = "admin" },
};

// Method syntax (preferred in automation code)
var admins = users
    .Where(u => u.Role == "admin")
    .OrderBy(u => u.Name)
    .Select(u => u.Name)
    .ToList();
// Result: ["Alice", "Carol"]

// Check any element satisfies condition
bool hasAdmin = users.Any(u => u.Role == "admin");  // true

// Find first matching
var firstAdmin = users.First(u => u.Role == "admin");  // Alice
var maybeAdmin = users.FirstOrDefault(u => u.Id == 99); // null — safe

// Count
int adminCount = users.Count(u => u.Role == "admin");  // 2

// Check ALL match
bool allHaveNames = users.All(u => !string.IsNullOrEmpty(u.Name));
```

### In tests — asserting on Selenium element collections
```csharp
var links = driver.FindElements(By.TagName("a"));

// All links should have href
Assert.That(links.All(l => l.GetAttribute("href") != null), Is.True);

// Get text of all items in a list
var itemTexts = driver.FindElements(By.CssSelector(".product-name"))
    .Select(e => e.Text)
    .ToList();

Assert.That(itemTexts, Does.Contain("Laptop"));
```

### Key LINQ methods to know
| Method | Purpose |
|---|---|
| `Where` | Filter |
| `Select` | Transform / project |
| `First` / `FirstOrDefault` | Get first match (throws vs null) |
| `Any` | Does any element match? |
| `All` | Do all elements match? |
| `Count` | Count matching elements |
| `OrderBy` / `OrderByDescending` | Sort |
| `GroupBy` | Group elements |
| `ToDictionary` | Convert to dictionary |
| `ToList` / `ToArray` | Materialize the query |

---

## 3. Collections

### Key types to know

```csharp
// List<T> — ordered, allows duplicates, dynamic size
var browsers = new List<string> { "Chrome", "Firefox" };
browsers.Add("Edge");
browsers.Remove("Firefox");
bool has = browsers.Contains("Chrome");

// Dictionary<TKey, TValue> — key-value pairs, fast lookup
var testData = new Dictionary<string, string>
{
    { "username", "admin" },
    { "password", "secret" }
};
string user = testData["username"];
bool exists = testData.TryGetValue("role", out var role);  // safe access

// HashSet<T> — unique values, fast contains check
var visitedUrls = new HashSet<string>();
visitedUrls.Add("https://example.com/login");
visitedUrls.Add("https://example.com/login");  // duplicate — ignored
// visitedUrls.Count == 1

// Queue<T> — FIFO, useful for test execution ordering
var testQueue = new Queue<string>();
testQueue.Enqueue("test1");
testQueue.Enqueue("test2");
var next = testQueue.Dequeue();  // "test1"

// Stack<T> — LIFO
var history = new Stack<string>();
history.Push("page1");
history.Push("page2");
var last = history.Pop();  // "page2"
```

### IEnumerable vs IList vs ICollection

```csharp
IEnumerable<T>   // read-only, lazy, forward-only iteration
ICollection<T>   // adds Count, Add, Remove to IEnumerable
IList<T>         // adds index access (list[0]) to ICollection
List<T>          // concrete implementation of IList<T>
```

**Best practice**: Accept `IEnumerable<T>` in method parameters (most flexible), return `IReadOnlyList<T>` or `List<T>` from methods.

---

## 4. Delegates & Events

### What a Delegate is
A delegate is a type-safe function pointer — it holds a reference to a method.

```csharp
// Declare delegate type
public delegate void TestAction(string testName);

// Methods that match the signature
public void LogTest(string name) => Console.WriteLine($"Running: {name}");
public void ReportTest(string name) => Console.WriteLine($"Reporting: {name}");

// Assign and invoke
TestAction action = LogTest;
action("LoginTest");  // prints "Running: LoginTest"

// Multicast — combine delegates
action += ReportTest;
action("LoginTest");  // calls BOTH methods
```

### Built-in delegate types (no need to declare your own)
```csharp
// Action — void return, 0-16 parameters
Action<string> log = msg => Console.WriteLine(msg);
log("test started");

// Func — has return value, last type param is return type
Func<string, int> getLength = s => s.Length;
int len = getLength("hello");  // 5

// Predicate<T> — returns bool
Predicate<string> isLong = s => s.Length > 5;
bool result = isLong("hello world");  // true
```

### Lambda expressions
```csharp
// Lambda = anonymous function, often used with delegates and LINQ
Func<int, int, int> add = (a, b) => a + b;
int sum = add(3, 4);  // 7

// Used in LINQ
var evens = numbers.Where(n => n % 2 == 0);
```

### Events
Events are built on delegates — they let one class notify others when something happens.

```csharp
public class TestRunner
{
    // Declare event using EventHandler (built-in delegate)
    public event EventHandler<string> TestStarted;
    public event EventHandler<string> TestPassed;

    public void Run(string testName)
    {
        TestStarted?.Invoke(this, testName);  // notify subscribers
        // ... run test
        TestPassed?.Invoke(this, testName);
    }
}

public class Logger
{
    public void Subscribe(TestRunner runner)
    {
        runner.TestStarted += (sender, name) => Console.WriteLine($"START: {name}");
        runner.TestPassed  += (sender, name) => Console.WriteLine($"PASS:  {name}");
    }
}
```

**In automation**: Events are used in reporting frameworks, browser event listeners, and custom test lifecycle hooks.

---

## Interview Questions & Answers

**Q: What is the difference between async/await and Task.Run?**
> "async/await is for I/O-bound operations — network calls, file reads. The thread is freed while waiting. Task.Run offloads CPU-bound work to a thread pool thread. In API testing, I use async/await for HttpClient calls. Task.Run would be for something like parsing a very large file."

**Q: What's the difference between IEnumerable and IList?**
> "IEnumerable is the most basic collection abstraction — it supports forward-only iteration and is lazy (elements computed on demand). IList adds index-based access and mutation (Add, Remove). In method signatures I accept IEnumerable when I only need to iterate — it's the most flexible since any collection satisfies it."

**Q: What is a lambda expression?**
> "An anonymous function defined inline. It's syntactic sugar for a delegate. `n => n > 5` is a Predicate that returns true if n is greater than 5. LINQ is built almost entirely on lambdas — Where, Select, First all accept them."

**Q: What's the difference between First() and FirstOrDefault()?**
> "First() throws InvalidOperationException if no element matches. FirstOrDefault() returns null (for reference types) or the default value (for value types). In test code I use FirstOrDefault when a missing element is a valid scenario, and First when I assert the element must exist."

**Q: When would you use a Dictionary vs a List?**
> "Dictionary when I need fast lookup by key — O(1) average vs O(n) for List. For test data sets where I need to find a record by ID or name, Dictionary is the right choice. List is for ordered sequences where I iterate or search by value."

**Q: What does ConfigureAwait(false) do?**
> "It tells the awaiter not to resume on the original synchronization context. In library code this prevents deadlocks in environments that have a sync context (like ASP.NET or WPF). In test code, it's usually not needed because test runners don't have a sync context."

---

## Quick Cheat Sheet

```csharp
// async/await
public async Task<T> GetAsync() => await httpClient.GetFromJsonAsync<T>(url);

// LINQ chain
var result = list.Where(x => x.Active).Select(x => x.Name).OrderBy(n => n).ToList();

// Safe dictionary access
dict.TryGetValue(key, out var value);

// Null-safe event raise
MyEvent?.Invoke(this, args);

// Predicate
Predicate<User> isAdmin = u => u.Role == "admin";
```
