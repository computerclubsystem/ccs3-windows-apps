namespace Ccs3ClientAppWindowsService.Messages.LocalClient.Declarations;

public static class DeviceToLocalClientNotificationMessageType {
    public const string Configuration = "configuration-notification";
    public const string CurrentStatus = "current-status-notification";
    public const string SecondsBeforeRestart = "seconds-before-restart-notification";
    public const string SessionWillEndSoon = "session-will-end-soon-notification";
}
