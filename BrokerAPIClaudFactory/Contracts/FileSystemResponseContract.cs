using System.ComponentModel.DataAnnotations;

namespace BrokerAPIClaudFactory.Contracts
{
    public class FileSystemResponseContract
    {
        [Required]
        public string Key { get; set; }
        public bool IsNewRequest { get; set; }
        public string Message { get; set; }

    }

}
