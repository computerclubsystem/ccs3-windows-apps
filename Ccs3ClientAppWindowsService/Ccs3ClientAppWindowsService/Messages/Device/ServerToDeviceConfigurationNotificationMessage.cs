using Ccs3ClientAppWindowsService.Messages.Device.Declarations;

namespace Ccs3ClientAppWindowsService.Messages.Device;

public class ServerToDeviceConfigurationNotificationMessageFeatureFlags {
    public bool CodeSignIn { get; set; }
    public bool SecondPrice { get; set; }
}

public class ServerToDeviceConfigurationNotificationMessageBody {
    public int PingInterval { get; set; }
    public int? SecondsAfterStoppedBeforeRestart { get; set; }
    public int SecondsBeforeNotifyingCustomerForSessionEnd { get; set; }
    public string? SessionEndNotificationSoundFilePath { get; set; }
    public ServerToDeviceConfigurationNotificationMessageFeatureFlags? FeatureFlags { get; set; }
    public string? SecondPriceCurrency { get; set; }
}

public class ServerToDeviceConfigurationNotificationMessage : ServerToDeviceNotificationMessage<ServerToDeviceConfigurationNotificationMessageBody> {
}


