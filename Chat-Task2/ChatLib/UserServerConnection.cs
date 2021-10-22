using System.IO.Pipes;

namespace ChatLib
{
    public class UserServerConnection
    {
        public User User { get; set; }
        public NamedPipeServerStream SendPipe { get; set; }
        public NamedPipeServerStream ReceivePipe { get; set; }
    }
}
