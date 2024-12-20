namespace Ccs3ClientAppWindowsService.Messages; 

public class Message<TBody> {
    public MessageHeader Header { get; set; }
    public TBody Body { get; set; }
}
