using System.Drawing;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Common.Controls;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling
{
	public static class SchedulerRibbonHelper
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public static void InstantiateClipboardControlRestrictions(ClipboardControl clipboardControlRestrictions)
		{
			clipboardControlRestrictions.SetButtonState(ClipboardAction.Cut, false);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "3"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "4"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "5"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "6")]
		public static void EnableRibbonControls(ToolStrip clipboard, ToolStrip edit, Control actions, Control locks, ToolStripItem filterAgents, ToolStripItem lockMenuItem, ToolStripItem loggedOnUserTimeZone, bool restrictionView)
		{
			clipboard.Visible = true;
			edit.Visible = true;
			actions.Visible = true;
			locks.Visible = true;
			filterAgents.Enabled = true;
			lockMenuItem.Enabled = true;
			loggedOnUserTimeZone.Enabled = true;

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
	}
}
