using Microsoft.Practices.CompositeUI.Configuration;
using Microsoft.Practices.CompositeUI.Services;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Common.Library.Services
{
    public class EmptyDependentModuleEnumerator : IModuleEnumerator
    {
        public IModuleInfo[] EnumerateModules()
        {
            return new IModuleInfo[0];
        }
    }
}