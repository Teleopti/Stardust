using System;
using System.Drawing;
using System.Linq;
using Syncfusion.Windows.Forms;
using Teleopti.Ccc.Win.Forecasting.Forms;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Common.Controls.Chart
{
	public partial class GridViewInChartButtons : BaseUserControl
	{
		public event EventHandler<ZoomButtonsEventArgs> ZoomChanged;

		public GridViewInChartButtons()
		{
			InitializeComponent();

			buttonAdvWorkloadDay.Tag = new ZoomButtonsEventArgs { Interval = WorkingInterval.Day, Target = TemplateTarget.Workload };
			ButtonAvdWorkloadWeek.Tag = new ZoomButtonsEventArgs { Interval = WorkingInterval.Week, Target = TemplateTarget.Workload };
			buttonAdvWorkloadMonth.Tag = new ZoomButtonsEventArgs { Interval = WorkingInterval.Month, Target = TemplateTarget.Workload };
			buttonAdvWorkloadIntraday.Tag = new ZoomButtonsEventArgs { Interval = WorkingInterval.Intraday, Target = TemplateTarget.Workload };

			buttonAdvSkillMonth.Tag = new ZoomButtonsEventArgs { Interval = WorkingInterval.Month, Target = TemplateTarget.Skill };
			buttonAdvSkillWeek.Tag = new ZoomButtonsEventArgs { Interval = WorkingInterval.Week, Target = TemplateTarget.Skill };
			buttonAdvSkillDay.Tag = new ZoomButtonsEventArgs { Interval = WorkingInterval.Day, Target = TemplateTarget.Skill };
			buttonAdvSkillIntraday.Tag = new ZoomButtonsEventArgs { Interval = WorkingInterval.Intraday, Target = TemplateTarget.Skill };

			if (!DesignMode) SetTexts();

			setToolTip();
		}

		private void setToolTip()
		{
			GuiHelper.SetToolTip(buttonAdvWorkloadDay, UserTexts.Resources.Day);
			GuiHelper.SetToolTip(ButtonAvdWorkloadWeek, UserTexts.Resources.Week);
			GuiHelper.SetToolTip(buttonAdvWorkloadMonth, UserTexts.Resources.Month);
			GuiHelper.SetToolTip(buttonAdvWorkloadIntraday, UserTexts.Resources.Intraday);

			GuiHelper.SetToolTip(buttonAdvSkillMonth, UserTexts.Resources.Month);
			GuiHelper.SetToolTip(buttonAdvSkillWeek, UserTexts.Resources.Week);
			GuiHelper.SetToolTip(buttonAdvSkillDay, UserTexts.Resources.Day);
			GuiHelper.SetToolTip(buttonAdvSkillIntraday, UserTexts.Resources.Intraday);
		}

		private void skillZoomButtonClicked(object sender, EventArgs e)
		{
			invokeZoomChanged((ZoomButtonsEventArgs)((ButtonAdv)sender).Tag);
		}

		private void workloadZoomButtonClicked(object sender, EventArgs e)
		{
			invokeZoomChanged((ZoomButtonsEventArgs)((ButtonAdv)sender).Tag);
		}

		private void invokeZoomChanged(ZoomButtonsEventArgs e)
		{
			if (ZoomChanged == null) return;
			ZoomChanged.Invoke(this, e);
			Refresh();
		}

		public override bool HasHelp
		{
			get
			{
				return false;
			}
		}

		internal void CheckSingleButton(TemplateTarget templateTarget, WorkingInterval interval)
		{
			foreach (var button in tableLayoutPanel1.Controls.OfType<ButtonAdv>())
			{
				button.State = ButtonAdvState.Default;
				var args = (ZoomButtonsEventArgs)button.Tag;
				button.BackColor = Color.White;
				if (args.Target == templateTarget && args.Interval == interval)
				{
					button.State = ButtonAdvState.Pressed;
					button.BackColor = Color.FromArgb(0, 153, 255);
				}
			}
		}

	}
}