using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceTimerSimulator.Messages;

public class DeviceMessageHeader {
    public string Type { get; set; }
    public string? CorrelationId { get; set; }
    public object? RoundTripData { get; set; }

    //    export interface DeviceMessageHeader {
    //        type: DeviceMessageType;
    //    correlationId?: string;
    //    roundTripData?: RoundTripData;
    //}
}
