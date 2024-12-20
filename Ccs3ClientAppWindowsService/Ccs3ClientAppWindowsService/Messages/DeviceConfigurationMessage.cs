namespace Ccs3ClientAppWindowsService.Messages;

public class DeviceConfigurationMessageBody {
    public int PingInterval { get; set; }
}

public class DeviceConfigurationMessage : Message<DeviceConfigurationMessageBody> {
}


