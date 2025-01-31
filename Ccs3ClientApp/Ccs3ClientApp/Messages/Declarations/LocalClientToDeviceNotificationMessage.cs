namespace Ccs3ClientAppWindowsService.Messages.LocalClient.Declarations;

public class LocalClientToDeviceNotificationMessage<TBody> {
    public LocalClientToDeviceNotificationMessageHeader Header { get; set; }
    public TBody Body { get; set; }
}
