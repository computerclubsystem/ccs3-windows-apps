using Ccs3ClientAppWindowsService.Messages.Device.Declarations;

namespace Ccs3ClientAppWindowsService.Messages.Device;

public class DeviceToServerEndDeviceSessionRequestMessageBody {
}

public class DeviceToServerEndDeviceSessionRequestMessage : DeviceToServerRequestMessage<DeviceToServerEndDeviceSessionRequestMessageBody> {
}

public static class DeviceToServerEndDeviceSessionRequestMessageHelper {
    public static DeviceToServerEndDeviceSessionRequestMessage CreateMessage() {
        var msg = new DeviceToServerEndDeviceSessionRequestMessage {
            Header = new DeviceToServerRequestMessageHeader {
                Type = DeviceToServerRequestMessageType.EndDeviceSessionByCustomer,
            },
            Body = new DeviceToServerEndDeviceSessionRequestMessageBody(),
        };
        return msg;
    }
}