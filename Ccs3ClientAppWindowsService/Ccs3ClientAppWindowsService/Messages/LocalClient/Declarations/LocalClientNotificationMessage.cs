namespace Ccs3ClientAppWindowsService.Messages.LocalClient.Declarations;

public class LocalClientNotificationMessage<TBody> {
    public LocalClientNotificationMessageHeader Header { get; set; }
    public TBody Body { get; set; }
}
