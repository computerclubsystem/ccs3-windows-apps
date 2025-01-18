namespace Ccs3ClientAppWindowsService.Messages;

public class DeviceConfigurationNotificationMessageBody {
    public int PingInterval { get; set; }
}

public class DeviceConfigurationNotificationMessage : Message<DeviceConfigurationNotificationMessageBody> {
}


