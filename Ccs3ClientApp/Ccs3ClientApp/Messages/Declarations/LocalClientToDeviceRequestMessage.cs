﻿namespace Ccs3ClientApp.Messages.LocalClient.Declarations;

public class LocalClientToDeviceRequestMessage<TBody> {
    public LocalClientToDeviceRequestMessageHeader Header { get; set; }
    public TBody Body { get; set; }
}
