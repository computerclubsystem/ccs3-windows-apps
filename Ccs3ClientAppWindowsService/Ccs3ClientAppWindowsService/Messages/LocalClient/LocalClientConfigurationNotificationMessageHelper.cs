using Ccs3ClientAppWindowsService.Messages.LocalClient.Declarations;

namespace Ccs3ClientAppWindowsService.Messages.LocalClient;

public static class LocalClientConfigurationNotificationMessageHelper {
    public static LocalClientConfigurationNotificationMessage CreateMessage() {
        return new LocalClientConfigurationNotificationMessage {
            Body = new LocalClientConfigurationNotificationMessageBody(),
            Header = new LocalClientNotificationMessageHeader {
                Type = LocalClientNotificationMessageType.Configuration,
            }
        };
    }
}
