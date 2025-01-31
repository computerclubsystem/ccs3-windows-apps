namespace Ccs3ClientAppWindowsService.Messages;

public class PartialMessage {
    public PartialMessageHeader Header { get; set; }
    public object? Body { get; set; }
}
