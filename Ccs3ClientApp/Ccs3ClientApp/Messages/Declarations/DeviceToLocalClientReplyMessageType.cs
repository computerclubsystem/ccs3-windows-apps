namespace Ccs3ClientApp.Messages.LocalClient.Declarations;

public static class DeviceToLocalClientReplyMessageType {
    public const string Ping = "ping-reply";
    public const string StartOnPrepaidTariff = "start-on-prepaid-tariff-reply";
    public const string EndDeviceSessionByCustomer = "end-device-session-by-customer-reply";
    public const string ChangePrepaidTariffPasswordByCustomer = "change-prepaid-tariff-password-by-customer-reply";
    public const string CreateSignInCode = "create-sign-in-code-reply";
}
