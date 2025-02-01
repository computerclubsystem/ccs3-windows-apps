using Ccs3ClientApp.Messages.LocalClient.Declarations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ccs3ClientApp.Messages;

public class DeviceToLocalClientSecondsBeforeRestartNotificationMessageBody {
    public int Seconds { get; set; }
}

public class DeviceToLocalClientSecondsBeforeRestartNotificationMessage : DeviceToLocalClientNotificationMessage<DeviceToLocalClientSecondsBeforeRestartNotificationMessageBody> {
}
