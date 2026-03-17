# Result&lt;T&gt; API Documentation

A functional programming library for Railway-Oriented Programming (ROP) in C#. This library provides `Result<T>` and `Error` types for handling success and failure states without throwing exceptions for expected business/validation errors.

---

## Table of Contents

1. [Quick Start](#quick-start)
2. [Core Types](#core-types)
   - [Error](#error)
   - [Result&lt;T&gt;](#resultt)
3. [Extension Methods](#extension-methods)
   - [Match](#match)
   - [Map](#map)
   - [Bind](#bind)
   - [Do](#do)
   - [MapError](#maperror)
4. [Async Support](#async-support)
5. [LINQ Query Syntax](#linq-query-syntax)
6. [Working with Collections](#working-with-collections)
7. [ASP.NET Core MVC Integration](#aspnet-core-mvc-integration)
8. [REST/HttpClient Integration](#resthttpclient-integration)
9. [Common Patterns](#common-patterns)
10. [API Reference](#api-reference)

---

## Quick Start

```csharp
using Fluxys.PSM2.FunctionalExtensions;

// Create a successful result
Result<int> success = Result.New(42);

// Create an error result
Result<int> failure = Error.NotFound("ItemNotFound", "The requested item was not found");

// Chain operations - errors short-circuit automatically
var result = await GetUserAsync(userId)
    .Map(user => user.Email)
    .Bind(email => ValidateEmailAsync(email))
    .Map(email => email.ToLower());

// Handle both outcomes
string message = result.Match(
    value => $"Success: {value}",
    error => $"Error: {error.ErrorMessage}"
);
```

---

## Core Types

### Error

The `Error` class represents a failure state with HTTP-friendly semantics.

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `StatusCode` | `int` | HTTP status code (e.g., 400, 404, 409, 500) |
| `ErrorIdentifier` | `string` | Machine-readable identifier for translations/lookup |
| `ErrorMessage` | `string` | Human-readable message for display |
| `Context` | `string?` | Optional additional context |

#### Creating Errors

```csharp
// Generic factory method
var error = Error.Create(400, "ValidationFailed", "The input is invalid");
var errorWithContext = Error.Create(400, "ValidationFailed", "Invalid input", "Field: email");

// NotFound (404)
var notFound = Error.NotFound("UserNotFound", "User does not exist");
var notFoundWithContext = Error.NotFound("UserNotFound", "User does not exist", "ID: 123");

// Conflict (409) - for business rule violations
var conflict = Error.Conflict("DuplicateEmail", "Email already registered");
var conflictWithContext = Error.Conflict("DuplicateEmail", "Email already registered", "user@example.com");

// From exception (500)
try { /* ... */ }
catch (Exception ex)
{
    return Error.FromException(ex);  // StatusCode=500, Identifier="UnhandledException"
}
```

---

### Result&lt;T&gt;

A readonly struct that holds either a success value of type `T` or an `Error`. It also carries validation messages that accumulate through the chain.

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Value` | `T?` | The success value (null if error) |
| `Error` | `Error?` | The error (null if success) |
| `ValidationMessages` | `ValidationMessage[]` | Accumulated warnings/info messages |

#### Creating Results

```csharp
// Factory methods
Result<string> success = Result.New("hello");
Result<string> failure = Result.Error<string>(someError);

// With validation messages
Result<string> withWarnings = Result.New("value", 
    new ValidationMessage { Identifier = "Warn1", Message = "Check this" });

// Implicit conversions (most common in practice)
public Result<int> ParseNumber(string input)
{
    if (int.TryParse(input, out var value))
        return value;  // Implicit conversion from T to Result<T>
    
    return Error.Conflict("ParseError", $"Cannot parse '{input}'");  // Implicit from Error
}
```

#### Pattern Matching

```csharp
var result = GetSomeResult();

// Using MatchSuccess/MatchError
if (result.MatchSuccess(out var value))
{
    Console.WriteLine($"Got: {value}");
}

if (result.MatchError(out var error))
{
    Console.WriteLine($"Error: {error.ErrorMessage}");
}
```

---

## Extension Methods

### Match

Collapses a `Result<T>` to a single value by providing handlers for both success and error cases.

```csharp
// Signature
TResult Match<T, TResult>(
    this Result<T> result,
    Func<T, TResult> valueMapper,
    Func<Error, TResult> errorMapper)
```

**Example:**

```csharp
var result = GetUser(userId);

// Convert to a display string
string display = result.Match(
    user => $"Welcome, {user.Name}!",
    error => $"Error: {error.ErrorMessage}"
);

// Convert to HTTP status code
int statusCode = result.Match(
    _ => 200,
    error => error.StatusCode
);
```

---

### Map

Transforms the success value. Does nothing if the result is an error. Similar to LINQ's `Select`.

```csharp
// Signature
Result<TResult> Map<T, TResult>(
    this Result<T> result,
    Func<T, TResult> mapper)
```

**Example:**

```csharp
Result<User> user = GetUser(userId);

// Transform the value
Result<string> email = user.Map(u => u.Email);
Result<int> nameLength = user.Map(u => u.Name.Length);

// Chain multiple maps
Result<string> greeting = user
    .Map(u => u.Name)
    .Map(name => name.ToUpper())
    .Map(name => $"Hello, {name}!");
```

---

### Bind

Chains operations that return `Result<T>`. Use this when your transformation can fail. Similar to LINQ's `SelectMany`.

```csharp
// Signature
Result<TResult> Bind<T, TResult>(
    this Result<T> result,
    Func<T, Result<TResult>> mapper)
```

**Example:**

```csharp
// Each step can fail
Result<Order> order = GetOrder(orderId)
    .Bind(o => ValidateOrder(o))      // Returns Result<Order>
    .Bind(o => ApplyDiscount(o))      // Returns Result<Order>
    .Bind(o => CalculateShipping(o)); // Returns Result<Order>

// If any step fails, subsequent steps are skipped
```

**When to use Map vs Bind:**

| Use `Map` when... | Use `Bind` when... |
|-------------------|-------------------|
| Transformation always succeeds | Transformation can fail |
| `Func<T, TResult>` | `Func<T, Result<TResult>>` |
| e.g., `user => user.Email` | e.g., `user => ValidateUser(user)` |

---

### Do

Executes a side effect on the success value without changing the result. Perfect for logging, auditing, or other operations that shouldn't affect the chain.

```csharp
// Signature
Result<T> Do<T>(this Result<T> result, Action<T> action)
```

**Example:**

```csharp
Result<Order> processedOrder = GetOrder(orderId)
    .Do(o => _logger.LogInformation("Processing order {Id}", o.Id))
    .Bind(o => ValidateOrder(o))
    .Do(o => _auditService.RecordValidation(o))
    .Bind(o => ProcessPayment(o))
    .Do(o => _metrics.IncrementOrdersProcessed());
```

---

### MapError

Transforms the error. Does nothing if the result is successful.

```csharp
// Signature
Result<T> MapError<T>(this Result<T> result, Func<Error, Error> errorMapper)
```

**Example:**

```csharp
// Add context to errors
Result<User> user = GetUser(userId)
    .MapError(e => Error.Create(
        e.StatusCode,
        e.ErrorIdentifier,
        e.ErrorMessage,
        $"While fetching user {userId}"
    ));

// Change error type
Result<Order> order = _externalService.GetOrder(orderId)
    .MapError(e => Error.Create(503, "ServiceUnavailable", "Order service is down"));
```

---

## Async Support

All extension methods have async overloads for seamless integration with async code.

### Async Method Signatures

```csharp
// Result<T> with async mapper
Task<Result<TResult>> Map<T, TResult>(this Result<T> result, Func<T, Task<TResult>> mapper);

// Task<Result<T>> with sync mapper
Task<Result<TResult>> Map<T, TResult>(this Task<Result<T>> result, Func<T, TResult> mapper);

// Task<Result<T>> with async mapper
Task<Result<TResult>> Map<T, TResult>(this Task<Result<T>> result, Func<T, Task<TResult>> mapper);

// Same pattern for Match, Bind, Do, MapError
```

### Example: Mixing Sync and Async

```csharp
// Start with sync, chain async operations
var result = await Result.New(userId)
    .Map(id => id.ToString())                    // sync
    .Bind(id => _userRepo.GetByIdAsync(id))      // async
    .Map(user => user.Email)                     // sync in async chain
    .Bind(email => _emailService.ValidateAsync(email))  // async
    .Map(email => email.ToLower());              // sync

// Handle result
return result.Match(
    email => Ok(email),
    error => BadRequest(error.ErrorMessage)
);
```

---

## LINQ Query Syntax

The library supports C# LINQ query syntax through `Select` and `SelectMany` methods.

### Synchronous Query Syntax

```csharp
Result<string> result =
    from user in GetUser(userId)
    from profile in GetProfile(user.ProfileId)
    from settings in GetSettings(profile.SettingsId)
    select $"{user.Name}: {settings.Theme}";

// Equivalent to:
Result<string> result = GetUser(userId)
    .Bind(user => GetProfile(user.ProfileId)
        .Bind(profile => GetSettings(profile.SettingsId)
            .Map(settings => $"{user.Name}: {settings.Theme}")));
```

### Async Query Syntax

```csharp
Result<Order> order = await
    from user in GetUserAsync(userId)
    from cart in GetCartAsync(user.CartId)       // async
    from validated in ValidateCart(cart)          // sync in async chain
    from order in CreateOrderAsync(validated)     // async
    select order;
```

---

## Working with Collections

### FlattenResults

Converts `IEnumerable<Result<T>>` to `Result<IEnumerable<T>>`. Fails if any result is an error.

```csharp
// Signature
Result<IEnumerable<T>> FlattenResults<T>(
    this IEnumerable<Result<T>> results,
    string errorIdentifier = "AggregatedError",
    string? errorMessage = null)
```

**Example:**

```csharp
var userIds = new[] { 1, 2, 3, 4, 5 };

// Get all users - fails if any lookup fails
Result<IEnumerable<User>> allUsers = userIds
    .Select(id => GetUser(id))
    .FlattenResults("UsersFetchFailed", "Could not fetch all users");

// Process only if all succeeded
allUsers.Match(
    users => ProcessBatch(users),
    error => HandleError(error)
);
```

### FlattenValues

Converts `IEnumerable<Result<T>>` to `Result<IEnumerable<T>>`. **Always succeeds** - errors become validation messages.

```csharp
// Signature
Result<IEnumerable<T>> FlattenValues<T>(this IEnumerable<Result<T>> results)
```

**Example:**

```csharp
var inputs = new[] { "1", "abc", "3", "def", "5" };

// Parse all - collect successes, record failures as warnings
Result<IEnumerable<int>> parsed = inputs
    .Select(s => int.TryParse(s, out var n)
        ? Result.New(n)
        : Error.Conflict("ParseError", $"Cannot parse '{s}'"))
    .FlattenValues();

// Always succeeds
parsed.MatchSuccess(out var values);  // [1, 3, 5]
var warnings = parsed.ValidationMessages;  // 2 warnings for "abc" and "def"
```

### FlattenManyResults

Like `FlattenResults`, but for `IEnumerable<Result<IEnumerable<T>>>`.

```csharp
var result = departments
    .Select(d => GetEmployeesForDepartment(d.Id))  // Each returns Result<IEnumerable<Employee>>
    .FlattenManyResults();  // Result<IEnumerable<Employee>> with all employees
```

---

## ASP.NET Core MVC Integration

The `Fluxys.PSM2.FunctionalExtensions.Mvc` package provides seamless integration with ASP.NET Core.

### ToActionResult

Converts a `Result<T>` to an `IActionResult`.

```csharp
[HttpGet("{id}")]
public async Task<IActionResult> GetUser(int id)
{
    var result = await _userService.GetByIdAsync(id);
    return result.ToActionResult();
}
```

**Behavior:**

| Result State | HTTP Response |
|--------------|---------------|
| Success with value | `200 OK` with JSON body |
| Success with null | `200 OK` (empty) |
| Error | Status from `Error.StatusCode`, JSON error body |

### ToResponseActionResult

Wraps the result in a `Response<T>` envelope that includes validation messages.

```csharp
[HttpPost]
public async Task<IActionResult> CreateUser(CreateUserRequest request)
{
    var result = await _userService.CreateAsync(request);
    return result.ToResponseActionResult();
}

// Response body:
// {
//   "value": { ... },
//   "errors": [],
//   "warnings": ["Password is weak"],
//   "infos": []
// }
```

### ToCommandResult

For command/mutation operations returning `CommandResult`.

```csharp
[HttpPost]
public async Task<IActionResult> ProcessOrder(int orderId)
{
    var result = await _orderService.ProcessAsync(orderId);
    return result.ToCommandResult();
}
```

### Controller Extensions

Wrap entire actions with automatic error handling.

```csharp
[HttpGet("{id}")]
public Task<IActionResult> GetUser(int id)
{
    return this.GetResult(async () => await _userService.GetByIdAsync(id));
}

[HttpGet("{id}")]
public Task<IActionResult> GetUserWithResponse(int id)
{
    return this.GetResponseResult(async () => await _userService.GetByIdAsync(id));
}
```

---

## REST/HttpClient Integration

The `Fluxys.PSM2.FunctionalExtensions.Rest` package helps consume REST APIs that return `Response<T>`.

### ResponseToResult

Converts an `HttpResponseMessage` to a `Result<T>`.

```csharp
public async Task<Result<User>> GetUserAsync(int id, CancellationToken ct)
{
    var response = await _httpClient.GetAsync($"/users/{id}", ct);
    
    return await response.ResponseToResult<UserDto, User>(
        prefix: "UserService",
        mapper: dto => dto.ToDomain(),
        cancellationToken: ct
    );
}
```

### CommandResultToResult

For endpoints returning `CommandResult`.

```csharp
public async Task<Result<OrderResult>> CreateOrderAsync(OrderRequest request, CancellationToken ct)
{
    var response = await _httpClient.PostAsJsonAsync("/orders", request, ct);
    
    return await response.CommandResultToResult<OrderCommandResult, OrderResult>(
        prefix: "OrderService",
        mapper: r => new OrderResult(r.OrderId),
        cancellationToken: ct
    );
}
```

---

## Common Patterns

### Pattern 1: Validation Pipeline

```csharp
public Result<User> ValidateAndCreate(CreateUserRequest request)
{
    return ValidateEmail(request.Email)
        .Bind(_ => ValidatePassword(request.Password))
        .Bind(_ => ValidateUsername(request.Username))
        .Map(_ => new User(request.Email, request.Username, request.Password));
}

private Result<string> ValidateEmail(string email)
{
    if (string.IsNullOrEmpty(email))
        return Error.Conflict("EmailRequired", "Email is required");
    if (!email.Contains("@"))
        return Error.Conflict("EmailInvalid", "Email format is invalid");
    return email;
}
```

### Pattern 2: Repository Operations

```csharp
public async Task<Result<Order>> GetOrderWithDetailsAsync(int orderId)
{
    return await _orderRepo.GetByIdAsync(orderId)
        .Bind(order => _customerRepo.GetByIdAsync(order.CustomerId)
            .Map(customer => order with { Customer = customer }))
        .Bind(order => _orderLineRepo.GetByOrderIdAsync(orderId)
            .Map(lines => order with { Lines = lines.ToList() }));
}
```

### Pattern 3: Conditional Operations

```csharp
public async Task<Result<Order>> ApplyDiscountIfEligible(Order order)
{
    if (!order.Customer.IsPreferred)
        return order;  // No discount, return as-is

    return await _discountService.CalculateDiscountAsync(order)
        .Map(discount => order with { Discount = discount });
}
```

### Pattern 4: Logging and Auditing

```csharp
public async Task<Result<Payment>> ProcessPaymentAsync(PaymentRequest request)
{
    return await ValidatePayment(request)
        .Do(p => _logger.LogInformation("Processing payment {Id}", p.Id))
        .Bind(p => _gateway.ChargeAsync(p))
        .Do(p => _auditLog.RecordPayment(p))
        .Do(p => _metrics.RecordPaymentProcessed(p.Amount))
        .MapError(e =>
        {
            _logger.LogError("Payment failed: {Error}", e.ErrorMessage);
            return e;
        });
}
```

### Pattern 5: Partial Success with FlattenValues

```csharp
public async Task<Result<ImportSummary>> ImportUsersAsync(IEnumerable<UserDto> users)
{
    var results = await Task.WhenAll(users.Select(u => ImportSingleUserAsync(u)));
    
    return results
        .FlattenValues()  // Collect successes, errors become warnings
        .Map(imported => new ImportSummary
        {
            Imported = imported.Count(),
            Total = users.Count()
        });
}
```

---

## API Reference

### Core Types

| Type | Description |
|------|-------------|
| `Error` | Immutable error with status code, identifier, message, and optional context |
| `Result<T>` | Readonly struct holding success value or error, plus validation messages |
| `ValidationMessage` | Warning/info message that accumulates through the chain |

### Error Factory Methods

| Method | Status Code | Description |
|--------|-------------|-------------|
| `Error.Create(code, id, msg)` | Custom | Generic error creation |
| `Error.NotFound(id, msg)` | 404 | Resource not found |
| `Error.Conflict(id, msg)` | 409 | Business rule violation |
| `Error.FromException(ex)` | 500 | Convert exception to error |

### Result Extension Methods

| Method | Input | Output | Description |
|--------|-------|--------|-------------|
| `Match` | `Result<T>`, two funcs | `TResult` | Handle both cases |
| `Map` | `Result<T>`, `Func<T, R>` | `Result<R>` | Transform success value |
| `Bind` | `Result<T>`, `Func<T, Result<R>>` | `Result<R>` | Chain fallible operations |
| `Do` | `Result<T>`, `Action<T>` | `Result<T>` | Execute side effect |
| `MapError` | `Result<T>`, `Func<Error, Error>` | `Result<T>` | Transform error |

### Collection Extensions

| Method | Description |
|--------|-------------|
| `FlattenResults` | All must succeed, first error wins |
| `FlattenManyResults` | Same as above for nested enumerables |
| `FlattenValues` | Always succeeds, errors become validation messages |

### MVC Extensions

| Method | Description |
|--------|-------------|
| `ToActionResult` | Convert to `IActionResult` |
| `ToResponseActionResult` | Convert to `IActionResult` with `Response<T>` envelope |
| `ToCommandResult` | Convert to `IActionResult` with `CommandResult` |
| `GetResult` | Controller extension for wrapping actions |
| `GetResponseResult` | Controller extension with `Response<T>` |

### REST Extensions

| Method | Description |
|--------|-------------|
| `ResponseToResult` | Convert `HttpResponseMessage` to `Result<T>` |
| `CommandResultToResult` | Convert `HttpResponseMessage` with `CommandResult` to `Result<T>` |

---

## Best Practices

1. **Return `Result<T>`, don't throw** - Use exceptions only for truly exceptional conditions (I/O failures, serialization errors). Use `Result<T>` for expected business/validation failures.

2. **Keep functions pure** - Avoid side effects inside `Map` and `Bind`. Use `Do` for logging, auditing, and other side effects.

3. **Use implicit conversions** - Write `return value;` instead of `return Result.New(value);` for cleaner code.

4. **Preserve error context** - Use `MapError` to add context to errors as they propagate up the call stack.

5. **Use `FlattenValues` for partial success** - When processing batches where some failures are acceptable.

6. **Use LINQ query syntax for complex chains** - It's more readable when you need values from multiple steps.

7. **Log at appropriate levels** - Use `Do` with logging at service boundaries, not inside every method.
