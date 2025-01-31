namespace Ccs3ClientApp.Messages.LocalClient.Declarations;

public class DeviceToLocalClientRequestMessage<TBody> {
    public DeviceToLocalClientRequestMessageHeader Header { get; set; }
    public TBody Body { get; set; }
}
