
using Ccs3ClientAppWindowsService.Messages.Device.Declarations;
using Ccs3ClientAppWindowsService.Messages.Device.Types;

namespace Ccs3ClientAppWindowsService.Messages.Device;

public class ServerToDeviceCurrentStatusNotificationMessageBody {
    public bool Started { get; set; }
    public DeviceStatusAmounts Amounts { get; set; }
}

public class ServerToDeviceCurrentStatusNotificationMessage : ServerToDeviceNotificationMessage<ServerToDeviceCurrentStatusNotificationMessageBody> {
}


