using System.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Common.Controls;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling
{
	public class SchedulerRibbonHelper
	{
		public static void InstantiateEditControl(EditControl editControl)
		{
			editControl.ToolStripButtonNew.Text = Resources.Add;
			editControl.NewSpecialItems.Add(new ToolStripButton { Text = Resources.Activity, Tag = ClipboardItems.Shift });
			editControl.NewSpecialItems.Add(new ToolStripButton { Text = Resources.PersonalActivity, Tag = ClipboardItems.PersonalShift });
			editControl.NewSpecialItems.Add(new ToolStripButton { Text = Resources.Overtime, Tag = ClipboardItems.Overtime });
			editControl.NewSpecialItems.Add(new ToolStripButton { Text = Resources.Absence, Tag = ClipboardItems.Absence });
			editControl.NewSpecialItems.Add(new ToolStripButton { Text = Resources.DayOff, Tag = ClipboardItems.DayOff });
			editControl.DeleteSpecialItems.Add(new ToolStripButton { Text = Resources.DeleteSpecial, Tag = ClipboardItems.Special });
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public static void InstantiateEditControlRestrictions(EditControl editControlRestrictions)
		{
			editControlRestrictions.Visible = false;
			editControlRestrictions.ToolStripButtonNew.Text = Resources.Add;
			editControlRestrictions.NewSpecialItems.Add(new ToolStripButton { Text = Resources.Preference, Tag = ClipboardItems.Preference });
			editControlRestrictions.NewSpecialItems.Add(new ToolStripButton { Text = Resources.StudentAvailability, Tag = ClipboardItems.StudentAvailability });
		}

		public static void InstantiateClipboardControl(ClipboardControl clipboardControl)
		{
			var clipboardControlBuilder = new ClipboardControlBuilder(clipboardControl);
			clipboardControlBuilder.Build();
		}

		public static void InstantiateClipboardControlRestrictions(ClipboardControl clipboardControlRestrictions)
		{
			clipboardControlRestrictions.SetButtonState(ClipboardAction.Cut, false);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "3"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "4"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "5"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "6")]
		public static void EnableRibbonControls(ToolStripEx clipBoard, ToolStripEx edit, ToolStripEx actions, ToolStripEx locks, ToolStripButton filterAgents, ToolStripMenuItem lockMenuItem, ToolStripMenuItem loggedOnUserTimeZone, SchedulingScreen.ZoomLevel level)
		{
			clipBoard.Visible = true;
			edit.Visible = true;
			actions.Visible = true;
			locks.Visible = true;
			filterAgents.Enabled = true;
			lockMenuItem.Enabled = true;
			loggedOnUserTimeZone.Enabled = true;

			if (level != SchedulingScreen.ZoomLevel.Level7)
			{
				clipBoard.Items[0].Visible = true;
				clipBoard.Items[1].Visible = false;

				edit.Items[0].Visible = true;
				edit.Items[1].Visible = false;
			}
			else
			{
				filterAgents.Enabled = false;
				actions.Visible = false;
				locks.Visible = false;

				clipBoard.Items[0].Visible = false;
				clipBoard.Items[1].Visible = true;

				edit.Items[0].Visible = false;
				edit.Items[1].Visible = true;
			}	
		}
	}
}
