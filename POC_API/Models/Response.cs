namespace POC_API.Models
{
    public class Response
    {
        public int StatusCode { get; set; }

        public string StatusMessage { get; set; }

        public object Data { get; set; }
    }
}
