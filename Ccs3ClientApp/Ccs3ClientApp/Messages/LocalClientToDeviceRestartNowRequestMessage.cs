using Ccs3ClientApp.Messages.LocalClient.Declarations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ccs3ClientApp.Messages;

public class LocalClientToDeviceRestartNowRequestMessageBody {
}

public class LocalClientToDeviceRestartNowRequestMessage : LocalClientToDeviceRequestMessage<LocalClientToDeviceRestartNowRequestMessageBody> {
}

public static class LocalClientToDeviceRestartNowRequestMessageHelper {
    public static LocalClientToDeviceRestartNowRequestMessage CreateMessage() {
        var msg = new LocalClientToDeviceRestartNowRequestMessage {
            Header = new LocalClientToDeviceRequestMessageHeader {
                Type = LocalClientToDeviceRequestMessageType.RestartNow,
            },
            Body = new LocalClientToDeviceRestartNowRequestMessageBody(),
        };
        return msg;
    }
}