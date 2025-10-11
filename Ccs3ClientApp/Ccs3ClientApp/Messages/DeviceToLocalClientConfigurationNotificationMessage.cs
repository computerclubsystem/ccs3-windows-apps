using Ccs3ClientApp.Messages.LocalClient.Declarations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ccs3ClientApp.Messages;

public class DeviceToLocalClientConfigurationNotificationMessageFeatureFlags {
    public bool CodeSignIn { get; set; }
    public bool SecondPrice { get; set; }
}

public class DeviceToLocalClientConfigurationNotificationMessageBody {
    public int PingInterval { get; set; }
    public DeviceToLocalClientConfigurationNotificationMessageFeatureFlags? FeatureFlags { get; set; }
    public string? SecondPriceCurrency { get; set; }
}

public class DeviceToLocalClientConfigurationNotificationMessage : DeviceToLocalClientNotificationMessage<DeviceToLocalClientConfigurationNotificationMessageBody> {
}
