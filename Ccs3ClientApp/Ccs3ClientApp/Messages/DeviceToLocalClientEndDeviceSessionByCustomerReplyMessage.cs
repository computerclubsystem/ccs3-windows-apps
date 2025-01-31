using Ccs3ClientApp.Messages.LocalClient.Declarations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ccs3ClientApp.Messages;

public class DeviceToLocalClientEndDeviceSessionReplyMessageBody {
}

public class DeviceToLocalClientEndDeviceSessionReplyMessage : DeviceToLocalClientReplyMessage<DeviceToLocalClientEndDeviceSessionReplyMessageBody> {
}

public static class DeviceToLocalClientEndDeviceSessionReplyMessageHelper {
    public static DeviceToLocalClientEndDeviceSessionReplyMessage CreateMessage() {
        var msg = new DeviceToLocalClientEndDeviceSessionReplyMessage {
            Header = new DeviceToLocalClientReplyMessageHeader {
                Type = DeviceToLocalClientReplyMessageType.EndDeviceSessionByCustomer,
            },
            Body = new DeviceToLocalClientEndDeviceSessionReplyMessageBody(),

        };
        return msg;
    }
}

