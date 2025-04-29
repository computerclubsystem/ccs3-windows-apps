using Ccs3ClientAppWindowsService.Messages.Device.Declarations;

namespace Ccs3ClientAppWindowsService.Messages.Device;

public class ServerToDeviceCreateSignInCodeReplyMessageBody {
    public string? Code { get; set; }
    public int? RemainingSeconds { get; set; }
    public string? Url { get; set; }
    public string? IdentifierType { get; set; }
}


public class ServerToDeviceCreateSignInCodeReplyMessage : ServerToDeviceReplyMessage<ServerToDeviceCreateSignInCodeReplyMessageBody> {
}
