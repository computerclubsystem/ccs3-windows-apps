using Ccs3ClientAppWindowsService.Messages.Device.Types;
using Ccs3ClientAppWindowsService.Messages.LocalClient.Declarations;

namespace Ccs3ClientAppWindowsService.Messages.LocalClient;

public class DeviceToLocalClientCurrentStatusNotificationMessageBody {
    public bool Started { get; set; }
    public DeviceStatusAmounts Amounts { get; set; }
}

public class DeviceToLocalClientCurrentStatusNotificationMessage : DeviceToLocalClientNotificationMessage<DeviceToLocalClientCurrentStatusNotificationMessageBody> {
}

public static class DeviceToLocalClientCurrentStatusNotificationMessageHelper {
    public static DeviceToLocalClientCurrentStatusNotificationMessage CreateMessage() {
        DeviceToLocalClientCurrentStatusNotificationMessage msg = new() {
            Body = new DeviceToLocalClientCurrentStatusNotificationMessageBody(),
            Header = new DeviceToLocalClientNotificationMessageHeader() {
                Type = DeviceToLocalClientNotificationMessageType.CurrentStatus,
            },
        };
        return msg;
    }
}