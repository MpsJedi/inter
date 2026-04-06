# EPAM QA Automation Engineer — Interview Prep Walkthrough

> Study this in order. Each topic builds on the previous one.
> You have the knowledge — this plan brings it back to the surface fast.

---

## Learning Order & Topic Map

| # | File | Topic | Why it matters |
|---|------|--------|----------------|
| 1 | [01_OOP_Principles.md](01_OOP_Principles.md) | OOP | Foundation of everything. Every class, test, and pattern you write uses OOP. |
| 2 | [02_SOLID_Principles.md](02_SOLID_Principles.md) | SOLID | EPAM loves this. Explains *why* your framework is designed the way it is. |
| 3 | [03_CSharp_DeepDive.md](03_CSharp_DeepDive.md) | C# Core | The language you'll code in. async/await, LINQ, collections, delegates. |
| 4 | [04_Selenium_And_POM.md](04_Selenium_And_POM.md) | Selenium + POM | The core tool. Page Object Model is non-negotiable at EPAM. |
| 5 | [05_Design_Patterns.md](05_Design_Patterns.md) | Design Patterns | Factory, Singleton, Builder, Facade. Show you think like an engineer. |
| 6 | [06_Framework_Design.md](06_Framework_Design.md) | Framework Design | The "describe your framework" question. Critical. |
| 7 | [07_API_Testing.md](07_API_Testing.md) | API Testing | HttpClient / RestSharp. Status codes, serialization, auth. |
| 8 | [08_CICD_And_Git.md](08_CICD_And_Git.md) | CI/CD + Git | Pipelines, parallel execution, rebase vs merge. |
| 9 | [09_Test_Stability.md](09_Test_Stability.md) | Test Stability | Flaky tests, retries, isolation. Senior-level topic. |
| 10 | [10_Interview_Prep.md](10_Interview_Prep.md) | Interview Answers | Word-for-word answers to the 5 hardest questions. |

---

## Quick Topic Descriptions

### 1. OOP Principles
The 4 pillars: **Encapsulation, Abstraction, Inheritance, Polymorphism**.
Every class in your test framework uses these. Without this, nothing else makes sense.
**Know it to**: explain WHY you structure pages and drivers as classes.

---

### 2. SOLID Principles
5 rules for writing maintainable code:
- **S** — Single Responsibility (one class, one job)
- **O** — Open/Closed (open to extend, closed to modify)
- **L** — Liskov Substitution (subtypes replace base types safely)
- **I** — Interface Segregation (small, focused interfaces)
- **D** — Dependency Inversion (depend on abstractions, not concretions)

**Know it to**: explain your framework architecture decisions.

---

### 3. C# Deep Dive
- **async/await** — non-blocking I/O, used in API tests and wait strategies
- **LINQ** — query collections cleanly (filter test data, assertions)
- **Collections** — List, Dictionary, IEnumerable — used everywhere
- **Delegates & Events** — foundation of event-driven code, callbacks

**Know it to**: write clean, idiomatic C# in live coding tasks.

---

### 4. Selenium + Page Object Model
- **Selenium WebDriver** — automates browser interaction
- **Page Object Model** — each page = one class, locators + actions inside
- **Wait strategies** — implicit vs explicit vs fluent waits

**Know it to**: describe and build the UI layer of your framework.

---

### 5. Design Patterns
- **Factory** — creates driver instances (Chrome, Firefox) without hardcoding
- **Singleton** — ensures one driver instance per test run
- **Builder** — constructs complex test data objects step-by-step
- **Facade** — hides complex API/HTTP setup behind a clean interface

**Know it to**: sound like an engineer, not just a tester.

---

### 6. Framework Design
The full picture: Pages / Tests / Drivers / Utils / Config / DI / Logging / Reporting.
This is the "describe your framework" answer assembled into one coherent design.

**Know it to**: answer the #1 EPAM interview question confidently.

---

### 7. API Testing
- **HttpClient** — built-in .NET HTTP client
- **RestSharp** — popular wrapper for cleaner API test code
- Validate status codes, response bodies, schemas, headers, auth tokens

**Know it to**: show you test more than just UI.

---

### 8. CI/CD + Git
- **Git**: rebase vs merge, branching strategy, resolving conflicts
- **CI pipelines**: running tests on pull requests, scheduled runs
- **Parallel execution**: TestNG/NUnit attributes, thread safety

**Know it to**: show tests live in a real delivery pipeline, not just locally.

---

### 9. Test Stability
- What makes tests flaky (timing, state, environment, locators)
- Explicit waits, retry logic, test isolation
- How to debug and fix flaky tests

**Know it to**: show maturity. Junior testers write tests. Seniors make them reliable.

---

### 10. Interview Prep
- "Describe your framework"
- "How do you handle test failures?"
- "How do you scale automation?"
- "What is your test design approach?"
- "Tell me about a challenge you solved"

**Know it to**: land the job.

---

## Connection Map (How Topics Link)

```
OOP
 └── SOLID
      └── Design Patterns (Factory, Singleton, Builder, Facade)
           └── Framework Design
                ├── Selenium + POM  ──> Test Stability
                ├── API Testing     ──> CI/CD
                └── C# Core ────────> All of the above
```

---

## The One Answer Formula

When answering any technical question at EPAM, use this structure:

> **What** it is → **Why** you use it → **How** you implemented it → **Trade-offs**

Example:
> "Page Object Model is a design pattern where each page of the application is represented as a class. I use it because it separates test logic from UI interaction, making tests more maintainable. In my framework, each page extends a BasePage that handles driver initialization. The trade-off is more upfront code, but it pays off when the UI changes — you update one class, not 50 tests."
