using Ccs3ClientAppWindowsService.Messages.LocalClient.Declarations;

namespace Ccs3ClientAppWindowsService.Messages.LocalClient;

public class DeviceToLocalClientSecondsBeforeRestartNotificationMessageBody {
    public int Seconds { get; set; }
}

public class DeviceToLocalClientSecondsBeforeRestartNotificationMessage : DeviceToLocalClientNotificationMessage<DeviceToLocalClientSecondsBeforeRestartNotificationMessageBody> {
}

public static class DeviceToLocalClientSecondsBeforeRestartNotificationMessageHelper {
    public static DeviceToLocalClientSecondsBeforeRestartNotificationMessage CreateMessage() {
        var msg = new DeviceToLocalClientSecondsBeforeRestartNotificationMessage {
            Header = new DeviceToLocalClientNotificationMessageHeader {
                Type = DeviceToLocalClientNotificationMessageType.SecondsBeforeRestart,
            },
            Body = new DeviceToLocalClientSecondsBeforeRestartNotificationMessageBody(),
        };
        return msg;
    }
}
