namespace Ccs3ClientAppWindowsService.Messages.Device.Declarations;

public class DeviceToServerNotificationMessage<TBody> {
    public DeviceToServerNotificationMessageHeader Header { get; set; }
    public TBody Body { get; set; }
}
