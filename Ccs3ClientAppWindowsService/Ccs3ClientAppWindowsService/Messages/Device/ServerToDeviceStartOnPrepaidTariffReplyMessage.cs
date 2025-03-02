using Ccs3ClientAppWindowsService.Messages.Device.Declarations;

namespace Ccs3ClientAppWindowsService.Messages.Device;

public class ServerToDeviceStartOnPrepaidTariffReplyMessageBody {
    public bool? AlreadyInUse { get; set; }
    public bool? NotAllowed { get; set; }
    public bool? PasswordDoesNotMatch { get; set; }
    public bool? NoRemainingTime { get; set; }
    public bool? NotAvailableForThisDeviceGroup { get; set; }
    public int? RemainingSeconds { get; set; }
    public bool? Success { get; set; }
}


public class ServerToDeviceStartOnPrepaidTariffReplyMessage : ServerToDeviceReplyMessage<ServerToDeviceStartOnPrepaidTariffReplyMessageBody> {
}
