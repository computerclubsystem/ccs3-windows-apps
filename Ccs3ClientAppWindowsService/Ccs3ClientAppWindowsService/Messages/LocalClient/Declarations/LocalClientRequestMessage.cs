namespace Ccs3ClientAppWindowsService.Messages.LocalClient.Declarations;

public class LocalClientRequestMessage<TBody> {
    public LocalClientRequestMessageHeader Header { get; set; }
    public TBody Body { get; set; }
}
