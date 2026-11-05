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

  ==========================================================================================================================
  # Phase 2 & Async Programming Discussion Questions

### Question 1:
**Q:** What is the difference between starting a task and awaiting it?
**A:** التاسك بتبدا بتعمل الشغل في الخلفية وترجع تكمل الكود عادي لكن ال await بتوقف الفانكشن لحد ما الشغل كله يخلص وترجع تكمل 


---

### Question 2:
**Q:** Does every Task create a new thread? Does `Task.Delay()` use a thread while waiting?
**A:** no threads uses thread pool and Task.Delay() : non blocking thread return to thread pool 


---

### Question 3:
**Q:** What happens if you call `.Result` or `.Wait()` on an unfinished task instead of `await`? Why is it dangerous?
**A:** هيحصل blocking لحد ما التاسك تخلص 


---

### Question 4:
**Q:** What happens to an exception thrown inside a task that is never awaited?
**A:** مش هعرف اجيب الخطا لان await هي اللي كانت بتخلينا نجيبه 


---

### Question 5:
**Q:** When should you use `Task.Run()`, and when should you avoid it?
**A:** use with cpu bound and avoid with I/O bound


---

### Question 6:
**Q:** What is the difference between `Task` and `Task<T>`?
**A:** Task return void and Task<T> return type of T


---

### Question 7:
**Q:** Why can a `lock` block not contain an `await`?
**A:** عشان دلوقتي lock مبتخليش غير thread واحد بس اللي يدخل لكن await بترجع ال thread to thread pool 


---

### Question 8:
**Q:** Why did the unprotected stock reservation oversell, and how did you prevent it?
**A:** بسبب Race Condition وبنحلها ب lock , semaphoreslim , interlocked



--- 
phase 3 
# Before 
[01:43:44.996] [Thread 34] OrderRepository -> Opening connection...
[01:43:45.201] [Thread 34] OrderRepository -> Beginning transaction...
[01:43:45.314] [Thread 34] OrderRepository -> Saving new Order #99...
[01:43:45.725] [Thread 34] OrderRepository -> Committing transaction...
[01:43:45.926] [Thread 34] OrderRepository -> Order #99 successfully saved.
Order processing finished

# after
[01:47:54.386] [Thread 32] OrderRepository -> Opening connection...
[01:47:54.588] [Thread 32] OrderRepository -> Beginning transaction...
[01:47:54.701] [Thread 32] OrderRepository -> Saving new Order #99...
[01:47:55.115] [Thread 32] OrderRepository -> Committing transaction...
[01:47:55.322] [Thread 32] OrderRepository -> Order #99 successfully saved.
Order processing finished
