![PASTA logo](./assets/pasta_logo_small.png)

> **PASTA** — _Ports🔹Adapters🔹Slices🔹Typed Abstractions🔹(Anti-Layers)_

A minimalist, modern architecture for building modular, testable, and maintainable systems in C#, with:
- No interfaces.
- No anemic layers.
- No framework-driven indirection.

Just functional slices of real business logic, with typed ports and clean boundaries.

---

## 📈 Architecture Diagram

![PASTA Architecture Diagram](./assets/PASTA-arch.png)

> Each use case forms a **vertical slice**: its own logic, ports, and orchestration.  
> No split projects per "layer". Functional cohesion lives within the slice.

---

## 💡 Key Concepts

| Component            | Role                                                            |
|----------------------|-----------------------------------------------------------------|
| **Use Case (Slice)** | Self-contained unit: handler, ports, business logic             |
| **Core**             | Pure logic: types, validators, calculators, etc.                |
| **Handlers**         | Just functions or static orchestrators that implement use-cases |
| **Output Ports**     | Delegate passed into use case to abstract side-effects          |
| **Output Adapters**  | Implements outbound ports (DB, Email, Storage)                  |

### 💡 Why Input Ports and Adapters Are Omitted
> In this sample, we intentionally omit explicit Input Ports and Adapters because:
> - Minimal APIs already serve as the input adapter (receiving and deserializing requests)
> - The use case handler method (e.g. CreateBookingHandler.Handle) acts as a de facto input port
> 
> This approach reduces ceremony while preserving clarity, especially in small to medium vertical slices.
> You should consider introducing explicit input ports and adapters when you have multiple entry points (e.g. API + CLI + gRPC),
> need to reuse orchestration logic or have complex input mapping.
---

## 💭 Philosophy

PASTA is an architectural style for modern .NET (and beyond) projects that favors:

- Functionally cohesive, vertically sliced designs
- Delegate-based composition over interface-based ceremony
- Clear separation between core logic and I/O without overengineering

PASTA takes inspiration from Hexagonal Architecture, Functional Core/Imperative Shell, and Vertical Slice Architecture, but distills them into a leaner, practical style.

---

## 🪶PASTA Manifesto

> "Layered systems are like overcooked spaghetti — sticky, tangled, and a pain to digest. PASTA serves it al dente."

### ✅ Logic Where It Belongs

- Functional core for pure, testable rules.
- Imperative shell for orchestration and I/O.
- Use vertical slices — no extra layers, no service soup.

### 📦 Ports = Typed Functions

- Ports are delegates, not interfaces.
- Adapters implement them with real infrastructure.
- Tests use pure functions — no mocks needed.

### 🧩 Handlers Are Just Functions

- No base classes. No DI magic.
- Pass dependencies explicitly — often grouped in records.
- Composable, readable, focused.

### 🧪 Test by Design

- No need for mocks when logic is in pure functions.
- Test core logic in isolation.
- Service handlers are easy to test with hand-written or inline dependencies.
---

## 🎯 Testing Strategies

| Test Type           | Scope                                    | What to Stub                                         | Purpose                                                    |
|---------------------|------------------------------------------|------------------------------------------------------|------------------------------------------------------------|
| **Unit Tests**      | `Core` + optionally `Handlers`           | Everything (Ports, Adapters, I/O)                    | Pure logic, correctness, no side effects                   |
| **Component Tests** | `Use Case → Handlers → Adapters (owned)` | Only 3rd-party/external adapters (e.g. SMTP, Stripe) | Verify application behavior across multiple layers/modules |


## Unit testing
PASTA enforces unit testing for Core (pure domain logic: rules, validations, calculations)
and Handlers (if they orchestrate logic without side effects). The goal is to test **individual
behaviors in isolation and ensure logic correctness unders all conditions**.

It should be fast, deterministic, no I/O

![PASTA Architecture Diagram](./assets/PASTA-unit.png)

## Component testing

PASTA encourages component testing for full use case execution — 
from API adapter through handlers, core, and owned adapters.
The goal is to verify the behavior **of a complete vertical slice of the system with real infrastructure components** (e.g. in-memory DB, message broker),
while **stubbing only external dependencies you don’t own** (e.g. 3rd-party APIs, SMTP).
It ensures correctness across integrated parts you control, with realistic I/O and boundary behavior.

![PASTA Architecture Diagram](./assets/PASTA-component.png)

---
## </> Sample Code

### 🏋️‍♂️ PastaFit - Functional Booking System with PASTA (.NET 9)

PastaFit is a sample application demonstrating the PASTA architecture in a real-world fitness domain: a booking system for gym classes.
It uses:
- [x] Functional primitives with `FunqTypes.Result<T, E>`
- [x] No interfaces – just function delegates and records
- [x] No layers – just Features, Ports, Shell, and Domain
- [x] Concise and expressive handlers
- [x] Testability and separation without overengineering

### 📦 Domain: Booking gym classes

- **Booking** – links a Member to a Class
- **Class** – has name and limited capacity
- **Member** – can be active/inactive

### 🏥 Use Cases

- Book a member into a class (with validation)
- Cancel a booking
- View all classes with available capacity
- View all members

### 📈 Architecture

| Layer                        | Responsibility                                                 |
|------------------------------|----------------------------------------------------------------|
| **Core**                     | Core data types (`Booking`, `Class`, `BookingError`)           |
| **Features**                 | Use case logic (e.g.`CreateBookingHandler`)                   |
| **Output Ports (Contracts)** | Delegates like `GetClass`, `SaveBooking`, etc.                 |
| **Output Adapters**          | Real implementation of output ports (`InMemoryBookingAdapter`) |
| **Shell**                    | Minimal API endpoints and DI glue                              |


### ▶️ How to Run
```shell
dotnet run --project src/PastaFit
```
Then open http://localhost:5247 and use the .http file or run curl commands.

### 📡 API Endpoints

| Method | Route            | Description                 |
| ------ | ---------------- | --------------------------- |
| GET    | `/classes`       | List classes + availability |
| GET    | `/members`       | List all members            |
| POST   | `/bookings`      | Create booking              |
| DELETE | `/bookings/{id}` | Cancel booking              |
