# Assignment 01 — Answers

Student name: ahmed wael 
Group / ID: Group 4

# Before 
Starting invoice export...
Customer files: D:\Simualtion\ahmed7wael-Threads-Assignment\01-InvoiceExport\InvoiceExport\bin\Debug\net9.0\Data
Invoice output: D:\Simualtion\ahmed7wael-Threads-Assignment\01-InvoiceExport\InvoiceExport\bin\Debug\net9.0\invoices

Wrote D:\Simualtion\ahmed7wael-Threads-Assignment\01-InvoiceExport\InvoiceExport\bin\Debug\net9.0\invoices\invoice-1001.txt
Wrote D:\Simualtion\ahmed7wael-Threads-Assignment\01-InvoiceExport\InvoiceExport\bin\Debug\net9.0\invoices\invoice-1002.txt
Wrote D:\Simualtion\ahmed7wael-Threads-Assignment\01-InvoiceExport\InvoiceExport\bin\Debug\net9.0\invoices\invoice-1003.txt
Wrote D:\Simualtion\ahmed7wael-Threads-Assignment\01-InvoiceExport\InvoiceExport\bin\Debug\net9.0\invoices\invoice-1004.txt
Wrote D:\Simualtion\ahmed7wael-Threads-Assignment\01-InvoiceExport\InvoiceExport\bin\Debug\net9.0\invoices\invoice-1005.txt
Export finished in 5.9 s


# after 
Starting invoice export...
Customer files: D:\Simualtion\ahmed7wael-Threads-Assignment\01-InvoiceExport\InvoiceExport\bin\Debug\net9.0\Data
Invoice output: D:\Simualtion\ahmed7wael-Threads-Assignment\01-InvoiceExport\InvoiceExport\bin\Debug\net9.0\invoices

Wrote D:\Simualtion\ahmed7wael-Threads-Assignment\01-InvoiceExport\InvoiceExport\bin\Debug\net9.0\invoices\invoice-1001.txt
Wrote D:\Simualtion\ahmed7wael-Threads-Assignment\01-InvoiceExport\InvoiceExport\bin\Debug\net9.0\invoices\invoice-1003.txt
Wrote D:\Simualtion\ahmed7wael-Threads-Assignment\01-InvoiceExport\InvoiceExport\bin\Debug\net9.0\invoices\invoice-1004.txt
Wrote D:\Simualtion\ahmed7wael-Threads-Assignment\01-InvoiceExport\InvoiceExport\bin\Debug\net9.0\invoices\invoice-1005.txt
Wrote D:\Simualtion\ahmed7wael-Threads-Assignment\01-InvoiceExport\InvoiceExport\bin\Debug\net9.0\invoices\invoice-1002.txt
Export finished in 1.8 s





## Problems with the current design

- دلوقتي الاوردرات كانت ماشية ورا بعض وكل واحد بيستني التاني يخلص عشان يبدا وده كان مبطئ العملية جامد\ وهما مش معتمدين علي بعض 

## What you changed

- خليت كل العمليات اللي بتعمل اوردر تشتغل في نفس الوقت مش مترتبين علي بعض ازاي بقي 
	# in class OrderDatabase : async GetPendingOrders() , async GetOrderById().
	# in class CustomerFileStore : async ReadProfile().
	# in class InvoiceFileWriter : async WriteInvoice().
