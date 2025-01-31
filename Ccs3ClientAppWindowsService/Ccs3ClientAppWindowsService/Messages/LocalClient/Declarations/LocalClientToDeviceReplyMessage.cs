namespace Ccs3ClientAppWindowsService.Messages.LocalClient.Declarations;

public class LocalClientToDeviceReplyMessage<TBody> {
    public LocalClientToDeviceReplyMessageHeader Header { get; set; }
    public TBody Body { get; set; }
}
