namespace Ccs3ClientAppWindowsService.Messages.Device.Declarations;

public class ServerToDeviceReplyMessage<TBody> {
    public ServerToDeviceReplyMessageHeader Header { get; set; }
    public TBody Body { get; set; }
}
