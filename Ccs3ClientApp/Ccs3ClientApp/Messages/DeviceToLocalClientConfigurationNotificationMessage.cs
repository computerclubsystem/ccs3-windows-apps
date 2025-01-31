using Ccs3ClientAppWindowsService.Messages.LocalClient.Declarations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ccs3ClientApp.Messages;

public class DeviceToLocalClientConfigurationNotificationMessageBody {
    public int PingInterval { get; set; }
}

public class DeviceToLocalClientConfigurationNotificationMessage : DeviceToLocalClientNotificationMessage<DeviceToLocalClientConfigurationNotificationMessageBody> {
}
