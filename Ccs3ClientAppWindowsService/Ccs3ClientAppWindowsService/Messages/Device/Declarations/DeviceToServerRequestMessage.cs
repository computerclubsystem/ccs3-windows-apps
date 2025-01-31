namespace Ccs3ClientAppWindowsService.Messages.Device.Declarations;

public class DeviceToServerRequestMessage<TBody> {
    public DeviceToServerRequestMessageHeader Header { get; set; }
    public TBody Body { get; set; }
}
