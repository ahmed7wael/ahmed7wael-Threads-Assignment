    namespace OrderProcessing;

    public class DataFiles
    {
        public const string CustomersFileName = "customers.json";
        public const string ProductsFileName = "products.json";
        public const string CouponsFileName = "coupons.json";
        public const string IncomingOrdersFileName = "incoming_orders.json";
        public const string OrdersFileName = "orders.json";
        public const string PaymentsFileName = "payments.json";
        public const string EmailsFileName = "emails.json";
        public const string EmailRetryFileName = "email_retry.json";


        public static readonly string DataFolder = Path.Combine(AppContext.BaseDirectory, "Data");

        public static readonly string customers = Path.Combine(DataFolder, CustomersFileName);
        public static readonly string products = Path.Combine(DataFolder, ProductsFileName);
        public static readonly string coupons = Path.Combine(DataFolder, CouponsFileName);
        public static readonly string incoming_orders = Path.Combine(DataFolder, IncomingOrdersFileName);
        public static readonly string orders = Path.Combine(DataFolder, OrdersFileName);
        public static readonly string payments = Path.Combine(DataFolder, PaymentsFileName);
        public static readonly string emails = Path.Combine(DataFolder, EmailsFileName);
        public static readonly string email_retry = Path.Combine(DataFolder, EmailRetryFileName);
        public static readonly string InvoicesFolder = Path.Combine(DataFolder, "invoices");
        public static readonly string ReportsFolder = Path.Combine(DataFolder, "reports");


    }
