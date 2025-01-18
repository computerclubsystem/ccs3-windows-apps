using Ccs3ClientApp.Messages.LocalClient.Declarations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ccs3ClientApp.Messages.Declarations;

/// <summary>
/// Used for initial deserialization just to see the message type
/// </summary>
public class LocalClientPartialMessageHeader {
    public string Type { get; set; }
}
