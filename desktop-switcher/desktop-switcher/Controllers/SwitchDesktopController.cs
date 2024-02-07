using desktop_switcher.Models;
using Microsoft.AspNetCore.Mvc;
using SwitchToDefaultDesktop;

namespace desktop_switcher.Controllers {
    [ApiController]
    [Route("switch-desktop")]
    public class SwitchDesktopController(
        ILogger<SwitchDesktopController> logger,
        DesktopManager desktopManager
    ) : ControllerBase {
        private const string switchingResultMessage = "Switching result {0}";

        [HttpPut]
        public Task<SwitchDesktopResponse> SwitchDesktop(SwitchDesktopRequest req) {
            bool switchResult;
            logger.LogDebug("Switching to desktop '{0}'", req.DesktopType);
            if (req.DesktopType == DesktopType.Default) {
                switchResult = desktopManager.SwitchToDefaultDesktop();
                logger.LogDebug(switchingResultMessage, switchResult);
            } else if (req.DesktopType == DesktopType.Secured) {
                switchResult = desktopManager.SwitchToSecureDesktop();
                logger.LogDebug(switchingResultMessage, switchResult);
#if CCS3_RESTORE_DESKTOP
                Task.Run(async () => {
                    await Task.Delay(TimeSpan.FromSeconds(3));
                    desktopManager.SwitchToDefaultDesktop();
                });
#endif
            } else {
                switchResult = false;
            }

            SwitchDesktopResponse res = new() { Success = switchResult };
            return Task.FromResult(res);
        }
    }
}
