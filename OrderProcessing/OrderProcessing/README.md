# E-Commerce Order Processing System (File Storage Edition)

## 1. Project Overview
An enterprise-grade, highly concurrent e-commerce order processing engine built from first-principles using **C#** and **.NET 10**. The system completely replaces a traditional relational database and network APIs with a robust, thread-safe **JSON File Storage Architecture** on disk. It handles end-to-end order lifecycles—ranging from customer validation, inventory management, dynamic pricing, external payment gateways, and resilient email dispatching with automated retries.

## 2. Project Goals
* Demonstrate advanced asynchronous programming paradigms (`async/await`, `Task.WhenAny`, `Task.WhenAll`, `SemaphoreSlim`).
* Implement strict thread-safe file operations without corrupting JSON collections under heavy concurrency.
* Manage distributed transaction compensations (Saga-lite pattern: stock rollbacks and payment refunds on failure).
* Scale up data processing pipelines using parallel computing (`Parallel.For`) and concurrent data structures.

## 3. Features
* **Async File Data Access Layer (`JsonFileStore`):** Custom thread-safe reader/writer with atomic file swapping and corruption handling (`DataFileCorruptedException`).
* **Concurrent Pipeline Processing:** Processes multiple orders simultaneously with non-blocking I/O waits.
* **Resilient Payment & Email Integrations:** Simulates network latency, gateway timeouts, card declines, and multi-tier exponential backoff email retries.
* **Advanced Scale-Up Modules:** Live progress tracking, parallel CPU-bound fraud scoring, asynchronous multi-load customer dashboards, and competitive shipping carrier quote selection.

## 4. Technologies Used
* **Language:** C# 12 / .NET 10 (`<TargetFramework>net10.0</TargetFramework>`)
* **Core Libraries:** `System.Text.Json`, `System.Diagnostics`, `System.Threading.Channels`, `ConcurrentBag`, `SemaphoreSlim`.
* **Architecture:** Manual constructor dependency injection (No external DI containers or generic repository wrappers).

## 5. Project Structure
```text
OrderProcessing/
│
├── Shared/
│   ├── Models/             # Domain models (Customer, Product, Order, PaymentTransaction, etc.)
│   ├── Enums/              # Status enums (OrderStatus, PaymentStatus, LoyaltyTier)
│   └── Custom_Exceptions/  # Custom domain exceptions
│
├── Storage/
│   ├── JsonFileStore.cs    # Thread-safe JSON file persistence engine
│   └── DataSeeder.cs       # Environment initializer and seed data generator
│
├── Repositories/           # Async and Sync repository layers for entities
├── Services/               # Business logic services (Pricing, Inventory, Payment, Email, Shipping)
│
├── Phase1/                 # Foundation & Async Data Access Layer
├── Phase2/                 # Concurrent Processing, Pricing, and Race Conditions
├── Phase3/                 # Full Checkout Workflow, Retries, and Failures
└── Phase4/                 # Scale-Up (Live Progress, Dashboards, Shipping Quotes, Parallel Scoring)
```

## 6. Architecture & Design Decisions
* **File Storage Concurrency:** `JsonFileStore` utilizes synchronization primitives (`SemaphoreSlim` / locks) combined with atomic temporary file writing (`File.WriteAllText` followed by a safe rename/replace) to ensure zero data loss or half-written file corruption when multiple threads read/write simultaneously.
* **No Generic Repositories:** To adhere strictly to clean boundaries and avoid unnecessary abstractions, specific repositories (`CustomerRepository`, `ProductRepository`, etc.) are implemented directly with clear separation of concerns.

## 7. Explanation of Key Classes & Components
* **`JsonFileStore`:** The central data access gatekeeper. Guarantees thread safety and handles missing or corrupted files gracefully.
* **`OrderProcessor`:** Orchestrates the 7-step checkout workflow, enforcing transaction compensations (e.g., refunding payments and releasing reserved inventory if database commits fail).
* **`ShippingQuoteService`:** Uses `Task.WhenAny` to race multiple shipping carriers (FastShip, CairoExpress, NileCourier) and select the fastest successful response within a strict time budget.

## 8. Execution Flow
1. **Seeding:** The application resets the root `Data/` folder and populates baseline JSON files (`customers.json`, `products.json`, `coupons.json`, `incoming_orders.json`).
2. **Validation & Processing:** Orders are read, validated against business rules, processed concurrently through inventory reservation, pricing computation, payment authorization, and confirmed via email dispatch.
3. **Persistence:** Final states are updated in `orders.json`, audit logs are written to `payments.json`, and failed notifications are redirected to `email_retry.json`.

## 9. Prerequisites
* .NET 10 SDK installed on your local machine.

## 10. Installation & Setup
1. Clone the repository or open the project folder in your terminal.
2. Ensure you are using a Windows/Linux/macOS environment supporting .NET 10.

## 11. How to Run the Project
Run the application via terminal using the dotnet CLI:

```bash
dotnet run
```

You can choose the respective phase runner or menu option directly from `Program.cs`.

---

# Answers to Phase 1–4 Questions

## Phase 1 Questions

**Why are the sync and async times almost the same when orders are awaited one after another?**
Because processing them sequentially (`await` inside a `foreach` loop) means each operation still waits for the previous one to finish before starting, turning the asynchronous calls into a sequential pipeline.

**Is the thread ID before an `await` always the same as after it? Why?**
Not necessarily. Without a synchronization context (like in a Console application), the continuation after an `await` can be scheduled on any available thread from the ThreadPool.

**What does the thread do while `Task.Delay()` is waiting? And while `Thread.Sleep()` is waiting?**
`Task.Delay()` frees up the current thread to do other work while waiting asynchronously. `Thread.Sleep()` blocks the current thread entirely, keeping it idle and unusable.

**Why should a repository method return `Task<T>` instead of `T`?**
To signal that the operation performs non-blocking I/O or simulated latency asynchronously, allowing the caller to `await` it without freezing the calling thread.

**What is the difference between `File.ReadAllText` and `File.ReadAllTextAsync`? Does the async version occupy a thread while the disk is reading?**
`File.ReadAllText` blocks the thread synchronously. `File.ReadAllTextAsync` leverages modern asynchronous file streams; while it utilizes I/O completion ports efficiently, .NET's underlying async file I/O implementation may still use thread pool threads for disk operations depending on the OS.

## Phase 2 Questions

**What is the difference between starting a task and awaiting it?**
Starting a task schedules its execution on the ThreadPool. Awaiting it pauses the calling method's execution until the task completes, retrieving its result or propagating any exceptions.

**Does every `Task` create a new thread? Does `Task.Delay()` use a thread while waiting?**
No. Tasks represent work items managed by the `TaskScheduler` and ThreadPool. `Task.Delay()` uses timers and does not consume a dedicated thread while waiting.

**What happens if you call `.Result` or `.Wait()` on an unfinished task instead of `await`? Why is it dangerous?**
It blocks the calling thread synchronously, which can easily cause deadlocks (especially in UI or synchronization-context-bound applications) and wastes thread pool resources.

**What happens to an exception thrown inside a task that is never awaited?**
In modern .NET, unobserved task exceptions can trigger `UnobservedTaskException` events, which historically could terminate the process depending on runtime configurations.

**When should you use `Task.Run()`, and when should you avoid it?**
Use `Task.Run()` for CPU-bound work to offload it from the main thread. Avoid it for I/O-bound work (like file or network calls), which should be truly asynchronous natively.

**What is the difference between `Task` and `Task<T>`?**
`Task` represents an asynchronous operation with no return value (like `void`), whereas `Task<T>` represents an operation that returns a value of type `T`.

**Why can a `lock` block not contain an `await`?**
Because C# `lock` relies on `Monitor.Enter` and `Monitor.Exit`, which are tied to a specific physical thread. Since `await` can resume execution on a different thread, the lock ownership would become invalid and unsafe.

**Why did the unprotected stock reservation oversell, and how did you prevent it?**
Multiple threads read the same stock value simultaneously before any write occurred (race condition). We prevented it by wrapping the check-and-update logic inside a thread-safe transaction lock (lock or database-like transaction simulation).

## Phase 3 Questions

**Does async make one order's workflow faster?**
No, async does not speed up individual CPU or latency steps; it improves throughput and scalability by preventing threads from blocking during waits.

**Why does async scale better for many orders?**
Because threads are not blocked during I/O delays, a small pool of threads can handle thousands of concurrent requests.

**What happens to the sync version with 1,000 orders?**
It would exhaust the ThreadPool, consume excessive memory for thread stacks, and suffer from severe thread-context-switching overhead.

**Why is async essential for apps that call databases, files and APIs?**
It eliminates thread starvation, maximizing hardware utilization under high concurrency.

## Phase 4 Questions

**Scale-up justifications:**
* `Task.WhenAny` was chosen for Task 10.1 to stream real-time task completions instantly.
* `Parallel.For` was used for Task 10.4A to maximize all available CPU cores for heavy mathematical scoring.
* `SemaphoreSlim` throttled concurrent I/O operations in Task 10.4B without custom loop-thread overhead.
