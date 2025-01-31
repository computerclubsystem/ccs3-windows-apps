namespace Ccs3ClientAppWindowsService.Messages.Device.Declarations;

public class DeviceToServerReplyMessageHeader {
    public string Type { get; set; }
    public string CorrelationId { get; set; }
    public bool? Failure { get; set; }
    public MessageError[]? Errors { get; set; }
}
