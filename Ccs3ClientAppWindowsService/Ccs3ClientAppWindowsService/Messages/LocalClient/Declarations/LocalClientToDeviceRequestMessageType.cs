﻿namespace Ccs3ClientAppWindowsService.Messages.LocalClient.Declarations;

public static class LocalClientToDeviceRequestMessageType {
    public const string Ping = "ping-request";
    public const string StartOnPrepaidTariff = "start-on-prepaid-tariff-request";
    public const string EndDeviceSessionByCustomer = "end-device-session-by-customer-request";
    public const string ChangePrepaidTariffPasswordByCustomer = "change-prepaid-tariff-password-by-customer-request";
    public const string RestartNow = "restart-now-request";
    public const string CreateSignInCode = "create-sign-in-code-request";
}
