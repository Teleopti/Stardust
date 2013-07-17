using System.Collections.Generic;
using Microsoft.Practices.CompositeUI;
using Microsoft.Practices.CompositeUI.SmartParts;
using Teleopti.Ccc.SmartClientPortal.Shell.Common.Services;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Common.Library.Services
{
    public class WorkspaceLocatorService : IWorkspaceLocatorService
    {
        public IWorkspace FindContainingWorkspace(WorkItem workItem, object smartPart)
        {
            while (workItem != null)
            {
                foreach (KeyValuePair<string, IWorkspace> namedWorkspace in workItem.Workspaces)
                {
                    if (namedWorkspace.Value.SmartParts.Contains(smartPart))
                        return namedWorkspace.Value;
                }
                workItem = workItem.Parent;
            }
            return null;
        }
    }
}