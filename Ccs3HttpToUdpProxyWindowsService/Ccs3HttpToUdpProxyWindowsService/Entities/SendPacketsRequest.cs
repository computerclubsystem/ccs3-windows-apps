namespace Ccs3HttpToUdpProxyWindowsService.Entities;

public class SendPacketsRequest {
    public PacketData[] PacketsData { get; set; }
    public int DelayBetweenPacketsMilliseconds { get; set; }
}
