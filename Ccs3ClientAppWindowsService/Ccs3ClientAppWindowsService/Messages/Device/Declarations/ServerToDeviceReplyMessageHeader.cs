namespace Ccs3ClientAppWindowsService.Messages.Device.Declarations;

public class ServerToDeviceReplyMessageHeader {
    public string Type { get; set; }
    public string CorrelationId { get; set; }
    public bool? Failure { get; set; }
    public MessageError[]? MessageErrors { get; set; }
}
