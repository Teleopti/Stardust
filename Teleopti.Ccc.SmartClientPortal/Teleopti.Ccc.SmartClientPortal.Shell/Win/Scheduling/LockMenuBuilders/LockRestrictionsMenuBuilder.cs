using System.Windows.Forms;
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.LockMenuBuilders
{
	public class LockRestrictionsMenuBuilder
	{
		public void BuildLockAvailability(ToolStripMenuItem toolStripMenuItemLockAvailabilityRM, ToolStripMenuItem toolStripMenuItemLockAvailability, UserLockHelper userLockHelper)
		{
			//AllUnavailable
			var toolStripMenuItemAllUnavailableAvailability = new ToolStripMenuItem(Resources.AllUnavailable)
				{ Name = "toolStripMenuItemAllUnavailableAvailability" };
			toolStripMenuItemAllUnavailableAvailability.MouseUp += userLockHelper.ToolStripMenuItemAllUnavailableAvailabilityMouseUp;
			toolStripMenuItemLockAvailability.DropDownItems.Add(toolStripMenuItemAllUnavailableAvailability);

			var toolStripMenuItemAllUnAvailableAvailabilityRM = new ToolStripMenuItem(Resources.AllUnavailable)
				{ Name = "toolStripMenuItemAllUnAvailableAvailabilityRM" };
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

		public void BuildLockStudentAvailability(ToolStripMenuItem toolStripMenuItemLockStudentAvailability, ToolStripMenuItem toolStripMenuItemLockStudentAvailabilityRM, UserLockHelper userLockHelper)
		{
			//AllUnavailable
			var toolStripMenuItemAllUnavailableStudentAvailability = new ToolStripMenuItem(Resources.AllUnavailable)
			{ Name = "toolStripMenuItemAllUnavailableStudentAvailability" };
			toolStripMenuItemAllUnavailableStudentAvailability.MouseUp += userLockHelper.ToolStripMenuItemAllUnavailableStudentAvailabilityMouseUp;
			toolStripMenuItemLockStudentAvailability.DropDownItems.Add(toolStripMenuItemAllUnavailableStudentAvailability);

			var toolStripMenuItemAllUnavailableStudentAvailabilityRM = new ToolStripMenuItem(Resources.AllUnavailable)
			{ Name = "toolStripMenuItemAllUnavailableStudentAvailabilityRM" };
			toolStripMenuItemAllUnavailableStudentAvailabilityRM.MouseUp += userLockHelper.ToolStripMenuItemAllUnavailableStudentAvailabilityMouseUp;
			toolStripMenuItemLockStudentAvailabilityRM.DropDownItems.Add(toolStripMenuItemAllUnavailableStudentAvailabilityRM);

			//AllAvailable
			var toolStripMenuItemAllAvailableStudentAvailability = new ToolStripMenuItem(Resources.AllAvailable)
			{ Name = "toolStripMenuItemAllAvailableStudentAvailability" };
			toolStripMenuItemAllAvailableStudentAvailability.MouseUp += userLockHelper.ToolStripMenuItemAllAvailableStudentAvailabilityMouseUp;
			toolStripMenuItemLockStudentAvailability.DropDownItems.Add(toolStripMenuItemAllAvailableStudentAvailability);

			var toolStripMenuItemAllAvailableStudentAvailabilityRM = new ToolStripMenuItem(Resources.AllAvailable)
			{ Name = "toolStripMenuItemAllAvailableStudentAvailabilityRM" };
			toolStripMenuItemAllAvailableStudentAvailabilityRM.MouseUp += userLockHelper.ToolStripMenuItemAllAvailableStudentAvailabilityMouseUp;
			toolStripMenuItemLockStudentAvailabilityRM.DropDownItems.Add(toolStripMenuItemAllAvailableStudentAvailabilityRM);

			//AllFulFilled
			var toolStripMenuItemAllFulFilledStudentAvailability = new ToolStripMenuItem(Resources.AllFulFilled)
			{ Name = "toolStripMenuItemAllFulFilledStudentAvailability" };
			toolStripMenuItemAllFulFilledStudentAvailability.MouseUp += userLockHelper.ToolStripMenuItemAllFulFilledStudentAvailabilityMouseUp;
			toolStripMenuItemLockStudentAvailability.DropDownItems.Add(toolStripMenuItemAllFulFilledStudentAvailability);

			var toolStripMenuItemAllFulFilledStudentAvailabilityRM = new ToolStripMenuItem(Resources.AllFulFilled)
			{ Name = "toolStripMenuItemAllFulFilledStudentAvailabilityRM" };
			toolStripMenuItemAllFulFilledStudentAvailabilityRM.MouseUp += userLockHelper.ToolStripMenuItemAllFulFilledStudentAvailabilityMouseUp;
			toolStripMenuItemLockStudentAvailabilityRM.DropDownItems.Add(toolStripMenuItemAllFulFilledStudentAvailabilityRM);
		}
	}
}