using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceTimerSimulator.Messages;

public class DeviceStatusDeviceMessage : DeviceMessage<DeviceStatusDeviceMessageBody> {
    public DeviceStatusDeviceMessage()
    {
        Header = new DeviceMessageHeader();
        Header.Type = DeviceMessageType.DeviceStatus;
    }
}

public class DeviceStatusDeviceMessageBody {
    public float? CpuTemp { get; set; }
    public float? CpuUsage { get; set; }
    public long? StorageFreeSpace { get; set; }
    public bool? Input1Value { get; set; }
    public bool? Input2Value { get; set; }
    public bool? Input3Value { get; set; }
    public bool? Output1Value { get; set; }
    public bool? Output2Value { get; set; }
    public bool? Output3Value { get; set; }
    public DateTimeOffset? LastTimeAddedAt { get; set; }
    public int? RemainingSeconds { get; set; }
    public DateTimeOffset? CurrentTime { get; set; }
}
