using Ccs3ClientApp.Messages.LocalClient.Declarations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ccs3ClientApp.Messages;
public class LocalClientToDeviceCreateSignInCodeRequestMessageBody {
}

public class LocalClientToDeviceCreateSignInCodeRequestMessage : LocalClientToDeviceRequestMessage<LocalClientToDeviceCreateSignInCodeRequestMessageBody> {
}

public static class LocalClientToDeviceCreateSignInCodeRequestMessageHelper {
    public static LocalClientToDeviceCreateSignInCodeRequestMessage CreateMessage() {
        var msg = new LocalClientToDeviceCreateSignInCodeRequestMessage {
            Header = new LocalClientToDeviceRequestMessageHeader {
                Type = LocalClientToDeviceRequestMessageType.CreateSignInCode,
            },
            Body = new LocalClientToDeviceCreateSignInCodeRequestMessageBody(),
        };
        return msg;
    }
}