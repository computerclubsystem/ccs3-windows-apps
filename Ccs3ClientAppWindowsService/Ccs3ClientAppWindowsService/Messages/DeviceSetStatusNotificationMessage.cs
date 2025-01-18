using Ccs3ClientAppWindowsService.Messages.Types;

namespace Ccs3ClientAppWindowsService.Messages;

public class DeviceSetStatusNotificationMessageBody {
    public bool Started { get; set; }
    public DeviceStatusAmounts Amounts { get; set; }
}

public class DeviceSetStatusNotificationMessage : Message<DeviceConfigurationNotificationMessageBody> {
}


