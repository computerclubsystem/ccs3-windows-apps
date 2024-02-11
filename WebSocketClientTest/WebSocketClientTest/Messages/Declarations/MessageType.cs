using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebSocketClientTest.Messages.Declarations
{
    internal static class MessageType
    {
        public static readonly string DeviceAuth = "device-auth";
        public static readonly string DeviceAuthResut = "device-auth-result";
        public static readonly string DeviceSetStatus = "device-set-status";
    }
}
