using System.ComponentModel.DataAnnotations;

namespace BrokerAPIClaudFactory.Contracts
{
    public class FileSystemRequestContract : IBrokerRequestContract
    {
        [Required]
        public string Method { get; set; }
        [Required]
        public string Path { get; set; }
    }

    public enum Method
    {
        GET,
        POST,
        PUT,
        DELETE,

    }
}
