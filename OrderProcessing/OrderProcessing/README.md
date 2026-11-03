# Phase 1: Benchmark & Analysis Questions

### 1. Why are the sync and async times almost the same when orders are awaited one after another?
* **Answer:** 
  عشان هنا احنا ماشين squantial by foreach وكل تاسك بتشتغل بعد ما اللي قبلها ما تخلص مش whenall فا الاتنين زي بعض 

---

### 2. Is the thread ID before an `await` always the same as after it? Why? Use your own log lines as evidence.
* **Answer:** 
  لا مش شرط انه يرجع نفس ال thread + in ASP.NEt مفيش حوار SynchronizationContext فا اللي بيرجع ل thread pool مش شرط هو اللي يرجع وممكن يرجع عادي 

---

### 3. What does the thread do while `Task.Delay()` is waiting? And while `Thread.Sleep()` is waiting?
* **Answer:** 
  Task.Delay() : ال thread return to thread pool non blocking.
  Thread.Sleep() : blocking وال thread بيفضل محجوز ومستني لحد ما الشغل يخلص ومش بنستفيد منه في حته تانية 

---

### 4. Why should a repository method return `Task<T>` instead of `T`?
* **Answer:** 
 عشان await return task وانا عايز استخدمها علي ال I/O operations

---

### 5. What is the difference between `File.ReadAllText` and `File.ReadAllTextAsync`? Does the async version occupy a thread while the disk is reading?
* **Answer:** 
  File.ReadAllText : Synchoronuous blocking threads.
  File.ReadAllTextAsync : Asynchoronuous non blocking threads  - thread return to thread pool 