using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebSocketClientTest.Messages.Declarations
{
    internal class Message<TBody>
    {
        public MessageHeader Header { get; set; } = new MessageHeader();
        public TBody Body { get; set; }
    }
}
