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

		public void BuildLockRotation(ToolStripMenuItem toolStripMenuItemLockRotations, ToolStripMenuItem toolStripMenuItemLockRotationsRM, UserLockHelper userLockHelper)
		{
			//All, toolStripMenuItemAllRotations, toolStripMenuItemAllRotationsRM, toolStripMenuItemAllRotationsMouseUp
			var toolStripMenuItemAllRotations = new ToolStripMenuItem(Resources.All)
				{ Name = "toolStripMenuItemAllRotations" };
			toolStripMenuItemAllRotations.MouseUp += userLockHelper.ToolStripMenuItemAllRotationsMouseUp;
			toolStripMenuItemLockRotations.DropDownItems.Add(toolStripMenuItemAllRotations);

			var toolStripMenuItemAllRotationsRM = new ToolStripMenuItem(Resources.All)
				{ Name = "toolStripMenuItemAllRotationsRM" };
			toolStripMenuItemAllRotationsRM.MouseUp += userLockHelper.ToolStripMenuItemAllRotationsMouseUp;
			toolStripMenuItemLockRotationsRM.DropDownItems.Add(toolStripMenuItemAllRotationsRM);

			//AllDaysOff, toolStripMenuItemAllDaysOffRotations, toolStripMenuItemAllDaysOffRotationsRM, toolStripMenuItemAllDaysOffRotationsMouseUp
			var toolStripMenuItemAllDaysOffRotations = new ToolStripMenuItem(Resources.AllDaysOff)
				{ Name = "toolStripMenuItemAllDaysOffRotations" };
			toolStripMenuItemAllDaysOffRotations.MouseUp += userLockHelper.ToolStripMenuItemAllDaysOffRotationsMouseUp;
			toolStripMenuItemLockRotations.DropDownItems.Add(toolStripMenuItemAllDaysOffRotations);

			var toolStripMenuItemAllDaysOffRotationsRM = new ToolStripMenuItem(Resources.AllDaysOff)
				{ Name = "toolStripMenuItemAllDaysOffRotationsRM" };
			toolStripMenuItemAllDaysOffRotationsRM.MouseUp += userLockHelper.ToolStripMenuItemAllDaysOffRotationsMouseUp;
			toolStripMenuItemLockRotationsRM.DropDownItems.Add(toolStripMenuItemAllDaysOffRotationsRM);

			//AllShifts, toolStripMenuItemAllShiftRotations, toolStripMenuItemAllShiftRotationsRM, toolStripMenuItemAllShiftsRotationsMouseUp
			var toolStripMenuItemAllShiftRotations = new ToolStripMenuItem(Resources.AllShifts)
				{ Name = "toolStripMenuItemAllShiftRotations" };
			toolStripMenuItemAllShiftRotations.MouseUp += userLockHelper.ToolStripMenuItemAllShiftsRotationsMouseUp;
			toolStripMenuItemLockRotations.DropDownItems.Add(toolStripMenuItemAllShiftRotations);

			var toolStripMenuItemAllShiftRotationsRM = new ToolStripMenuItem(Resources.AllShifts)
				{ Name = "toolStripMenuItemAllShiftRotationsRM" };
			toolStripMenuItemAllShiftRotationsRM.MouseUp += userLockHelper.ToolStripMenuItemAllShiftsRotationsMouseUp;
			toolStripMenuItemLockRotationsRM.DropDownItems.Add(toolStripMenuItemAllShiftRotationsRM);

			//AllFulFilled, toolStripMenuItemAllFulFilledRotations, toolStripMenuItemAllFulFilledRotationsRM, toolStripMenuItemAllFulFilledRotationsMouseUp
			var toolStripMenuItemAllFulFilledRotations = new ToolStripMenuItem(Resources.AllFulFilled)
				{ Name = "toolStripMenuItemAllFulFilledRotations" };
			toolStripMenuItemAllFulFilledRotations.MouseUp += userLockHelper.ToolStripMenuItemAllFulFilledRotationsMouseUp;
			toolStripMenuItemLockRotations.DropDownItems.Add(toolStripMenuItemAllFulFilledRotations);

			var toolStripMenuItemAllFulFilledRotationsRM = new ToolStripMenuItem(Resources.AllFulFilled)
				{ Name = "toolStripMenuItemAllFulFilledRotationsRM" };
			toolStripMenuItemAllFulFilledRotationsRM.MouseUp += userLockHelper.ToolStripMenuItemAllFulFilledRotationsMouseUp;
			toolStripMenuItemLockRotationsRM.DropDownItems.Add(toolStripMenuItemAllFulFilledRotationsRM);

			//AllFulFilledDaysOff, toolStripMenuItemAllFulFilledDaysOffRotations, toolStripMenuItemAllFulFilledDaysOffRotationsRM, toolStripMenuItemAllFulFilledDaysOffRotationsMouseUp
			var toolStripMenuItemAllFulFilledDaysOffRotations = new ToolStripMenuItem(Resources.AllFulFilledDaysOff)
				{ Name = "toolStripMenuItemAllFulFilledDaysOffRotations" };
			toolStripMenuItemAllFulFilledDaysOffRotations.MouseUp += userLockHelper.ToolStripMenuItemAllFulFilledDaysOffRotationsMouseUp;
			toolStripMenuItemLockRotations.DropDownItems.Add(toolStripMenuItemAllFulFilledDaysOffRotations);

			var toolStripMenuItemAllFulFilledDaysOffRotationsRM = new ToolStripMenuItem(Resources.AllFulFilledDaysOff)
				{ Name = "toolStripMenuItemAllFulFilledDaysOffRotationsRM" };
			toolStripMenuItemAllFulFilledDaysOffRotationsRM.MouseUp += userLockHelper.ToolStripMenuItemAllFulFilledDaysOffRotationsMouseUp;
			toolStripMenuItemLockRotationsRM.DropDownItems.Add(toolStripMenuItemAllFulFilledDaysOffRotationsRM);

			//AllFulFilledShifts, toolStripMenuItemAllFulFilledShiftsRotations, toolStripMenuItemAllFulFilledShiftsRotationsRM, toolStripMenuItemAllFulFilledShiftsRotationsMouseUp
			var toolStripMenuItemAllFulFilledShiftsRotations = new ToolStripMenuItem(Resources.AllFulFilledShifts)
				{ Name = "toolStripMenuItemAllFulFilledShiftsRotations" };
			toolStripMenuItemAllFulFilledShiftsRotations.MouseUp += userLockHelper.ToolStripMenuItemAllFulFilledShiftsRotationsMouseUp;
			toolStripMenuItemLockRotations.DropDownItems.Add(toolStripMenuItemAllFulFilledShiftsRotations);

			var toolStripMenuItemAllFulFilledShiftsRotationsRM = new ToolStripMenuItem(Resources.AllFulFilledShifts)
				{ Name = "toolStripMenuItemAllFulFilledShiftsRotationsRM" };
			toolStripMenuItemAllFulFilledShiftsRotationsRM.MouseUp += userLockHelper.ToolStripMenuItemAllFulFilledShiftsRotationsMouseUp;
			toolStripMenuItemLockRotationsRM.DropDownItems.Add(toolStripMenuItemAllFulFilledShiftsRotationsRM);
		}
	}
}