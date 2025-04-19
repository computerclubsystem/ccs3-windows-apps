using Ccs3ClientApp.Messages.LocalClient.Declarations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ccs3ClientApp.Messages;

public class DeviceToLocalClientSessionWillEndSoonNotificationMessageBody {
    public long RemainingSeconds { get; set; }
    public string? NotificationSoundFile { get; set; }
}

public class DeviceToLocalClientSessionWillEndSoonNotificationMessage : DeviceToLocalClientNotificationMessage<DeviceToLocalClientSessionWillEndSoonNotificationMessageBody> {
}