namespace InterviewPractice._03_CSharp;

// ============================================================
// CONCEPT: async / await
// Non-blocking code for I/O operations (HTTP calls, file reads).
// The thread is FREE while waiting for the response.
// Used in every API test.
// ============================================================

public class AsyncExamples
{
    // ---- EXAMPLE: async method ----

    // Returns Task<string> because it's async and returns a string
    public async Task<string> GetUserNameAsync(int userId)
    {
        // Simulating an HTTP call (in real code: await httpClient.GetAsync(...))
        await Task.Delay(100); // non-blocking wait
        return $"User_{userId}";
    }

    // Returns Task (no return value — void equivalent for async)
    public async Task DeleteUserAsync(int userId)
    {
        await Task.Delay(50);
        Console.WriteLine($"Deleted user {userId}");
    }

    // ---- EXAMPLE: calling async methods ----

    public async Task RunTest()
    {
        // CORRECT: await the async method
        var name = await GetUserNameAsync(1);
        Console.WriteLine($"Got: {name}");

        await DeleteUserAsync(1);
        Console.WriteLine("Done");
    }

    // ---- WHAT NOT TO DO ----

    public void BadExample()
    {
        // WRONG: .Result blocks the thread — can deadlock
        // var name = GetUserNameAsync(1).Result;

        // WRONG: .Wait() same problem
        // DeleteUserAsync(1).Wait();

        // CORRECT: make the calling method async instead
    }
}

// ---- TODO: Rewrite from memory ----
// 1. Create an async method "FetchProductAsync(int productId)" that:
//    - returns Task<string>
//    - awaits Task.Delay(200) (simulates HTTP call)
//    - returns "Product_42" (or whatever the id is)
//
// 2. Create an async method "RunProductTest()" that:
//    - calls FetchProductAsync(42) with await
//    - prints the result
//    - is itself async returning Task
//
// 3. In a comment, explain: why can't you use async void here?
//    (Hint: test runners can't track void async methods — exceptions are swallowed)

// YOUR CODE BELOW:
