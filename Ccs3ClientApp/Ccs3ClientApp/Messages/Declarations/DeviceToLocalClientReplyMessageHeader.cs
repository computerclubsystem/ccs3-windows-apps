using Ccs3ClientApp.Messages.Declarations;

namespace Ccs3ClientAppWindowsService.Messages.LocalClient.Declarations;

public class DeviceToLocalClientReplyMessageHeader {
    public string Type { get; set; }
    public bool? Failure { get; set; }
    public MessageError[]? MessageErrors { get; set; }
}
