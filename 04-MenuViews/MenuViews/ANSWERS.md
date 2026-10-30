# Assignment 04 — Answers

Student name: ahmed wael 
Group / ID: Group 4

# Before 
Expected: 400000
Actual:   208736

# After 
Expected: 400000
Actual:   400000


## Why Expected and Actual disagree

- بسبب ان ال threads بتدخل كلها في نفس الوقت فا بيحصل مشاكل وممكن اكتر من thread يعدل في نفس الوقت علي القيمة القديمة عشان كدة القيمة  بينهم بتطلع كل شوية متغيرة وكمان بسبب ان العملية مش atomic 

## What you changed

- in class MenuViewCounter : use Interlocked.Increment(ref Views); 

## When would you protect the update differently

- عشان ال loct بنستخدمها مع ال عمليات اللي الكبيرة اللي فيها شروط واني عايز اتحكم في ال threads انه واحد بس اللي يدخل لكن هنا انا عندي عملية واحدة بس عايز احولها atomic 
