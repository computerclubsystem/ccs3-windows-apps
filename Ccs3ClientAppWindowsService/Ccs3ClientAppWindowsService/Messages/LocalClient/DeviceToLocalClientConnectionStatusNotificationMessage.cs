using Ccs3ClientAppWindowsService.Messages.LocalClient.Declarations;

namespace Ccs3ClientAppWindowsService.Messages.LocalClient;

public class DeviceToLocalClientConnectionStatusNotificationMessageBody {
    public bool Connected { get; set; }
}

public class DeviceToLocalClientConnectionStatusNotificationMessage : DeviceToLocalClientNotificationMessage<DeviceToLocalClientConnectionStatusNotificationMessageBody> {
}

public static class DeviceToLocalClientConnectionStatusNotificationMessageHelper {
    public static DeviceToLocalClientConnectionStatusNotificationMessage CreateMessage() {
        return new DeviceToLocalClientConnectionStatusNotificationMessage {
            Body = new DeviceToLocalClientConnectionStatusNotificationMessageBody(),
            Header = new DeviceToLocalClientNotificationMessageHeader {
                Type = DeviceToLocalClientNotificationMessageType.ConnectionStatus,
            }
        };
    }
}
