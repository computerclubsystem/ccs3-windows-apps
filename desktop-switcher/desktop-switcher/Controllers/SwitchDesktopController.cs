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
        [HttpPut]
        public Task<SwitchDesktopResponse> SwitchDesktop(SwitchDesktopRequest req) {
            logger.LogDebug("Switching to desktop '{0}'", req.DesktopType);
            if (req.DesktopType == DesktopType.Default) {
                bool switchResult = desktopManager.SwitchToDefaultDesktop();
                logger.LogTrace("Switching result {0}", switchResult);
            } else if (req.DesktopType == DesktopType.Secured) {
                bool switchResult = desktopManager.SwitchToSecureDesktop();
                logger.LogTrace("Switching result {0}", switchResult);
#if CCS3_RESTORE_DESKTOP
                Task.Run(async () => {
                    await Task.Delay(TimeSpan.FromSeconds(3));
                    desktopManager.SwitchToDefaultDesktop();
                });
#endif
            } else {

            }

            SwitchDesktopResponse res = new();
            return Task.FromResult(res);
        }
    }
}
