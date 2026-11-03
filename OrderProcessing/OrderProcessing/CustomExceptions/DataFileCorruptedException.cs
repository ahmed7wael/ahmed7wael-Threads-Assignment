namespace OrderProcessing
{
    public class DataFileCorruptedException : Exception
    {

        public string FileName { get; }
        public DataFileCorruptedException(string fileName, string massage, Exception? innerException) : base(massage, innerException)
        {
            FileName = fileName;
        }   


    }
}
