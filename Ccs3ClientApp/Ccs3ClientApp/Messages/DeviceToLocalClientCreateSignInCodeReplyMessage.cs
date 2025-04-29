using Ccs3ClientApp.Messages.LocalClient.Declarations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ccs3ClientApp.Messages;


public class DeviceToLocalClientCreateSignInCodeReplyMessageBody {
    public string? Code { get; set; }
    public string? Url { get; set; }
    public string? IdentifierType { get; set; }
    public int? RemainingSeconds { get; set; }
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