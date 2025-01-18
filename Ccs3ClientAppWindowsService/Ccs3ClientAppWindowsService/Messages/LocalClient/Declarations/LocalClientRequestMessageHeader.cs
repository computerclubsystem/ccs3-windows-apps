namespace Ccs3ClientAppWindowsService.Messages.LocalClient.Declarations;

public class LocalClientRequestMessageHeader {
    public string Type { get; set; }
    public string? CorrelationId { get; set; }
    // Source and Target should not be part of device messages
    //public string? Source { get; set; }
    //public string? Target { get; set; }
    public object? RoundTripData { get; set; }
}
