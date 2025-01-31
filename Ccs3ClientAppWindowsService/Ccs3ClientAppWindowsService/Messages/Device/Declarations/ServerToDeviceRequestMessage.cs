namespace Ccs3ClientAppWindowsService.Messages.Device.Declarations;

public class ServerToDeviceRequestMessage<TBody> {
    public ServerToDeviceRequestMessageHeader Header { get; set; }
    public TBody Body { get; set; } 
}
