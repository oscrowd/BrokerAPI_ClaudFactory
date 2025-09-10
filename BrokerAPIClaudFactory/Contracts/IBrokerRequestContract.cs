namespace BrokerAPIClaudFactory.Contracts
{
    public interface IBrokerRequestContract
    {
        
        public string Method { get; set; }
        public string Path { get; set; }
    }
}
