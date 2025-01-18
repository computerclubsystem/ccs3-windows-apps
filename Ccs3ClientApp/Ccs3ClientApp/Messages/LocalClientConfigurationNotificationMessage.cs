using Ccs3ClientApp.Messages.LocalClient.Declarations;

namespace Ccs3ClientApp.Messages.LocalClient;

public class LocalClientConfigurationNotificationMessageBody {
    public int PingInterval { get; set; }
}

public class LocalClientConfigurationNotificationMessage : LocalClientNotificationMessage<LocalClientConfigurationNotificationMessageBody> {
}
