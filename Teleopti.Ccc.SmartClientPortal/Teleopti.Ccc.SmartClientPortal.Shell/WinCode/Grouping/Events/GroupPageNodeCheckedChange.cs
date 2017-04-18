using System;
using Microsoft.Practices.Composite.Presentation.Events;
using Syncfusion.Windows.Forms.Tools;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Grouping.Events
{
    public class GroupPageNodeCheckedChange : CompositePresentationEvent<GroupPageNodeCheckData>
    {
        
    }

	public class GroupPageNodeCheckData
	{
		public Guid AgentId { get; set; }
		public TreeNodeAdv Node{ get; set; }
	}
}