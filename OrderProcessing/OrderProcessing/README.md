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
[02:32:52.720] [Thread 32] OrderRepository -> Opening connection...
[EmailService] -> Sending confirmation email for Order #99...
[EmailService] -> Email Sent Successfully!
[02:32:52.927] [Thread 32] OrderRepository -> Beginning transaction...
[02:32:53.041] [Thread 32] OrderRepository -> Saving new Order #99...
[02:32:53.457] [Thread 9] OrderRepository -> Committing transaction...
[02:32:53.663] [Thread 32] OrderRepository -> Order #99 successfully saved.
Order processing finished

# Task 9.7 — Fire-and-Forget Investigation

### Question 9:
**Q:** Show what happens if the email is still being sent when the method returns, or when the console app
exits?
**A:** هيحصل Data lossممكن والايميل ميتعبتش لانك مبتنستناش الفانكشن تخلص ومفيش await which do pause 

### Question 10:
**Q:** Show what happens if the email operation throws an exception.?
**A:** مش هعرف اجيب الاكسبشن ده ليه بقي عشان await هي ال catch Exceptions

---
# Phase 4 Questions & Answers

### Question 1
**Q:** For each of Tasks 10.1–10.4: which tool did you use, and why can't "start, then await one by one" meet the requirement?
**A:** Tasks 10.1 use Task.WhenAny()
	   Tasks 10.2 use Task.WhenAll()	
	   Tasks 10.3 use Task.WhenAny()
	   Tasks 10.4 Parallel.For للـ CPU-bound and Task.Run للـ I/O-bound
		عشان لما كل واحدة تستني التانية تخلص الاول كدة انا مستفدتيش حاجه من Async وال الاداء هيبطئ جامد ما كله يخلص

### Question 2
**Q:** In 10.2, when several loads fail, how do you get every exception? What does a plain await give you?
**A:** الـ await العادي لـ Task.WhenAll (أو الانتظار الفردي) بيرجعلك الاستثناء الأول فقط الذي وقع، ويقوم بـ Short-circuiting متجاهلاً باقي الأخطاء.

### Question 3
**Q:** In 10.3, what happens to the losing carriers if you never cancel them?
**A:** هتفضل شغالة عادي وتستهالك موارد ال Thread pool حتي لو كل حاجه خلصت وهيحصل استنزاف للموارد اللي عندي

### Question 4
**Q:** Why does 10.4 A get faster with more CPU cores, while 10.4 B does not depend on the number of cores?
**A:** 
* في 10.4 A دي شغلانة حسابات ثقيلة على المعالج (CPU-bound) واستخدمت فيها `Parallel.For` فبتوزع الشغل على كل أنوية المعالج المتاحة عشان يحسبوهم في نفس اللحظة بالتوازي، فكل ما تزيد الأنوية الأداء بيسرع.
* أما في 10.4 B دي شغلانة انتظار وكتابة ملفات (I/O-bound) والسرعة مش فارق معاها المعالج خالص، بل محكومة بـ `SemaphoreSlim` بحد أقصى 4 إرسالات مع بعض بغض النظر عن عدد الأنوية في الجهاز.

### Question 5
**Q:** What is the difference between parallel work (10.4 A) and concurrent asynchronous work (10.1)?
**A:** 
* الـ Parallel (في 10.4 A): يخص الـ CPU الحسابي؛ بنقسم الشغل فيزيائياً على أكتر من كور عشان يشتغلوا مع بعض في نفس الكسر من الثانية.
* أما الـ Async/Concurrent (في 10.1): يخص الـ I/O؛ مش بحجز الـ Thread ولا بعطل المعالج وهو مستني الردود، بل الـ Thread بيفضل حر يدير مهام تانية بكفاءة من غير استهلاك زيادة في الموارد.

### Question 6
**Q:** Show a cancelled run (--cancel-after) and explain what happened to the orders, the stock and the payments.
**A:** 
* لما بتفعل الإلغاء، الـ `CancellationToken` بيوقف كل التاسكات الشغالة فوراً وبسلاسة عند أول نقطة فحص.
* الطلبات (Orders): تتوقف عن متابعة دورة حياتها ومبتكملش باقي الخطوات.
* المخزون (Stock): بيعمل Rollback ومبيخصمش الكميات في الهواء من غير أوردر مكتمل.
* المدفوعات (Payments): بتتكنسل فوراً أو تقف بأمان لمنع أي خصم مالي غير مكتمل أو تلف في ملفات الـ JSON.
