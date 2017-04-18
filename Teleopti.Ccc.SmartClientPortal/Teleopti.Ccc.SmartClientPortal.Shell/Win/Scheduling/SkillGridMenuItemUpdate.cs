using System.Windows.Forms;
using Teleopti.Ccc.Win.Scheduling;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling
{
	public class SkillGridMenuItemUpdate
	{
		private readonly ContextMenuStrip _menuStrip;
		private readonly SkillResultViewSetting _setting;
		private readonly ToolStripButton _toolStripButtonChartPeriodView;
		private readonly ToolStripButton _toolStripButtonChartMonthView;
		private readonly ToolStripButton _toolStripButtonChartWeekView;
		private readonly ToolStripButton _toolStripButtonChartDayView;
		private readonly ToolStripButton _toolStripButtonChartIntradayView;


		public SkillGridMenuItemUpdate(ContextMenuStrip menuStrip, 
			SkillResultViewSetting setting, 
			ToolStripButton toolStripButtonChartPeriodView, 
			ToolStripButton toolStripButtonChartMonthView,
			ToolStripButton toolStripButtonChartWeekView,
			ToolStripButton toolStripButtonChartDayView,
			ToolStripButton toolStripButtonChartIntradayView)
		{
			_menuStrip = menuStrip;
			_setting = setting;
			_toolStripButtonChartPeriodView = toolStripButtonChartPeriodView;
			_toolStripButtonChartMonthView = toolStripButtonChartMonthView;
			_toolStripButtonChartWeekView = toolStripButtonChartWeekView;
			_toolStripButtonChartDayView = toolStripButtonChartDayView;
			_toolStripButtonChartIntradayView = toolStripButtonChartIntradayView;
		}

		public void Update()
		{
			switch (_setting)
			{
				case SkillResultViewSetting.Period:
					UpdatePeriodSelected();
					break;
				case SkillResultViewSetting.Month:
					UpdateMonthSelected();
					break;
				case SkillResultViewSetting.Week:
					UpdateWeekSelected();
					break;
				case SkillResultViewSetting.Day:
					UpdateDaySelected();
					break;
				case SkillResultViewSetting.Intraday:
					UpdateIntradaySelected();
					break;
			}	
		}

		private void UpdatePeriodSelected()
		{
			((ToolStripMenuItem)_menuStrip.Items["IntraDay"]).Checked = false;
			((ToolStripMenuItem)_menuStrip.Items["Day"]).Checked = false;
			((ToolStripMenuItem)_menuStrip.Items["Week"]).Checked = false;
			((ToolStripMenuItem)_menuStrip.Items["Month"]).Checked = false;
			((ToolStripMenuItem)_menuStrip.Items["Period"]).Checked = true;

			_toolStripButtonChartPeriodView.Checked = true;
			_toolStripButtonChartMonthView.Checked = false;
			_toolStripButtonChartWeekView.Checked = false;
			_toolStripButtonChartDayView.Checked = false;
			_toolStripButtonChartIntradayView.Checked = false;
		}

		private void UpdateMonthSelected()
		{
			((ToolStripMenuItem)_menuStrip.Items["IntraDay"]).Checked = false;
			((ToolStripMenuItem)_menuStrip.Items["Day"]).Checked = false;
			((ToolStripMenuItem)_menuStrip.Items["Week"]).Checked = false;
			((ToolStripMenuItem)_menuStrip.Items["Month"]).Checked = true;
			((ToolStripMenuItem)_menuStrip.Items["Period"]).Checked = false;

			_toolStripButtonChartPeriodView.Checked = false;
			_toolStripButtonChartMonthView.Checked = true;
			_toolStripButtonChartWeekView.Checked = false;
			_toolStripButtonChartDayView.Checked = false;
			_toolStripButtonChartIntradayView.Checked = false;
		}

		private void UpdateWeekSelected()
		{
			((ToolStripMenuItem)_menuStrip.Items["IntraDay"]).Checked = false;
			((ToolStripMenuItem)_menuStrip.Items["Day"]).Checked = false;
			((ToolStripMenuItem)_menuStrip.Items["Week"]).Checked = true;
			((ToolStripMenuItem)_menuStrip.Items["Month"]).Checked = false;
			((ToolStripMenuItem)_menuStrip.Items["Period"]).Checked = false;

			_toolStripButtonChartPeriodView.Checked = false;
			_toolStripButtonChartMonthView.Checked = false;
			_toolStripButtonChartWeekView.Checked = true;
			_toolStripButtonChartDayView.Checked = false;
			_toolStripButtonChartIntradayView.Checked = false;
		}

		private void UpdateDaySelected()
		{
			((ToolStripMenuItem)_menuStrip.Items["IntraDay"]).Checked = false;
			((ToolStripMenuItem)_menuStrip.Items["Day"]).Checked = true;
			((ToolStripMenuItem)_menuStrip.Items["Week"]).Checked = false;
			((ToolStripMenuItem)_menuStrip.Items["Month"]).Checked = false;
			((ToolStripMenuItem)_menuStrip.Items["Period"]).Checked = false;

			_toolStripButtonChartPeriodView.Checked = false;
			_toolStripButtonChartMonthView.Checked = false;
			_toolStripButtonChartWeekView.Checked = false;
			_toolStripButtonChartDayView.Checked = true;
			_toolStripButtonChartIntradayView.Checked = false;
		}

		private void UpdateIntradaySelected()
		{
			((ToolStripMenuItem)_menuStrip.Items["IntraDay"]).Checked = true;
			((ToolStripMenuItem)_menuStrip.Items["Day"]).Checked = false;
			((ToolStripMenuItem)_menuStrip.Items["Week"]).Checked = false;
			((ToolStripMenuItem)_menuStrip.Items["Month"]).Checked = false;
			((ToolStripMenuItem)_menuStrip.Items["Period"]).Checked = false;

			_toolStripButtonChartPeriodView.Checked = false;
			_toolStripButtonChartMonthView.Checked = false;
			_toolStripButtonChartWeekView.Checked = false;
			_toolStripButtonChartDayView.Checked = false;
			_toolStripButtonChartIntradayView.Checked = true;
		}
	}
}
