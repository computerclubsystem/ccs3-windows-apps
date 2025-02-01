using Ccs3ClientAppWindowsService.Messages.LocalClient.Declarations;

namespace Ccs3ClientAppWindowsService.Messages.LocalClient;

public class LocalClientToDeviceChangePrepaidTariffPasswordByCustomerRequestMessageBody {
    public string CurrentPasswordHash { get; set; }
    public string NewPasswordHash { get; set; }
}

public class LocalClientToDeviceChangePrepaidTariffPasswordByCustomerRequestMessage : LocalClientToDeviceRequestMessage<LocalClientToDeviceChangePrepaidTariffPasswordByCustomerRequestMessageBody> {
}

