using System;
using System.Windows.Forms;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Scheduling;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling
{
	public class SkillGridContextMenuBuilder
	{
		public void Build(SkillResultViewSetting currentSetting, ContextMenuStrip contextMenuSkillGrid,
			bool useShrinkage, EventHandler skillGridPeriodHandler,
			EventHandler toolStripMenuItemUseShrinkageClickHandler)
		{
			var skillGridMenuItem = new ToolStripMenuItem(Resources.Period)
			{
				Name = "Period",
				Checked = currentSetting.Equals(SkillResultViewSetting.Period),
				Tag = SkillResultViewSetting.Period
			};
			skillGridMenuItem.Click += skillGridPeriodHandler;
			contextMenuSkillGrid.Items.Add(skillGridMenuItem);

			skillGridMenuItem = new ToolStripMenuItem(Resources.Month)
			{
				Name = "Month",
				Checked = currentSetting.Equals(SkillResultViewSetting.Month),
				Tag = SkillResultViewSetting.Month
			};
			skillGridMenuItem.Click += skillGridPeriodHandler;
			contextMenuSkillGrid.Items.Add(skillGridMenuItem);

			skillGridMenuItem = new ToolStripMenuItem(Resources.Week)
			{
				Name = "Week",
				Checked = currentSetting.Equals(SkillResultViewSetting.Week),
				Tag = SkillResultViewSetting.Week
			};
			skillGridMenuItem.Click += skillGridPeriodHandler;
			contextMenuSkillGrid.Items.Add(skillGridMenuItem);

			skillGridMenuItem = new ToolStripMenuItem(Resources.Day)
			{
				Name = "Day",
				Checked = currentSetting.Equals(SkillResultViewSetting.Day),
				Tag = SkillResultViewSetting.Day
			};
			skillGridMenuItem.Click += skillGridPeriodHandler;
			contextMenuSkillGrid.Items.Add(skillGridMenuItem);

			skillGridMenuItem = new ToolStripMenuItem(Resources.Intraday)
			{
				Name = "Intraday",
				Checked = currentSetting.Equals(SkillResultViewSetting.Intraday),
				Tag = SkillResultViewSetting.Intraday
			};
			skillGridMenuItem.Click += skillGridPeriodHandler;
			contextMenuSkillGrid.Items.Add(skillGridMenuItem);

			skillGridMenuItem = new ToolStripMenuItem(Resources.UseShrinkage);
			skillGridMenuItem.Click += toolStripMenuItemUseShrinkageClickHandler;
			skillGridMenuItem.Checked = useShrinkage;
			skillGridMenuItem.Name = "UseShrinkage";
			contextMenuSkillGrid.Items.Add(skillGridMenuItem);

			var skillGridMenuSeparator = new ToolStripSeparator();
			contextMenuSkillGrid.Items.Add(skillGridMenuSeparator);

			skillGridMenuItem = new ToolStripMenuItem(Resources.CreateSkillSummery) {Name = "CreateSkillSummery"};
			contextMenuSkillGrid.Items.Add(skillGridMenuItem);

			skillGridMenuItem = new ToolStripMenuItem(Resources.EditSkillSummery) {Name = "Edit", Enabled = false};
			contextMenuSkillGrid.Items.Add(skillGridMenuItem);

			skillGridMenuItem = new ToolStripMenuItem(Resources.DeleteSkillSummery) {Name = "Delete", Enabled = false};
			contextMenuSkillGrid.Items.Add(skillGridMenuItem);
		}
	}
}