using System.Windows.Forms;
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.LockMenuBuilders
{
	public class LockRestrictionsMenuBuilder
	{
		public void Build(ToolStripMenuItem toolStripMenuItemLockAvailabilityRM, ToolStripMenuItem toolStripMenuItemLockAvailability, UserLockHelper userLockHelper)
		{
			//AllUnavailable
			var toolStripMenuItemAllUnavailableAvailability = new ToolStripMenuItem(Resources.AllUnavailable)
				{ Name = "toolStripMenuItemAllUnavailableAvailability" };
			toolStripMenuItemAllUnavailableAvailability.MouseUp += userLockHelper.ToolStripMenuItemAllUnavailableAvailabilityMouseUp;
			toolStripMenuItemLockAvailability.DropDownItems.Add(toolStripMenuItemAllUnavailableAvailability);

			var toolStripMenuItemAllUnAvailableAvailabilityRM = new ToolStripMenuItem(Resources.AllUnavailable)
				{ Name = "toolStripMenuItemAllAvailableAvailabilityRM" };
			toolStripMenuItemAllUnAvailableAvailabilityRM.MouseUp += userLockHelper.ToolStripMenuItemAllUnavailableAvailabilityMouseUp;
			toolStripMenuItemLockAvailabilityRM.DropDownItems.Add(toolStripMenuItemAllUnAvailableAvailabilityRM);

			//AllAvailable
			var toolStripMenuItemAllAvailableAvailability = new ToolStripMenuItem(Resources.AllAvailable)
				{ Name = "toolStripMenuItemAllAvailableAvailability" };
			toolStripMenuItemAllAvailableAvailability.MouseUp += userLockHelper.ToolStripMenuItemAllAvailableAvailabilityMouseUp;
			toolStripMenuItemLockAvailability.DropDownItems.Add(toolStripMenuItemAllAvailableAvailability);

			var toolStripMenuItemAllAvailableAvailabilityRM = new ToolStripMenuItem(Resources.AllAvailable)
				{ Name = "toolStripMenuItemAllAvailableAvailabilityRM" };
			toolStripMenuItemAllAvailableAvailabilityRM.MouseUp += userLockHelper.ToolStripMenuItemAllAvailableAvailabilityMouseUp;
			toolStripMenuItemLockAvailabilityRM.DropDownItems.Add(toolStripMenuItemAllAvailableAvailabilityRM);

			//AllFulFilled
			var toolStripMenuItemAllFulFilledAvailability = new ToolStripMenuItem(Resources.AllFulFilled)
				{Name = "toolStripMenuItemAllFulFilledAvailability"};
			toolStripMenuItemAllFulFilledAvailability.MouseUp += userLockHelper.ToolStripMenuItemAllFulFilledAvailabilityMouseUp;
			toolStripMenuItemLockAvailability.DropDownItems.Add(toolStripMenuItemAllFulFilledAvailability);

			var toolStripMenuItemAllFulFilledAvailabilityRM = new ToolStripMenuItem(Resources.AllFulFilled)
				{ Name = "toolStripMenuItemAllFulFilledAvailabilityRM" };
			toolStripMenuItemAllFulFilledAvailabilityRM.MouseUp += userLockHelper.ToolStripMenuItemAllFulFilledAvailabilityMouseUp;
			toolStripMenuItemLockAvailabilityRM.DropDownItems.Add(toolStripMenuItemAllFulFilledAvailabilityRM);
		}

		//private void buildLockAllRestrictions(ToolStripMenuItem toolStripMenuItemLockRestrictions)
		//{
		//	//toolStripMenuItemLockAllRestrictions.MouseUp += toolStripMenuItemLockAllRestrictionsMouseUp;
		//}
	}
}