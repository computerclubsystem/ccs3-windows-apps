using Ccs3ClientApp.Messages.LocalClient.Declarations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ccs3ClientApp.Messages;

public class LocalClientToDevicePingNotificationMessageBody {
}


public class LocalClientToDevicePingNotificationMessage : LocalClientToDeviceNotificationMessage<LocalClientToDevicePingNotificationMessageBody> {
}

public class LocalClientToDevicePingNotificationMessageHelper {
    public static LocalClientToDevicePingNotificationMessage CreateMessage() {
        var msg = new LocalClientToDevicePingNotificationMessage {
            Header = new LocalClientToDeviceNotificationMessageHeader {
                Type = LocalClientToDeviceNotificationMessageType.Ping,
            },
            Body = new LocalClientToDevicePingNotificationMessageBody(),
        };
        return msg;
    }
}
