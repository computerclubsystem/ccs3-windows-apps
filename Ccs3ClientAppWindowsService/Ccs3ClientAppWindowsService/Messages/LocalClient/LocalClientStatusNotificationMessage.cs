using Ccs3ClientAppWindowsService.Messages.LocalClient.Declarations;
using Ccs3ClientAppWindowsService.Messages.Types;

namespace Ccs3ClientAppWindowsService.Messages.LocalClient;

public class LocalClientStatusNotificationMessageBody {
    public bool Started { get; set; }
    public DeviceStatusAmounts Amounts { get; set; }
}

public class LocalClientStatusNotificationMessage : LocalClientNotificationMessage<LocalClientStatusNotificationMessageBody> {
}
