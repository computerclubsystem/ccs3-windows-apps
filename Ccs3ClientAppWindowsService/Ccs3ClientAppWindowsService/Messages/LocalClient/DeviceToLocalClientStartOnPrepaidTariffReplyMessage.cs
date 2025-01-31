using Ccs3ClientAppWindowsService.Messages.LocalClient.Declarations;

namespace Ccs3ClientAppWindowsService.Messages.LocalClient;

public class DeviceToLocalClientStartOnPrepaidTariffReplyMessageBody {
    public bool? AlreadyInUse { get; set; }
    public bool? NotAllowed { get; set; }
    public bool? PasswordDoesNotMatch { get; set; }
    public int? RemainingSeconds { get; set; }
    public bool? Success { get; set; }
}

public class DeviceToLocalClientStartOnPrepaidTariffReplyMessage : DeviceToLocalClientReplyMessage<DeviceToLocalClientStartOnPrepaidTariffReplyMessageBody> {
}

public static class DeviceToLocalClientStartOnPrepaidTariffReplyMessageHelper {
    public static DeviceToLocalClientStartOnPrepaidTariffReplyMessage CreateMessage() {
        var msg = new DeviceToLocalClientStartOnPrepaidTariffReplyMessage {
            Header = new DeviceToLocalClientReplyMessageHeader {
                Type = DeviceToLocalClientReplyMessageType.StartOnPrepaidTariff,
            },
            Body = new DeviceToLocalClientStartOnPrepaidTariffReplyMessageBody(),
        };
        return msg;
    }
}