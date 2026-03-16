# FluentSolidity.FunctionalExtensions

Tiny pragmatic functional helpers for C#. Provides lightweight `Result<T>` and `Either<TLeft, TRight>` discriminated unions plus mapping/binding/async/query-syntax helpers and collection flattening utilities.

## Core Types

### `Result<T>`
Immutable `readonly struct` representing success (`T`) or error (`Error`), with an optional side-channel of `PipelineMessage[]` that travels through the computation pipeline. Implicit conversions from `T` and `Error` for ergonomics.

### `Either<TLeft, TRight>`
General-purpose `readonly struct` discriminated union. Left is conventionally the "alternative/error" track; Right is the "happy" track. Useful when the error side is not necessarily an `Error` record.

### `Error`
A C# `record` with an identifier, message, and optional context. Factory methods: `Error.Create(identifier, message)` and `Error.Create(identifier, message, context)`. Also provides `ToPipelineMessage()` to convert to a `PipelineMessage`.

### `PipelineMessage`
A `record(string Id, MessageLevel Level, string Message, string? Context)` that travels alongside a `Result<T>` through a pipeline. Used for non-fatal warnings, info messages, and aggregated error details.

### `MessageLevel`
Enum: `Info`, `Warning`, `Error`.

## Result Key Operations

| Method | Description |
|--------|-------------|
| `Map` / `Bind` | Transform and sequence; error short-circuits the chain. |
| `Match` | Collapse into a single value (sync/async). |
| `Do` | Side-effect on success only; returns original result. |
| `DoError` | Side-effect on error only; returns original result. |
| `DoWhen` | Conditional side-effect: executes only when success AND predicate returns true. |
| `MapError` | Transform the error while leaving success untouched. |
| `Ensure` | Guard: flips success to error if a predicate fails; short-circuits on existing error. |
| `Tap` | Inspect any value mid-chain without altering it (generic, works on any `T`). |
| `Select` / `SelectMany` | LINQ query comprehension support (sync & async mixes). |
| `FlattenResults` | Fails if any error; aggregates pipeline messages, de-duplicates. |
| `FlattenManyResults` | Like `FlattenResults` but for `IEnumerable<Result<IEnumerable<T>>>`; flattens nested enumerables. |
| `FlattenValues` | Always succeeds; errors become pipeline messages. |

All combinators (`Map`, `Bind`, `Do`, `DoError`, `DoWhen`, `Match`, `Ensure`) provide **4 overloads** covering sync/async source and sync/async lambda combinations.

## Either Key Operations

| Method | Description |
|--------|-------------|
| `Map` / `Bind` | Transform the Right value; Left short-circuits. |
| `MapLeft` / `BindLeft` | Transform the Left value; Right short-circuits. |
| `Match` | Collapse into a single value. |
| `Do` | Side-effect on Right only. |
| `DoLeft` | Side-effect on Left only. |
| `Ensure` | Guard: flips Right to Left if predicate fails. |
| `Select` / `SelectMany` | LINQ query comprehension support. |

## Rule of Thumb: Map vs Bind

| Use | When the lambda returns | Example signature |
|-----|-------------------------|-------------------|
| `.Map()` | A plain value `T` | `Func<T, TResult>` or `Func<T, Task<TResult>>` |
| `.Bind()` | A `Result<T>` or `Task<Result<T>>` | `Func<T, Result<TResult>>` or `Func<T, Task<Result<TResult>>>` |

**Map** transforms the success value; the library wraps the output in a new `Result<T>`.  
**Bind** is for functions that already return `Result<T>` (e.g., validation, lookups that can fail).

This rule applies to other methods too:

| Method | Lambda returns plain value | Lambda returns Result |
|--------|---------------------------|----------------------|
| Transform success | `Map` | `Bind` |
| Transform error | `MapError` | -- |
| Side-effect only | `Do` / `DoError` / `DoWhen` | -- |
| Guard / validate | -- | `Ensure` |

## Usage Examples

### Basic Map (lambda returns plain value)
```csharp
var result = Result.New(21)
    .Map(x => x * 2);  // 42 -- lambda returns int, wrapped in Result<int>
```

### Basic Bind (lambda returns Result)
```csharp
Result<int> Parse(string s) =>
    int.TryParse(s, out var v) ? Result.New(v) : Error.Create("Parse", "Invalid");

var result = Result.New("42")
    .Bind(s => Parse(s));  // Parse returns Result<int>, so use Bind
```

### Chaining Map and Bind
```csharp
var final = Result.New(10)
    .Map(x => x * 2)                    // 20 -- plain value
    .Bind(x => Validate(x))             // Validate returns Result<int>
    .Map(x => new Response { Value = x }); // plain DTO
```

### Real-world pipeline
```csharp
var response = await pricingParameterFactory
    .Bind(f => f.Create(request))           // Create returns Result<Param>
    .Map(p => repository.Save(p, ct))       // Save returns Task<Param> (plain)
    .Map(p => new CreateResponse { Id = p.Id });  // plain DTO

return response.Match(
    ok => Results.Ok(ok),
    err => Results.Problem(err.ErrorMessage));
```

### Short-circuiting on error
```csharp
var result = Result.New(10)
    .Bind(_ => Result.Error<int>(Error.Create("E1", "Boom")))  // becomes error
    .Map(v => v * 10)   // never executed
    .Match(v => v.ToString(), e => e.ErrorIdentifier);  // "E1"
```

### Side-effects with Do
```csharp
var result = await GetUserAsync(id)
    .Do(user => logger.LogInformation("Found {Name}", user.Name))  // side-effect on success
    .Map(user => new UserDto(user));
```

### Side-effects on error with DoError
```csharp
var result = await GetUserAsync(id)
    .DoError(err => logger.LogWarning("Lookup failed: {Msg}", err.ErrorMessage))
    .Map(user => new UserDto(user));
```

### Conditional side-effects with DoWhen
```csharp
var result = await GetUserAsync(id)
    .DoWhen(
        user => user.IsAdmin,
        user => logger.LogInformation("Admin access: {Name}", user.Name))
    .Map(user => new UserDto(user));
```

### Guard validation with Ensure
```csharp
var result = Result.New(age)
    .Ensure(
        a => a >= 18,
        a => Error.Create("TooYoung", $"Age {a} is below minimum"));
// If age < 18, result becomes an error; otherwise passes through unchanged
```

### Chaining Ensure with Map and Bind
```csharp
var result = Result.New(order)
    .Ensure(o => o.Items.Any(), _ => Error.Create("Empty", "Order has no items"))
    .Ensure(o => o.Total > 0, _ => Error.Create("InvalidTotal", "Total must be positive"))
    .Bind(o => ApplyDiscount(o))
    .Map(o => new OrderConfirmation(o));
```

### Flattening collections
```csharp
var results = new[] { Result.New(1), Result.Error<int>(Error.Create("E", "bad")), Result.New(2) };

// FlattenValues: always succeeds; errors become pipeline messages
var flat = results.FlattenValues();  // Success([1,2]) with 1 pipeline message

// FlattenResults: fails if any error
var flat2 = results.FlattenResults(); // Error (first error wins)
```

### Query syntax (LINQ comprehension)
```csharp
var result =
    from a in Result.New(10)
    from b in Validate(a)           // returns Result<int>
    from c in ComputeAsync(b)       // async works too
    select a + b + c;
```

### Tap (generic, works on any type)
```csharp
var user = await GetUserAsync(id)
    .Tap(u => logger.LogDebug("Loaded user: {Id}", u.Id));  // inspect without altering

// Async overloads also available:
var result = someValue
    .Tap(async v => await auditService.LogAsync(v));
```

### Either usage
```csharp
// Construction
var right = Either.Right<string, int>(42);
var left  = Either.Left<string, int>("not found");

// Chaining
var result = Either.Right<string, int>(10)
    .Map(x => x * 2)                          // Right(20)
    .Ensure(x => x < 100, x => $"Too big: {x}")  // guard
    .Bind(x => Lookup(x))                     // Lookup returns Either<string, int>
    .Match(
        left:  msg => $"Error: {msg}",
        right: val => $"Got: {val}");

// MapLeft / BindLeft -- transform the error track
var enriched = result
    .MapLeft(err => $"[ERR] {err}");

// DoLeft -- side-effect on Left
var logged = result
    .DoLeft(err => logger.LogWarning("Left: {Err}", err));

// Query syntax works too
var composed =
    from a in Either.Right<string, int>(10)
    from b in Either.Right<string, int>(20)
    select a + b;  // Right(30)
```

### Pipeline messages
```csharp
// Attach messages to a result
var info = new PipelineMessage("INF01", MessageLevel.Info, "Processing started");
var result = Result.New(42).WithMessages(info);

// Messages propagate through Map, Bind, and Flatten chains
var final = result
    .Map(x => x * 2)
    .Bind(x => Validate(x));
// final.Messages contains messages from all steps
```

## Build & Test
```bash
dotnet restore
dotnet build -c Debug
dotnet test -c Debug
```

Tests are organized per combinator in the `Core.Tests` project (e.g., `ResultMapTests.cs`, `ResultEnsureTests.cs`, `EitherDoLeftTests.cs`, etc.).

## Design Notes

- No exceptions for expected validation; use `Error`.
- Both `Result<T>` and `Either<TLeft, TRight>` are `readonly struct` types to avoid heap allocation.
- Explicit `bool` tag discrimination (not null-checks or default-value comparisons).
- Async APIs mirror sync to prevent accidental blocking -- every combinator has 4 overloads (sync/async source x sync/async lambda).
- Pipeline messages use `IReadOnlyList<PipelineMessage>` with pre-allocated capacity for efficient O(N) propagation.
- Compile-time safety: `[Obsolete(error: true)]` overload prevents accidentally passing `Func<T, Task>` (void-returning async) to `Map`.
- Helpers aim to stay minimal -- no complex monads beyond `Result<T>` and `Either<TLeft, TRight>`.
