using Ccs3ClientApp.Messages.LocalClient.Declarations;

namespace Ccs3ClientApp.Messages;

public class DeviceToLocalClientConnectionStatusNotificationMessageBody {
    public bool Connected { get; set; }
}

public class DeviceToLocalClientConnectionStatusNotificationMessage : DeviceToLocalClientNotificationMessage<DeviceToLocalClientConnectionStatusNotificationMessageBody> {
}
