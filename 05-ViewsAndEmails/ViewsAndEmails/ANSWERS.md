# Assignment 05 — Answers

Student name:  
Group / ID:

# Before 
1 - Unhandled exception. System.Collections.Generic.KeyNotFoundException: The given key 'koshary-bowl' was not present in the dictionary.
2 - Views expected=50000, actual=20930
	Email jobs placed=20, unique sent=20, total sends=20

3 - Views expected=50000, actual=17387
	Email jobs placed=20, unique sent=20, total sends=20




## Failures you observed

- in Dictonary and Queue not thread safe فا ممكن اكتر من  thread يدخل ويعدل في نفس الوقت وساعتها هيحصل daya corruptiion and lost update 

## What you changed

- instead of Dictionary use ConcurrentDictionary and use function addOrUpdate 
- instead of Queue use ConcurrentQueue and use function TryDequeue 

## Collection safety vs business-rule safety

- ال Collection safety : مصمم عشان انه ميضربش ex او يحصل مشاكل لو اكتر من thread دخلوا في نفس الوقت عشان يعدلوا 
- اما ال business-rule safety : انت اللي بتكون بتتحكم في الخطوات والمفروض تحافظ انه ميحصلش مشاكل ال race condition 
