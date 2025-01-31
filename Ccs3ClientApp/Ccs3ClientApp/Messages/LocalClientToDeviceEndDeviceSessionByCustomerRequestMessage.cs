using Ccs3ClientApp.Messages.LocalClient.Declarations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ccs3ClientApp.Messages;

public class LocalClientToDeviceEndDeviceSessionByCustomerRequestMessageBody {
}

public class LocalClientToDeviceEndDeviceSessionByCustomerRequestMessage : LocalClientToDeviceRequestMessage<LocalClientToDeviceEndDeviceSessionByCustomerRequestMessageBody> {
}

public static class LocalClientToDeviceEndDeviceSessionByCustomerRequestMessageHelper {
    public static LocalClientToDeviceEndDeviceSessionByCustomerRequestMessage CreateMessage() {
        var msg = new LocalClientToDeviceEndDeviceSessionByCustomerRequestMessage {
            Header = new LocalClientToDeviceRequestMessageHeader {
                Type = LocalClientToDeviceRequestMessageType.EndDeviceSessionByCustomer,
            },
            Body = new LocalClientToDeviceEndDeviceSessionByCustomerRequestMessageBody(),

        };
        return msg;
    }
}

