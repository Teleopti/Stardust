using System.Collections.Generic;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Win.Scheduling;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling
{
	public class QuickAccessState
	{
		private readonly RibbonControlAdvFixed _ribbonControl;
		private readonly SchedulingScreenSettings _schedulingScreenSettings;

		public QuickAccessState(SchedulingScreenSettings schedulingScreenSettings, RibbonControlAdvFixed ribbonControl)
		{
			_ribbonControl = ribbonControl;
			_schedulingScreenSettings = schedulingScreenSettings;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		public void Load(ToolStripButton toolStripButtonQuickAccessSave, ToolStripSplitButton toolStripSplitButtonQuickAccessUndo, ToolStripButton toolStripButtonQuickAccessRedo, ToolStripButton toolStripButtonQuickAccessCancel, ToolStripButton toolStripButtonShowTexts)
		{
			IList<string> readSettingList = _schedulingScreenSettings.QuickAccessButtons;

			if (readSettingList.Count == 1 && readSettingList[0] == "NotSavedBefore")
			{
				//not saved before, add default

				_ribbonControl.Header.AddQuickItem(new QuickButtonReflectable(toolStripButtonQuickAccessSave));
				_ribbonControl.Header.AddQuickItem(new QuickSplitButtonReflectable(toolStripSplitButtonQuickAccessUndo));
				_ribbonControl.Header.AddQuickItem(new QuickButtonReflectable(toolStripButtonQuickAccessRedo));
				_ribbonControl.Header.AddQuickItem(new QuickButtonReflectable(toolStripButtonQuickAccessCancel));
				_ribbonControl.Header.AddQuickItem(new QuickButtonReflectable(toolStripButtonShowTexts));
			}
			else
			{
				//add the saved ones
				foreach (string name in readSettingList)
				{
					foreach (var control in _ribbonControl.Controls)
					{
						var toolStrip = control as ToolStrip;
						if (toolStrip != null)
							loadQuickAccessItemsFromToolStripEx(toolStrip, name);

						var panel = control as RibbonPanel;
						if (panel != null)
						{
							foreach (var stripControl in panel.Controls)
							{
								toolStrip = stripControl as ToolStrip;
								if (toolStrip != null)
									loadQuickAccessItemsFromToolStripEx(toolStrip, name);
							}
						}
					}
					foreach (ToolStripItem toolstripItem in _ribbonControl.OfficeMenu.MainItems)
					{
						loadStripItem(toolstripItem, name);
					}
				}
			}
		}

		public void Save()
		{
			IList<string> itemList = new List<string>();
			foreach (ToolStripItem tsi in _ribbonControl.Header.QuickItems)
			{
				if (tsi.GetType() == typeof(QuickButtonReflectable))
				{
					var toolStripButton = (QuickButtonReflectable)tsi;
					itemList.Add(toolStripButton.ReflectedButton.Name);
				}
				else if (tsi.GetType() == typeof(QuickDropDownButtonReflectable))
				{
					var toolStripDropDownButton = (QuickDropDownButtonReflectable)tsi;
					itemList.Add(toolStripDropDownButton.ReflectedDropDownButton.Name);
				}
				else if (tsi.GetType() == typeof(QuickSplitButtonReflectable))
				{
					var toolStripSplitDownButton = (QuickSplitButtonReflectable)tsi;
					itemList.Add(toolStripSplitDownButton.ReflectedSplitButton.Name);
				}
			}
			_schedulingScreenSettings.QuickAccessButtons.Clear();
			foreach (string button in itemList)
			{
				_schedulingScreenSettings.QuickAccessButtons.Add(button);
			}
		}

		private void loadQuickAccessItemsFromToolStripEx(ToolStrip toolStrip, string name)
		{
			foreach (ToolStripItem stripItem in toolStrip.Items)
			{
				loadStripItem(stripItem, name);
				// if it's a Panel
				var stripPanelItem = stripItem as ToolStripPanelItem;
				if (stripPanelItem != null)
				{
					foreach (ToolStripItem item in stripPanelItem.Items)
					{
						loadStripItem(item, name);
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		private void loadStripItem(ToolStripItem stripItem, string name)
		{
			var stripButton = stripItem as ToolStripButton;
			if (stripButton != null && stripButton.Name == name)
			{
				_ribbonControl.Header.AddQuickItem(new QuickButtonReflectable(stripButton));
				return;
			}
			var stripSplitButton = stripItem as ToolStripSplitButton;
			if (stripSplitButton != null && stripSplitButton.Name == name)
			{
				_ribbonControl.Header.AddQuickItem(new QuickSplitButtonReflectable(stripSplitButton));
				return;
			}
			var stripDropDown = stripItem as ToolStripDropDownButton;
			if (stripDropDown != null && stripDropDown.Name == name)
			{
				_ribbonControl.Header.AddQuickItem(new QuickDropDownButtonReflectable(stripDropDown));
			}
		}
	}
}
