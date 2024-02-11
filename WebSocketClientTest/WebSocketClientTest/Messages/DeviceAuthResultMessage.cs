using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocketClientTest.Messages.Declarations;

namespace WebSocketClientTest.Messages
{
    internal class DeviceAuthResultSettings
    {
        public int ReportDiagnosticsInterval { get; set; }
        public string? NewVersionUrl { get; set; }
    }

    internal class DeviceAuthResultMessageBody
    {
        public bool Authenticated { get; set; }
        public DeviceAuthResultSettings? Settings { get; set; }
    }

    internal class DeviceAuthResultMessage : Message<DeviceAuthResultMessageBody>
    {
    }
}
