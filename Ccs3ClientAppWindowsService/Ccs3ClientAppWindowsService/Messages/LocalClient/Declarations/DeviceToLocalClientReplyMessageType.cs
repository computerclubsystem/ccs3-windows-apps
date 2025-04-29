namespace Ccs3ClientAppWindowsService.Messages.LocalClient.Declarations;

public static class DeviceToLocalClientReplyMessageType {
    public const string Ping = "ping-reply";
    public const string StartOnPrepaidTariff = "start-on-prepaid-tariff-reply";
    public const string ChangePrepaidTariffPasswordByCustomer = "change-prepaid-tariff-by-customer-reply";
    public const string CreateSignInCode = "create-sign-in-code-reply";
}
