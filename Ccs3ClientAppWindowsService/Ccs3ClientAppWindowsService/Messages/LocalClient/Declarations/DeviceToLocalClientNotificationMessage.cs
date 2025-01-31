namespace Ccs3ClientAppWindowsService.Messages.LocalClient.Declarations;

public class DeviceToLocalClientNotificationMessage<TBody> {
    public DeviceToLocalClientNotificationMessageHeader Header { get; set; }
    public TBody Body { get; set; } 
}
