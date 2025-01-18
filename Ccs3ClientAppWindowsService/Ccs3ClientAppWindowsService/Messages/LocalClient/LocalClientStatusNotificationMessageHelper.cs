using Ccs3ClientAppWindowsService.Messages.LocalClient.Declarations;

namespace Ccs3ClientAppWindowsService.Messages.LocalClient;

public static class LocalClientStatusNotificationMessageHelper {
    public static LocalClientStatusNotificationMessage CreateMessage() {
        LocalClientStatusNotificationMessage msg = new() {
            Body = new LocalClientStatusNotificationMessageBody(),
            Header = new Declarations.LocalClientNotificationMessageHeader() {
                Type = LocalClientNotificationMessageType.Status,
            }
        };
        return msg;
    }
}
