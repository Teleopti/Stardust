using Microsoft.Practices.CompositeUI;
using Microsoft.Practices.CompositeUI.SmartParts;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Common.Services
{
    public interface IWorkspaceLocatorService
    {
        IWorkspace FindContainingWorkspace(WorkItem workItem, object smartPart);
    }
}
