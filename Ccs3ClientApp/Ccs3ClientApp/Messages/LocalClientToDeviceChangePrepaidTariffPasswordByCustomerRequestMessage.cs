using Ccs3ClientApp.Messages.LocalClient.Declarations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ccs3ClientApp.Messages;

public class LocalClientToDeviceChangePrepaidTariffPasswordByCustomerRequestMessageBody {
    public string CurrentPasswordHash { get; set; }
    public string NewPasswordHash { get; set; }
}

public class LocalClientToDeviceChangePrepaidTariffPasswordByCustomerRequestMessage : LocalClientToDeviceRequestMessage<LocalClientToDeviceChangePrepaidTariffPasswordByCustomerRequestMessageBody> {
}

public static class LocalClientToDeviceChangePrepaidTariffPasswordByCustomerRequestMessageHelper {
    public static LocalClientToDeviceChangePrepaidTariffPasswordByCustomerRequestMessage CreateMessage() {
        var msg = new LocalClientToDeviceChangePrepaidTariffPasswordByCustomerRequestMessage {
            Header = new LocalClientToDeviceRequestMessageHeader {
                Type = LocalClientToDeviceRequestMessageType.ChangePrepaidTariffPasswordByCustomer,
            },
            Body = new LocalClientToDeviceChangePrepaidTariffPasswordByCustomerRequestMessageBody(),
        };
        return msg;
    }
}
