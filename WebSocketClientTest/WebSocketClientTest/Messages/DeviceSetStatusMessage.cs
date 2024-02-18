using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocketClientTest.Messages.Declarations;

namespace WebSocketClientTest.Messages
{
    internal class DeviceState
    {
        public static readonly string Enabled = "enabled";
        public static readonly string Disabled = "disabled";
    }

    internal class DeviceStatusAmounts
    {
        public decimal TotalSum { get; set; }
        public decimal TotalTime { get; set; }
        public int StartedAt{ get; set; }
        public int ExpectedEndAt{ get; set; }
        public int RemainingSeconds { get; set; }
    }

    internal class DeviceSetStatusMessageBody
    {
        public string State { get; set; }
        public DeviceStatusAmounts Amounts { get; set; }
    }

    internal class DeviceSetStatusMessage : Message<DeviceSetStatusMessageBody>
    {
    }
}
