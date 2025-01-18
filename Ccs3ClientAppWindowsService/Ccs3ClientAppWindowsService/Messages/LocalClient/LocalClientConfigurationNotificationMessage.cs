using Ccs3ClientAppWindowsService.Messages.LocalClient.Declarations;

namespace Ccs3ClientAppWindowsService.Messages.LocalClient;

public class LocalClientConfigurationNotificationMessageBody {
    public int PingInterval { get; set; }
}

public class LocalClientConfigurationNotificationMessage : LocalClientNotificationMessage<LocalClientConfigurationNotificationMessageBody> {
}
