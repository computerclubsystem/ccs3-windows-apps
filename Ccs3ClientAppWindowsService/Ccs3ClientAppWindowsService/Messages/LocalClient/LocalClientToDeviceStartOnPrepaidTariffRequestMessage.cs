using Ccs3ClientAppWindowsService.Messages.LocalClient.Declarations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ccs3ClientAppWindowsService.Messages.LocalClient;

public class LocalClientToDeviceStartOnPrepaidTariffRequestMessageBody {
    public int TariffId { get; set; }
    public string PasswordHash { get; set; }
}

public class LocalClientToDeviceStartOnPrepaidTariffRequestMessage : LocalClientToDeviceRequestMessage<LocalClientToDeviceStartOnPrepaidTariffRequestMessageBody> {
}
