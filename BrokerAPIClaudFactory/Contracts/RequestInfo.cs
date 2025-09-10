namespace BrokerAPIClaudFactory.Contracts
{
    public class RequestInfo
    {
        public string Key { get; set; }
        public string Method { get; set; }
        public string Path { get; set; }
        public string QueryString { get; set; }
        public string Headers { get; set; }
        public string Body { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<string> WaitingClients { get; set; } = new List<string>();
    }
}
