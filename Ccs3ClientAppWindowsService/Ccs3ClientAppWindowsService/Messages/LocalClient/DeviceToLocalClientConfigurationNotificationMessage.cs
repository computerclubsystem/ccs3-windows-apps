using Ccs3ClientAppWindowsService.Messages.LocalClient.Declarations;

namespace Ccs3ClientAppWindowsService.Messages.LocalClient;

public class DeviceToLocalClientConfigurationNotificationMessageBody {
    public int PingInterval { get; set; }
}

public class DeviceToLocalClientConfigurationNotificationMessage : DeviceToLocalClientNotificationMessage<DeviceToLocalClientConfigurationNotificationMessageBody> {
}

public static class DeviceToLocalClientConfigurationNotificationMessageHelper {
    public static DeviceToLocalClientConfigurationNotificationMessage CreateMessage() {
        return new DeviceToLocalClientConfigurationNotificationMessage {
            Body = new DeviceToLocalClientConfigurationNotificationMessageBody(),
            Header = new DeviceToLocalClientNotificationMessageHeader {
                Type = DeviceToLocalClientNotificationMessageType.Configuration,
            }
        };
    }
}
