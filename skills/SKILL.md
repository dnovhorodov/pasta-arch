## name: dotnet-pasta-functional-core
description: Universal architectural standard for modern .NET 10+ services. Enforces PASTA Architecture (Ports, Adapters, Slices, Typed Abstractions, Anti-Layers), Functional Core Imperative Shell (FCIS), FunqTypes for Result/Option monadic flows, Func/Action port records, strongly-typed IDs, and zero framework indirection.

# Universal C# Skill: PASTA Architecture & Pure Functional Core (.NET 10+)

This skill defines the universal architectural standard and code conventions for modern .NET applications (targeting .NET 10+ and C# 13/14+). It enforces **PASTA Architecture** and the **Functional Core, Imperative Shell (FCIS)** pattern.

> **The PASTA Axiom:**
> **P**orts 🔹 **A**dapters 🔹 **S**lices 🔹 **T**yped Abstractions 🔹 (**A**nti-Layers)
>
> A minimalist, modern architecture for building modular, testable, and maintainable systems in C#:
> * **No interfaces.**
> * **No anemic layers.**
> * **No framework-driven indirection.**
>
> Just functional slices of real business logic, with typed ports and clean boundaries.

---

## 1. The PASTA Breakdown

```
 ┌────────────────────────────────────────────────────────────────────────┐
 │                      S - SLICES (Vertical Slice)                       │
 │                                                                        │
 │   ┌──────────────────────────────────────────────────────────────┐     │
 │   │               A - (ANTI-LAYERS) EXECUTION FLOW               │     │
 │   │  Trigger / Endpoint ──► Slice Action (Imperative Shell)      │     │
 │   └──────────────────────────────┬───────────────────────────────┘     │
 │                                  │                                     │
 │          ┌───────────────────────┴───────────────────────┐             │
 │          ▼                                               ▼             │
 │  ┌──────────────────────────────┐        ┌──────────────────────────┐  │
 │  │      PURE DOMAIN CORE        │        │        P - PORTS         │  │
 │  │  T - Typed Abstractions      │        │  Func<> / Action<>       │  │
 │  │      (Records, FunqTypes)    │        │  Granular Port Records   │  │
 │  │  Pure Invariants & Logic     │        └─────────────┬────────────┘  │
 │  └──────────────────────────────┘                      │               │
 └────────────────────────────────────────────────────────┼───────────────┘
                                                          ▼
                                            ┌──────────────────────────┐
                                            │       A - ADAPTERS       │
                                            │  Infrastructure Wiring   │
                                            │  (SQL, HTTP, Bus, Cache) │
                                            └──────────────────────────┘
```

* **P — Ports:** Outbound and inbound dependency boundaries modeled strictly as records of standard BCL `Func<>` and `Action<>` delegates. No multi-method service interfaces, no generic repositories.
* **A — Adapters:** Infrastructure bindings that implement ports (SQL queries via Dapper/ADO.NET, HTTP integrations, message brokers, clock/crypto providers). Adapters expose functions matching port delegate signatures.
* **S — Slices:** Vertical, feature-driven organization. Code is grouped strictly by business capability and intent—never by horizontal technical concerns (`Controllers/`, `Services/`, `Repositories/`).
* **T — Typed Abstractions:** Deep type safety eliminating primitive obsession. Strongly-typed identifiers, immutable domain records, and algebraic types (`Result<T, E>`, `Option<T>`) powered by **`FunqTypes`**.
* **A — Anti-Layers:** The systematic elimination of ceremony-heavy anemic layers (e.g., Controller $\to$ Service $\to$ Manager $\to$ Repository $\to$ DAO). Slices interact directly through their ports and pure functions without artificial indirection.

---

## 2. Functional Core, Imperative Shell (FCIS)

Every business operation is partitioned into two distinct zones:

### Pure Functional Core
* Contains domain policies, calculation logic, invariants, and state transitions ($S_{\text{initial}} \to S_{\text{next}}$).
* **Guarantees:** Deterministic, synchronous, zero side-effects, zero I/O (`HttpClient`, database connections, or file systems are strictly forbidden).
* Evaluates input, applies domain logic, and returns either updated state or domain errors wrapped in `FunqTypes.Result<T, E>`.

### Imperative Shell (Slice Action)
* Coordinates the runtime environment: captures inputs, reads external state via **Ports**, passes state into the **Pure Core**, and commits state changes back through **Ports**.
* Organizes execution via Railway-Oriented Programming (ROP) pipelines using monadic composition operators (`Bind`, `BindAsync`, `Map`, `MapAsync`).
* Handles cancellation tokens, logging, transactions, and transport responses (e.g., HTTP status mapping).

---

## 3. Ports via Standard `Func<>` and `Action<>` Records

### Prohibitions:
* **NO** `IRepository<T>`, `IUnitOfWork`, or generic CRUD interfaces.
* **NO** `IService` / `IManager` interfaces.
* **NO** custom `delegate` declarations where standard BCL `Func<>` or `Action<>` suffice.

### Standard Pattern: Granular Port Records

Dependencies required by a slice are declared inside a sealed `record` containing standard `Func<...>` and `Action<...>` properties returning `ValueTask`:

```csharp
namespace App.Features.Common;

using FunqTypes;

// Generic Port Record Blueprint
public sealed record FeaturePorts<TId, TEntity, TError>(
    Func<TId, CancellationToken, ValueTask<Option<TEntity>>> LoadById,
    Func<TEntity, CancellationToken, ValueTask<Result<Unit, TError>>> Save
);
```

#### Why Standard `Func<>` / `Action<>` Records?
1. **Zero Mocking Overhead:** Unit tests inject simple lambdas (`(id, ct) => ValueTask.FromResult(Option<TEntity>.Some(testEntity))`) with zero mocking libraries (`Moq`, `NSubstitute`) or dynamic proxies.
2. **Universal Ecosystem Adoption:** Standard BCL generic delegates compose natively with middleware, logging wrappers, Polly retry policies, and caching decorators without requiring custom delegate adaptors.
3. **Explicit Dependency Surface:** Slices state the exact operations they need—nothing more.

---

## 4. Railway-Oriented Flow with `FunqTypes`

Do not reinvent `Result` or `Option` types. All monadic error handling and optionality must rely on the **`FunqTypes`** library.

### Installation

Add the official package reference to your `.csproj`:

```xml
<PackageReference Include="FunqTypes" Version="1.4.1" />
```

Or install via the .NET CLI:

```bash
dotnet add package FunqTypes
```

### Zero Unchecked Domain Exceptions
Exceptions are strictly reserved for fatal, unrecoverable system crashes (out of memory, process corruption). All expected branch outcomes (not found, unauthorized, conflict, validation failures) are treated as domain values via `Result<TValue, TError>`.

### Closed Error Hierarchy
Model domain errors using closed record hierarchies:

```csharp
namespace App.Domain.Errors;

public abstract record DomainError
{
    public sealed record NotFound(string Resource, string Key) : DomainError;
    public sealed record Conflict(string Message) : DomainError;
    public sealed record Validation(string Field, string Message) : DomainError;
    public sealed record Unauthorized(string Reason) : DomainError;
    public sealed record InfrastructureFailure(string Subsystem, string Message) : DomainError;
}
```

### Pipeline Composition Operators

`FunqTypes` provides first-class monadic pipeline operators:

* `.Bind(func)` / `.BindAsync(func)`: Chains operations where the next step produces another `Result<U, E>`, short-circuiting on the first error.
* `.Map(func)` / `.MapAsync(func)`: Transforms the inner success value from $T \to U$ while preserving the railway track.
* `Result.Combine(r1, r2, ...)` / `r1.Combine(r2)`: Combines up to 8 independent `Result` instances into a tuple `Result<(T1, T2, ...), E>`, accumulating *all* errors if multiple steps fail.
* `.Ensure(predicate, error)` / `.EnsureAsync(...)`: Validates a condition, failing if the predicate evaluates to false.
* `.Tap(action)` / `.TapAsync(...)`: Executes side effects (e.g., logging or metrics) without altering the contained value.
* `.Match(onSuccess, onFailure)`: Pattern matches at the boundary (e.g., endpoint) to project results into HTTP responses.

---

## 5. Generic Slice Blueprint (Canonical Structure)

Every feature slice lives in a single folder containing the complete vertical workflow:

```
Features/<FeatureName>/
├── <Feature>Models.cs       # [T] Typed Abstractions: Domain records & values
├── <Feature>Transitions.cs  # [T] Pure Core: Validation, invariant rules, state transforms
├── <Feature>Ports.cs        # [P] Ports: Record of Func<> / Action<> contracts
├── <Feature>Action.cs       # [A] (Anti-Layers) Action: ROP Imperative shell orchestrator
└── <Feature>Endpoint.cs     # [S] Slice entry point (Minimal API / Message Handler)
```

### Complete Generic Implementation Pattern

#### 1. Models & Typed Abstractions (`FeatureModels.cs`)
```csharp
namespace App.Features.Orders;

using App.Domain.Identifiers;

public readonly record struct OrderId(Guid Value);

public sealed record Order(
    OrderId Id,
    string ReferenceNumber,
    decimal TotalAmount,
    OrderStatus Status,
    DateTimeOffset CreatedAtUtc);

public enum OrderStatus { Draft, Confirmed, Cancelled }
```

#### 2. Pure Core Transitions (`FeatureTransitions.cs`)
```csharp
namespace App.Features.Orders;

using FunqTypes;
using App.Domain.Errors;

public static class OrderTransitions
{
    // Pure function: deterministic, synchronous, zero I/O, accumulates all rule violations via Result.Combine
    public static Result<Order, DomainError> Confirm(Order order, DateTimeOffset now)
    {
        var statusCheck = order.Status is OrderStatus.Draft
            ? Result<Unit, DomainError>.Ok(Unit.Value)
            : Result<Unit, DomainError>.Fail(
                new DomainError.Conflict($"Cannot confirm order in status '{order.Status}'."));

        var amountCheck = order.TotalAmount > 0m
            ? Result<Unit, DomainError>.Ok(Unit.Value)
            : Result<Unit, DomainError>.Fail(
                new DomainError.Validation(nameof(order.TotalAmount), "Amount must be greater than zero."));

        return Result.Combine(statusCheck, amountCheck)
            .Map((_, _) => order with { Status = OrderStatus.Confirmed });
    }
}
```

#### 3. Ports Record (`FeaturePorts.cs`)
```csharp
namespace App.Features.Orders;

using FunqTypes;
using App.Domain.Errors;

public sealed record ConfirmOrderPorts(
    Func<OrderId, CancellationToken, ValueTask<Option<Order>>> FetchOrder,
    Func<Order, CancellationToken, ValueTask<Result<Unit, DomainError>>> PersistOrder,
    Func<Order, CancellationToken, ValueTask<Result<Unit, DomainError>>> PublishConfirmedEvent
);
```

#### 4. Action / Orchestrator (`FeatureAction.cs`)

The action implements a linear **Railway-Oriented Pipeline**. No manual procedural checking or nested branching—execution transitions smoothly from retrieval to pure logic to persistence via monadic operators:

```csharp
namespace App.Features.Orders;

using FunqTypes;
using App.Domain.Errors;

public record ConfirmOrderCommand(OrderId Id);
public record ConfirmOrderResponse(OrderId Id, OrderStatus Status);

public static class ConfirmOrderAction
{
    public static async Task<Result<ConfirmOrderResponse, DomainError>> ExecuteAsync(
        ConfirmOrderCommand command,
        ConfirmOrderPorts ports,
        TimeProvider timeProvider,
        CancellationToken ct)
    {
        // 1. Enter the railway track: retrieve state via Port and project Option -> Result via ToResult
        var initialResult = (await ports.FetchOrder(command.Id, ct))
            .ToResult<DomainError>(
                new DomainError.NotFound(nameof(Order), command.Id.Value.ToString()));

        // 2. Monadic ROP Pipeline: Pure Core -> Shell Persist -> Shell Publish -> Projection
        var confirmedResult = await initialResult
            // Pure Core Transition: Synchronous invariant verification & state change
            .Bind(order => OrderTransitions.Confirm(order, timeProvider.GetUtcNow()))
            // Imperative Shell: Persist state via Port (retaining the updated entity on success)
            .BindAsync(async order => 
                (await ports.PersistOrder(order, ct)).Map(_ => order))
            // Imperative Shell: Publish outbound domain event via Port
            .BindAsync(async order => 
                (await ports.PublishConfirmedEvent(order, ct)).Map(_ => order));

        // 3. Final Projection: Map domain entity to response model
        return confirmedResult.Map(order => new ConfirmOrderResponse(order.Id, order.Status));
    }
}
```

#### 5. Minimal API Slice Endpoint (`FeatureEndpoint.cs` — C# 14 Extension Block)
```csharp
namespace App.Features.Orders;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

public static class ConfirmOrderEndpoint
{
    extension(IEndpointRouteBuilder app)
    {
        public IEndpointRouteBuilder MapConfirmOrder()
        {
            app.MapPost("/orders/{id:guid}/confirm", async (
                Guid id,
                ConfirmOrderPorts ports,
                TimeProvider timeProvider,
                CancellationToken ct) =>
            {
                var result = await ConfirmOrderAction.ExecuteAsync(
                    new ConfirmOrderCommand(new OrderId(id)),
                    ports,
                    timeProvider,
                    ct);

                return result.Match(
                    success => Results.Ok(success),
                    errors => errors[0] switch
                    {
                        DomainError.NotFound nf => Results.NotFound(nf),
                        DomainError.Validation val => Results.BadRequest(val),
                        DomainError.Conflict conf => Results.Conflict(conf),
                        _ => Results.StatusCode(StatusCodes.Status500InternalServerError)
                    });
            });

            return app;
        }
    }
}
```

---

## 6. Typed Abstractions: Strongly-Typed Identifiers

Avoid primitive obsession (`string`, `Guid`, `long` used directly as domain identity). All identifiers must be zero-allocation `readonly record struct` instances equipped with JSON converters.

```csharp
namespace App.Domain.Identifiers;

using System.Text.Json;
using System.Text.Json.Serialization;

public interface IStronglyTypedId<TSelf, TPrimitive>
    where TSelf : struct, IStronglyTypedId<TSelf, TPrimitive>
    where TPrimitive : notnull
{
    TPrimitive Value { get; }
    static abstract TSelf From(TPrimitive value);
}

public sealed class StronglyTypedIdJsonConverter<TSelf, TPrimitive> : JsonConverter<TSelf>
    where TSelf : struct, IStronglyTypedId<TSelf, TPrimitive>
    where TPrimitive : notnull
{
    public override TSelf Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.TokenType switch
        {
            JsonTokenType.String when typeof(TPrimitive) == typeof(Guid) && reader.TryGetGuid(out var g) =>
                TSelf.From((TPrimitive)(object)g),
            JsonTokenType.String when typeof(TPrimitive) == typeof(string) && reader.GetString() is { } s =>
                TSelf.From((TPrimitive)(object)s),
            JsonTokenType.Number when typeof(TPrimitive) == typeof(long) && reader.TryGetInt64(out var l) =>
                TSelf.From((TPrimitive)(object)l),
            _ => throw new JsonException($"Cannot parse {typeof(TSelf).Name} from {reader.TokenType}.")
        };

    public override void Write(Utf8JsonWriter writer, TSelf value, JsonSerializerOptions options)
    {
        if (value.Value is Guid g) writer.WriteStringValue(g);
        else if (value.Value is string s) writer.WriteStringValue(s);
        else writer.WriteNumberValue((long)(object)value.Value);
    }
}
```

---

## 7. Adapters & Composition (Connecting Ports to Reality)

Adapters bind database engines, network clients, and message buses to port delegates. Because ports are simple delegate records, adapters can be written as lightweight factory functions or static methods.

```csharp
namespace App.Infrastructure.Persistence;

using System.Data.Common;
using FunqTypes;
using App.Domain.Errors;
using App.Features.Orders;

public static class OrderSqlAdapters
{
    public static ConfirmOrderPorts CreateConfirmOrderPorts(
        DbDataSource dataSource,
        Func<Order, CancellationToken, ValueTask<Result<Unit, DomainError>>> eventPublisher)
    {
        return new ConfirmOrderPorts(
            FetchOrder: async (orderId, ct) =>
            {
                await using var connection = await dataSource.OpenConnectionAsync(ct);
                // Execute optimized query (Dapper / ADO.NET)
                Order? order = null; /* Query mapping here */
                return order is not null ? Option<Order>.Some(order) : Option<Order>.None();
            },
            PersistOrder: async (order, ct) =>
            {
                await using var connection = await dataSource.OpenConnectionAsync(ct);
                // Execute update query
                return Result<Unit, DomainError>.Ok(Unit.Value);
            },
            PublishConfirmedEvent: eventPublisher
        );
    }
}
```

### 3-Tier Testing Strategy (Unit, Component, E2E)

1. **Unit Tests (In-Memory, Zero I/O, Zero Mocks):**
   - Test Pure Core policies/transitions synchronously (including multi-error accumulation via `Result.Combine`).
   - Test Slice Actions/Handlers by passing inline lambda `Func<>` stubs into the Port Record—never use `Moq` or `NSubstitute`.
2. **Component Tests (In-Process HTTP via `WebApplicationFactory<Program>`):**
   - Test the full slice from HTTP endpoint through input validation, handler, domain core, and ports using a fast in-memory adapter.
3. **E2E / Integration Tests (`Testcontainers` + Real Infrastructure Adapter):**
   - Spin up disposable infrastructure containers (e.g., `Testcontainers.PostgreSql`), inject the connection string into `WebApplicationFactory<Program>`, and verify the real SQL/adapter wiring end-to-end with zero changes to slice code.

```csharp
[Fact]
public async Task ConfirmOrder_WhenValid_ReturnsSuccess()
{
    var testOrder = new Order(new OrderId(Guid.NewGuid()), "ORD-1", 100m, OrderStatus.Draft, DateTimeOffset.UtcNow);
    var saved = false;

    var ports = new ConfirmOrderPorts(
        FetchOrder: (id, ct) => ValueTask.FromResult(Option<Order>.Some(testOrder)),
        PersistOrder: (order, ct) => { saved = true; return ValueTask.FromResult(Result<Unit, DomainError>.Ok(Unit.Value)); },
        PublishConfirmedEvent: (order, ct) => ValueTask.FromResult(Result<Unit, DomainError>.Ok(Unit.Value))
    );

    var result = await ConfirmOrderAction.ExecuteAsync(
        new ConfirmOrderCommand(testOrder.Id),
        ports,
        TimeProvider.System,
        CancellationToken.None);

    Assert.True(result.IsSuccess);
    Assert.True(saved);
}
```

---

## 8. High-Performance .NET 10+ Coding Standards

1. **Delegate Port Returns:** Always use `ValueTask` or `ValueTask<T>` for port signatures to allow zero-allocation synchronous returns (e.g., in-memory cache hits, stubbed tests).
2. **Nullable Reference Types:** `#nullable enable` must be active across all files. No compiler warnings allowed.
3. **Cancellation Hygiene:** Propagate `CancellationToken` through every asynchronous `Func<>` and shell invocation.
4. **No Sync-Over-Async:** Calling `.Result`, `.Wait()`, or `.GetAwaiter().GetResult()` is strictly prohibited.
5. **Memory Discipline:** Use `ReadOnlySpan<T>` and `stackalloc` where applicable for ephemeral parsing, hashing, and token operations.

---

## 9. Reviewer & Quality Checklist

Before committing code, verify against these non-negotiable rules:

* [ ] **PASTA Structure Honored:** The slice contains **P**orts (Func/Action records), **A**dapters (infrastructure wiring), **S**lices (feature cohesion), **T**yped Abstractions (FunqTypes, strongly-typed IDs), and **A**nti-Layers (no artificial service/repository indirection).
* [ ] **Package Reference Configured:** `FunqTypes` is referenced via NuGet (`<PackageReference Include="FunqTypes" Version="1.4.1" />`).
* [ ] **Railway Pipeline Adopted:** Actions compose workflows using `.Bind()`, `.BindAsync()`, `.Map()`, and `.MapAsync()` rather than procedural early returns.
* [ ] **Standard Delegate Contracts:** Port boundaries are modeled via standard BCL `Func<...>` and `Action<...>` records—never custom named delegates.
* [ ] **Zero Interfaces for CRUD / Services:** No `IRepository`, `IUnitOfWork`, or `IFooService`.
* [ ] **Zero Exceptions for Domain Flow:** Predictable domain branches return `Result<T, DomainError>`.
* [ ] **Pure Core Isolation:** Transition functions are synchronous, deterministic, and free of I/O or external packages.