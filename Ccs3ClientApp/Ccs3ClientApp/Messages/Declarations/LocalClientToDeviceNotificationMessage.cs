namespace Ccs3ClientApp.Messages.LocalClient.Declarations;

public class LocalClientToDeviceNotificationMessage<TBody> {
    public LocalClientToDeviceNotificationMessageHeader Header { get; set; }
    public TBody Body { get; set; }
}
