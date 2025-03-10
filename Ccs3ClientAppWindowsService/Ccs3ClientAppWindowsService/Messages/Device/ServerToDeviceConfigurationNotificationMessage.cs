﻿using Ccs3ClientAppWindowsService.Messages.Device.Declarations;

namespace Ccs3ClientAppWindowsService.Messages.Device;

public class ServerToDeviceConfigurationNotificationMessageBody {
    public int PingInterval { get; set; }
    public int? SecondsAfterStoppedBeforeRestart { get; set; }
}

public class ServerToDeviceConfigurationNotificationMessage : ServerToDeviceNotificationMessage<ServerToDeviceConfigurationNotificationMessageBody> {
}


