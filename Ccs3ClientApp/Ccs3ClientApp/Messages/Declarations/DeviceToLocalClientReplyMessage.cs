﻿namespace Ccs3ClientApp.Messages.LocalClient.Declarations;

public class DeviceToLocalClientReplyMessage<TBody> {
    public DeviceToLocalClientReplyMessageHeader Header { get; set; }
    public TBody Body { get; set; }
}
