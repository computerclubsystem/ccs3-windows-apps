using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocketClientTest.Messages.Declarations;

namespace WebSocketClientTest.Messages
{
    internal class DeviceAuthMessageBody
    {
        public string DeviceId { get; set; }
        public string SoftwareVersion { get; set; }
    }

    internal class DeviceAuthMessage : Message<DeviceAuthMessageBody>
    {
    }

    internal static class DeviceAuthMessageFactory
    {
        public static DeviceAuthMessage Create()
        {
            DeviceAuthMessage msg = new();
            msg.Header.Type = MessageType.DeviceAuth;
            msg.Body = new DeviceAuthMessageBody();
            return msg;
        }
    }
}
