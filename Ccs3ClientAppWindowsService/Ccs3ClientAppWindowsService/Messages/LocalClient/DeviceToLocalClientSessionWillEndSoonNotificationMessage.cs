using Ccs3ClientAppWindowsService.Messages.LocalClient.Declarations;

namespace Ccs3ClientAppWindowsService.Messages.LocalClient;

public class DeviceToLocalClientSessionWillEndSoonNotificationMessageBody {
    public long RemainingSeconds { get; set; }
    public string? NotificationSoundFile { get; set; }
}

public class DeviceToLocalClientSessionWillEndSoonNotificationMessage : DeviceToLocalClientNotificationMessage<DeviceToLocalClientSessionWillEndSoonNotificationMessageBody> {
}

public static class DeviceToLocalClientSessionWillEndSoonNotificationMessageHelper {
    public static DeviceToLocalClientSessionWillEndSoonNotificationMessage CreateMessage() {
        var msg = new DeviceToLocalClientSessionWillEndSoonNotificationMessage {
            Header = new DeviceToLocalClientNotificationMessageHeader {
                Type = DeviceToLocalClientNotificationMessageType.SessionWillEndSoon,
            },
            Body = new DeviceToLocalClientSessionWillEndSoonNotificationMessageBody(),
        };
        return msg;
    }
}
