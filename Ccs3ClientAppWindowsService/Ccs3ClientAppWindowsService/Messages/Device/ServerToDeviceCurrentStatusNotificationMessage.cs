﻿
using Ccs3ClientAppWindowsService.Messages.Device.Declarations;
using Ccs3ClientAppWindowsService.Messages.Device.Types;

namespace Ccs3ClientAppWindowsService.Messages.Device;

public class ServerToDeviceCurrentStatusNotificationMessageBody {
    public bool Started { get; set; }
    public int? TariffId { get; set; }
    public bool? CanBeStoppedByCustomer { get; set; }
    public DeviceStatusAmounts Amounts { get; set; }
    public TariffShortInfo? ContinuationTariffShortInfo { get; set; }
}

public class ServerToDeviceCurrentStatusNotificationMessage : ServerToDeviceNotificationMessage<ServerToDeviceCurrentStatusNotificationMessageBody> {
}


