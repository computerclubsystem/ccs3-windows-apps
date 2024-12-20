namespace Ccs3ClientAppWindowsService.Messages {
    public class MessageHeader {
        public string Type { get; set; }
        public string? CorrelationId { get; set; }
        // Source and Target should not be part of device messages
        //public string? Source { get; set; }
        //public string? Target { get; set; }
        public object? RoundTripData { get; set; }
        public bool? Failure { get; set; }
        public MessageError[]? MessageError { get; set; }
    }

    public class MessageError {
        public string Code { get; set; }
        public string? Description { get; set; }
    }
}
