namespace Ccs3HttpToUdpProxyWindowsService.Entities;

public class PacketData {
    public string DestinationIpAddress { get; set; }
    public int DestinationPort { get; set; }
    public string PacketHexString { get; set; }
}
