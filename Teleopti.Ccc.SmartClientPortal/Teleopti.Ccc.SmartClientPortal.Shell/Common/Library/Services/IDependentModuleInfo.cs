using System.Collections.Generic;
using Microsoft.Practices.CompositeUI.Configuration;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Common.Library.Services
{
    public interface IDependentModuleInfo : IModuleInfo
    {
        IList<string> Dependencies { get; }
        string Name { get; }
    }
}