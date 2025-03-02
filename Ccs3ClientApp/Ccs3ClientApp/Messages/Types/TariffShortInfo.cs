using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ccs3ClientApp.Messages.Types;
public class TariffShortInfo {
    public int Id { get; set; }
    public string Name { get; set; }
    public int? Duration { get; set; }
}
