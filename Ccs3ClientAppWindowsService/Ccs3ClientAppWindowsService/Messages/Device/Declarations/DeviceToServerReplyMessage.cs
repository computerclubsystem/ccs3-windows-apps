namespace Ccs3ClientAppWindowsService.Messages.Device.Declarations;

public class DeviceToServerReplyMessage<TBody> {
    public DeviceToServerNotificationMessageHeader Header { get; set; }
    public TBody Body { get; set; }
}
