
using Ccs3ClientApp.Messages.LocalClient.Declarations;
using Ccs3ClientApp.Messages.Types;

namespace Ccs3ClientApp.Messages;
public class LocalClientStatusNotificationMessageBody {
    public bool Started { get; set; }
    public DeviceStatusAmounts Amounts { get; set; }
}

public class LocalClientStatusNotificationMessage : LocalClientNotificationMessage<LocalClientStatusNotificationMessageBody> {
}
