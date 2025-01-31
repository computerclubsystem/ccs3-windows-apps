namespace Ccs3ClientApp.Messages.Declarations;

public class PartialMessageHeader {
    public string Type { get; set; }
    public string? CorrelationId { get; set; }
    public bool? Failure { get; set; }
    public MessageError[]? MessageErrors { get; set; }
}
