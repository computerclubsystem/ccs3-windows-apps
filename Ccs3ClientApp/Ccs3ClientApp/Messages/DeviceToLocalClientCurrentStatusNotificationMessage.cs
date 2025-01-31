using Ccs3ClientApp.Messages.Types;
using Ccs3ClientAppWindowsService.Messages.LocalClient.Declarations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ccs3ClientApp.Messages;

public class DeviceToLocalClientCurrentStatusNotificationMessageBody {
    public bool Started { get; set; }
    public DeviceStatusAmounts Amounts { get; set; }
}

public class DeviceToLocalClientCurrentStatusNotificationMessage : DeviceToLocalClientNotificationMessage<DeviceToLocalClientCurrentStatusNotificationMessageBody> {
}
