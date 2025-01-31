using Ccs3ClientAppWindowsService.Messages.Device.Declarations;

namespace Ccs3ClientAppWindowsService.Messages.Device;

public class DeviceToServerStartOnPrepaidTariffRequestMessageBody {
    public int TariffId { get; set; }
    public string PasswordHash { get; set; }
}

public class DeviceToServerStartOnPrepaidTariffRequestMessage : DeviceToServerRequestMessage<DeviceToServerStartOnPrepaidTariffRequestMessageBody> {
}

public static class DeviceToServerStartOnPrepaidTariffRequestMessageHelper {
    public static DeviceToServerStartOnPrepaidTariffRequestMessage CreateMessage() {
        var msg = new DeviceToServerStartOnPrepaidTariffRequestMessage {
            Header = new DeviceToServerRequestMessageHeader {
                Type = DeviceToServerRequestMessageType.StartOnPrepaidTariff,
            },
            Body = new DeviceToServerStartOnPrepaidTariffRequestMessageBody(),
        };
        return msg;
    }
}