namespace Ccs3ClientAppWindowsService.Messages.Device.Declarations;

public class ServerToDeviceNotificationMessage<TBody> {
    public ServerToDeviceNotificationMessageHeader Header { get; set; }
    public TBody Body { get; set; }
}

