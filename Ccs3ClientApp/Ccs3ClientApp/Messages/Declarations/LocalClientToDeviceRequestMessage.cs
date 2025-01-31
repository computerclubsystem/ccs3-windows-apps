namespace Ccs3ClientAppWindowsService.Messages.LocalClient.Declarations;

public class LocalClientToDeviceRequestMessage<TBody> {
    public LocalClientToDeviceRequestMessageHeader Header { get; set; }
    public TBody Body { get; set; }
}
