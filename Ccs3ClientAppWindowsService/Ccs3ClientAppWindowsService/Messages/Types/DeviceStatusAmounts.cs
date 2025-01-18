namespace Ccs3ClientAppWindowsService.Messages.Types;

public class DeviceStatusAmounts {
    public decimal? TotalSum { get; set; }
    public long? TotalTime { get; set; }
    public long? StartedAt { get; set; }
    public long? StoppedAt { get; set; }
    public long? ExpectedEndAt { get; set; }
    public long? RemainingSeconds { get; set; }
}
