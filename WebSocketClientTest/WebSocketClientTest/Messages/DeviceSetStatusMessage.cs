using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocketClientTest.Messages.Declarations;

namespace WebSocketClientTest.Messages
{
    internal class DeviceStatusAccessType
    {
        public static readonly string Enabled = "enabled";
        public static readonly string Disabled = "disabled";
    }

    internal class DeviceStatusAmounts
    {
        public decimal TotalSum { get; set; }
        public int DurationSeconds { get; set; }
        public int? RemainingSeconds { get; set; }
    }

    internal class DeviceSetStatusMessageBody
    {
        public string AccessType { get; set; }
        public DeviceStatusAmounts Amounts { get; set; }
    }

    internal class DeviceSetStatusMessage : Message<DeviceSetStatusMessageBody>
    {
    }
}
