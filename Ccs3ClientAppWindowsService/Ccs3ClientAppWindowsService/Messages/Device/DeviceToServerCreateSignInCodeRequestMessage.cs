using Ccs3ClientAppWindowsService.Messages.Device.Declarations;

namespace Ccs3ClientAppWindowsService.Messages.Device;

public class DeviceToServerCreateSignInCodeRequestMessageBody {
}

public class DeviceToServerCreateSignInCodeRequestMessage : DeviceToServerRequestMessage<DeviceToServerCreateSignInCodeRequestMessageBody> {
}

public static class DeviceToServerCreateSignInCodeRequestMessageHelper {
    public static DeviceToServerCreateSignInCodeRequestMessage CreateMessage() {
        var msg = new DeviceToServerCreateSignInCodeRequestMessage {
            Header = new DeviceToServerRequestMessageHeader {
                Type = DeviceToServerRequestMessageType.CreateSignInCode,
            },
            Body = new DeviceToServerCreateSignInCodeRequestMessageBody(),
        };
        return msg;
    }
}