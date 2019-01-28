using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.AgentRestrictions;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.SchedulingScreenInternals;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.SingleAgentRestriction;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling
{
	public static class SchedulerRibbonHelper
	{
		[SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
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

		[SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public static void InstantiateEditControlRestrictions(EditControl editControlRestrictions)
		{
			editControlRestrictions.Visible = false;
			editControlRestrictions.ToolStripButtonNew.Text = Resources.Add;
			editControlRestrictions.NewSpecialItems.Add(new ToolStripButton { Text = Resources.AddPreferenceThreeDots, Tag = ClipboardItems.Preference });
			editControlRestrictions.NewSpecialItems.Add(new ToolStripButton { Text = Resources.AddStudentAvailabilityThreeDots, Tag = ClipboardItems.StudentAvailability });
		}

		public static void InstantiateClipboardControl(ClipboardControl clipboardControl)
		{
			var clipboardControlBuilder = new ClipboardControlBuilder(clipboardControl);
			clipboardControlBuilder.Build();
		}

		[SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public static void InstantiateClipboardControlRestrictions(ClipboardControl clipboardControlRestrictions)
		{
			clipboardControlRestrictions.SetButtonState(ClipboardAction.Cut, false);
		}

		[SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2"), SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "3"), SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "4"), SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "5"), SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "6")]
		public static void EnableRibbonControls(ToolStrip clipboard, ToolStrip edit, Control actions, Control locks, ToolStripItem filterAgents, ToolStripItem lockMenuItem, bool restrictionView)
		{
			clipboard.Visible = true;
			edit.Visible = true;
			actions.Visible = true;
			locks.Visible = true;
			filterAgents.Enabled = true;
			lockMenuItem.Enabled = true;

			if (!restrictionView)
			{
				setFirstVisible(clipboard);
				setFirstVisible(edit);
			}
			else
			{
				filterAgents.Enabled = false;
				actions.Visible = false;
				locks.Visible = false;

				setSecondVisible(clipboard);
				setSecondVisible(edit);
			}	
		}

		private static void setFirstVisible(ToolStrip toolStrip)
		{
			if (toolStrip.Items.Count < 2)
			{
				toolStrip.Visible = false;
			}
			else
			{
				toolStrip.Items[0].Visible = true;
				toolStrip.Items[1].Visible = false;
			}
		}

		private static void setSecondVisible(ToolStrip toolStrip)
		{
			if (toolStrip.Items.Count < 2)
			{
				toolStrip.Visible = false;
			}
			else
			{
				toolStrip.Items[0].Visible = false;
				toolStrip.Items[1].Visible = true;
			}
		}

		public static void SetShowRibbonTexts(bool showRibbonTexts, ToolStripPanelItem loadOptions, ToolStripPanelItem show, ToolStripPanelItem locks, ToolStripPanelItem actions, ToolStripPanelItem view, EditControl editControl, EditControl editControlRestrictions, ClipboardControl clipboardControl, ClipboardControl clipboardControlRestrictions, ToolStripEx requests)
		{
			//Load Options
			toggleRibbonTexts(showRibbonTexts, loadOptions.Items);
			//Show
			toggleRibbonTexts(showRibbonTexts, show.Items);
			//Locks
			toggleRibbonTexts(showRibbonTexts, locks.Items);
			//Actions
			toggleRibbonTexts(showRibbonTexts, actions.Items);
			//Views
			toggleRibbonTexts(showRibbonTexts, view.Items);
			//Edit
			toggleRibbonTexts(showRibbonTexts, editControl.PanelItem.Items);
			toggleRibbonTexts(showRibbonTexts, editControlRestrictions.PanelItem.Items);
			//Clipboard
			toggleRibbonTexts(showRibbonTexts, clipboardControl.PanelItem.Items);
			toggleRibbonTexts(showRibbonTexts, clipboardControlRestrictions.PanelItem.Items);
			//REQUESTS
			toggleRibbonTexts(showRibbonTexts, requests.Items);

			if (showRibbonTexts)
				formatRibbonControls(loadOptions, show, locks, actions, view, editControl);
		}

		private static void toggleRibbonTexts(bool showRibbonTexts, ToolStripItemCollection items)
		{
			if (showRibbonTexts)
			{
				foreach (ToolStripItem item in items)
				{
					item.Owner.ShowItemToolTips = false;
					item.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
				}
			}
			else
			{
				foreach (ToolStripItem item in items)
				{
					item.Owner.ShowItemToolTips = true;
					item.DisplayStyle = ToolStripItemDisplayStyle.Image;
					item.AutoSize = true;
				}
			}
		}

		private static void formatRibbonControls(ToolStripPanelItem loadOptions, ToolStripPanelItem show, ToolStripPanelItem locks, ToolStripPanelItem actions, ToolStripPanelItem view, EditControl editControl)
		{
			//Load Options
			setAllToolStripItemsToLongestWidth(loadOptions);
			//Show
			setAllToolStripItemsToLongestWidth(show);
			//Locks
			setAllToolStripItemsToLongestWidth(locks);
			//Actions
			setAllToolStripItemsToLongestWidth(actions);
			//Views
			setAllToolStripItemsToLongestWidth(view);
			//Edit
			setAllToolStripItemsToLongestWidth(editControl.PanelItem);
		}

		private static void setAllToolStripItemsToLongestWidth(ToolStripPanelItem panelItem)
		{
			int maxWidth = 0;

			foreach (ToolStripItem item in panelItem.Items)
			{
				if (item.Width > maxWidth)
					maxWidth = item.Width;
			}

			foreach (ToolStripItem item in panelItem.Items)
			{
				item.ImageAlign = ContentAlignment.MiddleLeft;
				item.TextAlign = ContentAlignment.MiddleLeft;
				item.AutoSize = false;
				item.Width = maxWidth;
			}
		}

		public static void EnableSwapButtons(IList<IScheduleDay> selectedSchedules, ScheduleViewBase scheduleView, ToolStripMenuItem swap,
			ToolStripMenuItem swapAndReschedule, ToolStripMenuItem swapRaw, ToolStripSplitButton dropDownButtonSwap, SchedulingScreenPermissionHelper permissionHelper,
			bool teamLeaderMode, IList<IEntity> selectedEntitiesFromTreeView, GridControl grid )
		{
			if (scheduleView == null) return;
			swap.Enabled = false;
			swapAndReschedule.Enabled = false;
			swapRaw.Enabled = false;
			dropDownButtonSwap.Enabled = false;
			var gridRangeInfoList = GridHelper.GetGridSelectedRanges(grid, true);

			if (gridRangeInfoList.Count == 2 && GridHelper.IsRangesSameSize(gridRangeInfoList[0], gridRangeInfoList[1]))
			{
				if (!GridHelper.SelectionContainsOnlyHeadersAndScheduleDays(grid, gridRangeInfoList)) return;
				dropDownButtonSwap.Enabled = true;
				swapRaw.Enabled = true;
			}
			if (selectedSchedules.Count <= 1 || scheduleView.AllSelectedPersons(selectedSchedules).Count() != 2)
				return;

			dropDownButtonSwap.Enabled = true;
			swap.Enabled = true;
			var automaticScheduleFunction = DefinedRaptorApplicationFunctionPaths.AutomaticScheduling;

			swapAndReschedule.Enabled = permissionHelper.HasFunctionPermissionForTeams(selectedEntitiesFromTreeView.OfType<ITeam>(), automaticScheduleFunction);
			if (teamLeaderMode)
				swapAndReschedule.Enabled = false;

			if (!swapAndReschedule.Enabled) return;
			var schedulesWithinValidPeriod = ScheduleHelper.SchedulesWithinValidSchedulePeriod(selectedSchedules);
			swapAndReschedule.Enabled = schedulesWithinValidPeriod.Count == selectedSchedules.Count;
		}

		public static void EnableScheduleButton(
			ToolStripDropDownItem schedulerToolStripButton, 
			ScheduleViewBase scheduleView, 
			bool teamLeaderMode)
		{
			bool enabled = 
				isEnabledScheduleButton(scheduleView, teamLeaderMode);

			schedulerToolStripButton.Enabled = enabled;
			enableSubItems(schedulerToolStripButton, enabled);
		}

		private static bool isEnabledScheduleButton(
			ScheduleViewBase scheduleView,
			bool teamLeaderMode)
		{
			if (teamLeaderMode)
				return false;

			if (!PrincipalAuthorization.Current_DONTUSE().IsPermitted(DefinedRaptorApplicationFunctionPaths.AutomaticScheduling))
				return false;

			if (scheduleView.ViewGrid.Selections.Count != 1)
				return false;

			if (scheduleView is AgentRestrictionsDetailView)
				return false;

			return true;
		}

		private static void enableSubItems(ToolStripDropDownItem item, bool enable)
		{
			foreach (var subItem in item.DropDownItems)
			{
				var control = subItem as ToolStripItem;
				if(control != null)
					control.Enabled = enable;
			}
		}

	}
}
