using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceTimerSimulator.Messages;

public class DeviceMessage<TBody> {
    public DeviceMessageHeader Header { get; set; }
    public TBody Body { get; set; }
}
