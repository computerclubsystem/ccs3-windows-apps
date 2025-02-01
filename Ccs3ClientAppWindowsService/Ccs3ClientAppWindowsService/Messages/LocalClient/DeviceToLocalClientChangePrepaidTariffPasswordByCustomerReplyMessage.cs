using Ccs3ClientAppWindowsService.Messages.LocalClient.Declarations;

namespace Ccs3ClientAppWindowsService.Messages.LocalClient;

public class DeviceToLocalClientChangePrepaidTariffPasswordByCustomerReplyMessageBody {
}


public class DeviceToLocalClientChangePrepaidTariffPasswordByCustomerReplyMessage : DeviceToLocalClientReplyMessage<DeviceToLocalClientChangePrepaidTariffPasswordByCustomerReplyMessageBody> {
}

public static class DeviceToLocalClientChangePrepaidTariffPasswordByCustomerReplyMessageHelper {
    public static DeviceToLocalClientChangePrepaidTariffPasswordByCustomerReplyMessage CreateMessage() {
        var msg = new DeviceToLocalClientChangePrepaidTariffPasswordByCustomerReplyMessage {
            Header = new DeviceToLocalClientReplyMessageHeader {
                Type = DeviceToLocalClientReplyMessageType.ChangePrepaidTariffPasswordByCustomer,
            },
            Body = new DeviceToLocalClientChangePrepaidTariffPasswordByCustomerReplyMessageBody(),
        };
        return msg;
    }
}