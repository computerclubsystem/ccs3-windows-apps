namespace Ccs3ClientApp.Messages.Declarations;

public class PartialMessage {
    public PartialMessageHeader Header { get; set; }
    public object? Body { get; set; }
}
