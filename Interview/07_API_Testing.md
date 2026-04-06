# API Testing — C# QA Automation Interview Prep

> API tests are faster, more stable, and more targeted than UI tests.
> Know both HttpClient (built-in) and RestSharp (popular library).

---

## Why API Testing Matters

- UI tests are slow and fragile — API tests are 10-100x faster
- Test business logic independently of the UI
- Create and clean up test data efficiently
- Validate contract between frontend and backend

---

## HTTP Basics (Must Know)

### HTTP Methods
| Method | Purpose | Has Body |
|---|---|---|
| GET | Retrieve data | No |
| POST | Create resource | Yes |
| PUT | Replace resource | Yes |
| PATCH | Partial update | Yes |
| DELETE | Delete resource | No |

### Status Codes
| Code | Meaning |
|---|---|
| 200 | OK |
| 201 | Created |
| 204 | No Content (success, no body) |
| 400 | Bad Request (client error, invalid input) |
| 401 | Unauthorized (not authenticated) |
| 403 | Forbidden (authenticated but no permission) |
| 404 | Not Found |
| 409 | Conflict (duplicate, resource already exists) |
| 422 | Unprocessable Entity (valid JSON, invalid business logic) |
| 500 | Internal Server Error |

---

## Option 1: HttpClient (Built-in .NET)

```csharp
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

public class ApiClient
{
    private readonly HttpClient _http;

    public ApiClient(string baseUrl)
    {
        _http = new HttpClient { BaseAddress = new Uri(baseUrl) };
        _http.DefaultRequestHeaders.Add("Accept", "application/json");
    }

    // GET
    public async Task<T> GetAsync<T>(string endpoint)
    {
        var response = await _http.GetAsync(endpoint);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>();
    }

    // POST
    public async Task<HttpResponseMessage> PostAsync<T>(string endpoint, T body)
    {
        var json = JsonSerializer.Serialize(body);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        return await _http.PostAsync(endpoint, content);
    }

    // DELETE
    public async Task<HttpResponseMessage> DeleteAsync(string endpoint)
        => await _http.DeleteAsync(endpoint);

    // Set auth token
    public void SetBearerToken(string token)
    {
        _http.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }
}
```

---

## Option 2: RestSharp (Popular Library)

```csharp
// NuGet: RestSharp
using RestSharp;

public class RestApiClient
{
    private readonly RestClient _client;

    public RestApiClient(string baseUrl)
    {
        _client = new RestClient(baseUrl);
    }

    public async Task<T> GetAsync<T>(string endpoint)
    {
        var request = new RestRequest(endpoint, Method.Get);
        var response = await _client.ExecuteAsync<T>(request);

        if (!response.IsSuccessful)
            throw new Exception($"GET {endpoint} failed: {response.StatusCode}");

        return response.Data;
    }

    public async Task<RestResponse> PostAsync<T>(string endpoint, T body)
    {
        var request = new RestRequest(endpoint, Method.Post);
        request.AddJsonBody(body);
        return await _client.ExecuteAsync(request);
    }

    public void AddAuthHeader(string token)
    {
        _client.AddDefaultHeader("Authorization", $"Bearer {token}");
    }
}
```

---

## Authentication in API Tests

### Bearer Token (JWT)
```csharp
// 1. Login to get token
var loginRequest = new RestRequest("/auth/login", Method.Post);
loginRequest.AddJsonBody(new { username = "admin", password = "pass" });
var loginResponse = await _client.ExecuteAsync(loginRequest);

var token = JObject.Parse(loginResponse.Content)["token"].ToString();

// 2. Use token in subsequent requests
_client.AddDefaultHeader("Authorization", $"Bearer {token}");
```

### Basic Auth
```csharp
var options = new RestClientOptions("https://api.example.com")
{
    Authenticator = new HttpBasicAuthenticator("username", "password")
};
var client = new RestClient(options);
```

### API Key
```csharp
_client.AddDefaultHeader("X-API-Key", "your-api-key");
// OR as query parameter
request.AddQueryParameter("api_key", "your-api-key");
```

---

## Deserialization

```csharp
// Model matching the JSON response
public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
}

// System.Text.Json (built-in)
var user = JsonSerializer.Deserialize<User>(responseBody, new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true  // handles camelCase JSON → PascalCase C#
});

// Newtonsoft.Json (NuGet — more flexible)
var user = JsonConvert.DeserializeObject<User>(responseBody);
```

---

## Writing API Tests (NUnit)

```csharp
[TestFixture]
public class UserApiTests
{
    private RestApiClient _client;
    private int _createdUserId;

    [SetUp]
    public async Task Setup()
    {
        _client = new RestApiClient("https://api.example.com");
        await _client.AuthenticateAsync("admin", "password");
    }

    [Test]
    public async Task GetUser_ExistingId_Returns200WithUser()
    {
        var response = await _client.GetRawAsync("/users/1");

        Assert.That((int)response.StatusCode, Is.EqualTo(200));

        var user = JsonSerializer.Deserialize<User>(response.Content,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.Multiple(() =>
        {
            Assert.That(user.Id, Is.EqualTo(1));
            Assert.That(user.Name, Is.Not.Null.Or.Empty);
            Assert.That(user.Email, Does.Contain("@"));
        });
    }

    [Test]
    public async Task CreateUser_ValidData_Returns201()
    {
        var newUser = new UserBuilder()
            .WithUsername("testuser")
            .WithEmail("test@example.com")
            .Build();

        var response = await _client.PostAsync("/users", newUser);

        Assert.That((int)response.StatusCode, Is.EqualTo(201));

        var created = JsonSerializer.Deserialize<User>(response.Content, ...);
        _createdUserId = created.Id;  // save for cleanup

        Assert.That(created.Id, Is.GreaterThan(0));
    }

    [Test]
    public async Task GetUser_NonExistentId_Returns404()
    {
        var response = await _client.GetRawAsync("/users/99999");
        Assert.That((int)response.StatusCode, Is.EqualTo(404));
    }

    [Test]
    public async Task CreateUser_MissingEmail_Returns400()
    {
        var response = await _client.PostAsync("/users", new { username = "bob" });
        Assert.That((int)response.StatusCode, Is.EqualTo(400));
    }

    [TearDown]
    public async Task Cleanup()
    {
        if (_createdUserId > 0)
            await _client.DeleteAsync($"/users/{_createdUserId}");
    }
}
```

---

## What to Validate in API Tests

```csharp
// 1. Status code
Assert.That((int)response.StatusCode, Is.EqualTo(200));

// 2. Response body fields
Assert.That(user.Id, Is.EqualTo(expectedId));
Assert.That(user.Email, Does.Contain("@"));
Assert.That(user.Name, Is.Not.Null.And.Not.Empty);

// 3. Response headers
Assert.That(response.ContentType, Does.Contain("application/json"));

// 4. Response time (performance)
Assert.That(response.ResponseTime.TotalMilliseconds, Is.LessThan(2000));

// 5. Collection responses
Assert.That(users, Is.Not.Empty);
Assert.That(users.Count, Is.GreaterThanOrEqualTo(1));
Assert.That(users.All(u => u.Id > 0), Is.True);

// 6. Error response body
var error = JsonSerializer.Deserialize<ErrorResponse>(response.Content, ...);
Assert.That(error.Message, Does.Contain("required"));
```

---

## Schema Validation

```csharp
// NuGet: Newtonsoft.Json.Schema or FluentAssertions
// Validate that response matches expected structure

var schema = JSchema.Parse(@"{
  'type': 'object',
  'properties': {
    'id':    { 'type': 'integer' },
    'name':  { 'type': 'string' },
    'email': { 'type': 'string', 'format': 'email' }
  },
  'required': ['id', 'name', 'email']
}");

var json = JObject.Parse(response.Content);
bool isValid = json.IsValid(schema, out IList<string> errors);
Assert.That(isValid, Is.True, string.Join(", ", errors));
```

---

## Interview Questions & Answers

**Q: What do you test in API tests?**
> "Status codes for each scenario (200, 201, 400, 401, 404), response body structure and field values, error messages for negative cases, response headers like Content-Type, and response time for SLA validation. For destructive operations I also validate the resource is actually gone after DELETE."

**Q: What's the difference between HttpClient and RestSharp?**
> "HttpClient is the built-in .NET class — low-level, verbose, but no external dependency. RestSharp is a higher-level wrapper that simplifies common patterns — body serialization, query params, auth headers — with less boilerplate. I prefer RestSharp for test frameworks because it's more readable. For production library code I'd lean toward HttpClient to avoid the dependency."

**Q: How do you handle authentication in API tests?**
> "I authenticate once in [SetUp] — POST to the login endpoint, extract the JWT token from the response, and set it as the default Authorization header for subsequent requests. If tests need different user roles, I have a helper method that authenticates as different users and returns the configured client."

**Q: How do you clean up data created by API tests?**
> "I save resource IDs created during the test and delete them in [TearDown]. Even if the test fails, TearDown runs so state doesn't leak between tests. For test isolation I also sometimes use unique timestamps in names — `testuser_20240115_143022` — so parallel tests don't collide."

**Q: What is the difference between 401 and 403?**
> "401 Unauthorized means the request has no valid credentials — the user is not authenticated (not logged in). 403 Forbidden means the user IS authenticated but doesn't have permission for this resource. A logged-in viewer trying to access admin endpoints gets 403, not 401."

**Q: How do you test an API that is not yet implemented?**
> "I write tests against the agreed API contract (OpenAPI/Swagger spec) first. If the endpoint doesn't exist, the test fails with 404 — which is a valid failure. This is contract-first testing. It also pushes developers to deliver the spec before or alongside the implementation."
