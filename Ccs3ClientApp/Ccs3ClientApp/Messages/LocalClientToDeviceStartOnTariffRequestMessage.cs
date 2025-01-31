using Ccs3ClientApp.Messages.LocalClient.Declarations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ccs3ClientApp.Messages;

public class LocalClientToDeviceStartOnPrepaidTariffRequestMessageBody {
    public int TariffId { get; set; }
    public string PasswordHash { get; set; }
}

public class LocalClientToDeviceStartOnPrepaidTariffRequestMessage : LocalClientToDeviceRequestMessage<LocalClientToDeviceStartOnPrepaidTariffRequestMessageBody> {
}

public static class LocalClientToDeviceStartOnPrepaidTariffRequestMessageHelper {
    public static LocalClientToDeviceStartOnPrepaidTariffRequestMessage CreateMessage() {
        var msg = new LocalClientToDeviceStartOnPrepaidTariffRequestMessage {
            Header = new LocalClientToDeviceRequestMessageHeader {
                Type = LocalClientToDeviceRequestMessageType.StartOnPrepaidTariff,
            },
            Body = new LocalClientToDeviceStartOnPrepaidTariffRequestMessageBody(),
        };
        return msg;
    }
}