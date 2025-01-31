using Ccs3ClientAppWindowsService.Messages.Device.Declarations;

namespace Ccs3ClientAppWindowsService.Messages.Device;

public class DeviceToServerPingNotificationMessageBody {
}

public class DeviceToServerPingNotificationMessage : DeviceToServerNotificationMessage<DeviceToServerPingNotificationMessageBody> {
}

public static class DeviceToServerPingNotificationMessageHelper {
    public static DeviceToServerPingNotificationMessage CreateMessage() {
        var msg = new DeviceToServerPingNotificationMessage {
            Header = new DeviceToServerNotificationMessageHeader {
                Type = DeviceToServerNotificationMessageType.Ping,
            },
            Body = new DeviceToServerPingNotificationMessageBody(),
        };
        return msg;
    }
}