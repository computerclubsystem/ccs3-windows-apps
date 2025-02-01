using Ccs3ClientAppWindowsService.Messages.Device.Declarations;

namespace Ccs3ClientAppWindowsService.Messages.Device;

public class DeviceToServerChangePrepaidTariffPasswordByCustomerRequestMessageBody {
    public string CurrentPasswordHash { get; set; }
    public string NewPasswordHash { get; set; }
}

public class DeviceToServerChangePrepaidTariffPasswordByCustomerRequestMessage : DeviceToServerRequestMessage<DeviceToServerChangePrepaidTariffPasswordByCustomerRequestMessageBody> {
}

public static class DeviceToServerChangePrepaidTariffPasswordByCustomerRequestMessageHelper {
    public static DeviceToServerChangePrepaidTariffPasswordByCustomerRequestMessage CreateMessage() {
        var msg = new DeviceToServerChangePrepaidTariffPasswordByCustomerRequestMessage {
            Header = new DeviceToServerRequestMessageHeader {
                Type = DeviceToServerRequestMessageType.ChangePrepaidTariffPasswordByCustomer,
            },
            Body = new DeviceToServerChangePrepaidTariffPasswordByCustomerRequestMessageBody(),
        };
        return msg;
    }
}