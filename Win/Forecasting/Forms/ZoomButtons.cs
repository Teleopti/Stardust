using System;
using System.Drawing;
using System.Linq;
using Syncfusion.Windows.Forms;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Forecasting.Forms
{
	public partial class ZoomButtons : BaseUserControl
	{
		public event EventHandler<ZoomButtonsEventArgs> ZoomChanged;

		public ZoomButtons()
		{
			InitializeComponent();

			buttonAdvWorkloadDay.Tag = new ZoomButtonsEventArgs { Interval = WorkingInterval.Day, Target = TemplateTarget.Workload };
			buttonAvdWorkloadWeek.Tag = new ZoomButtonsEventArgs { Interval = WorkingInterval.Week, Target = TemplateTarget.Workload };
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
			GuiHelper.SetToolTip(buttonAdvWorkloadDay, Resources.Day);
			GuiHelper.SetToolTip(buttonAvdWorkloadWeek, Resources.Week);
			GuiHelper.SetToolTip(buttonAdvWorkloadMonth, Resources.Month);
			GuiHelper.SetToolTip(buttonAdvWorkloadIntraday, Resources.Intraday);

			GuiHelper.SetToolTip(buttonAdvSkillMonth, Resources.Month);
			GuiHelper.SetToolTip(buttonAdvSkillWeek, Resources.Week);
			GuiHelper.SetToolTip(buttonAdvSkillDay, Resources.Day);
			GuiHelper.SetToolTip(buttonAdvSkillIntraday, Resources.Intraday);
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
			var handler = ZoomChanged;
			if (handler != null)
			{
					handler.Invoke(this, e);
			}
		}

		internal void CheckButton(TemplateTarget templateTarget, WorkingInterval interval)
		{
			foreach (var button in tableLayoutPanel1.Controls.OfType<ButtonAdv>())
			{
				var args = (ZoomButtonsEventArgs)button.Tag;
				if (templateTarget != args.Target) continue;
				button.State = ButtonAdvState.Default;
				button.BackColor = Color.White;
				if (interval == args.Interval)
				{
						button.State = ButtonAdvState.Pressed;
					button.BackColor = Color.FromArgb(0, 153, 255);
				}
			}
		}
	}
}
