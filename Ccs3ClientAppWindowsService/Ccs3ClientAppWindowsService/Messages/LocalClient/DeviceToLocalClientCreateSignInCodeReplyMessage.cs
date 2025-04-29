using Ccs3ClientAppWindowsService.Messages.LocalClient.Declarations;

namespace Ccs3ClientAppWindowsService.Messages.LocalClient;

public class DeviceToLocalClientCreateSignInCodeReplyMessageBody {
    public string? Code { get; set; }
    public int? RemainingSeconds { get; set; }
    public string? Url { get; set; }
    public string? IdentifierType { get; set; }
}

public class DeviceToLocalClientCreateSignInCodeReplyMessage : DeviceToLocalClientReplyMessage<DeviceToLocalClientCreateSignInCodeReplyMessageBody> {
}

public static class DeviceToLocalClientCreateSignInCodeReplyMessageHelper {
    public static DeviceToLocalClientCreateSignInCodeReplyMessage CreateMessage() {
        var msg = new DeviceToLocalClientCreateSignInCodeReplyMessage {
            Header = new DeviceToLocalClientReplyMessageHeader {
                Type = DeviceToLocalClientReplyMessageType.CreateSignInCode,
            },
            Body = new DeviceToLocalClientCreateSignInCodeReplyMessageBody(),
        };
        return msg;
    }
}