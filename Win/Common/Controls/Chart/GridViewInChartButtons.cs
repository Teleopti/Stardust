using System;
using System.Linq;
using Syncfusion.Windows.Forms;
using Teleopti.Ccc.Win.Forecasting.Forms;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Common.Controls.Chart
{
    public partial class GridViewInChartButtons : BaseUserControl
    {
        public event EventHandler<ZoomButtonsEventArgs> ZoomChanged;


        /// <summary>
        /// Initializes a new instance of the <see cref="GridViewInChartButtons"/> class. it is a set of buttons which handles the zooming on the grid
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-02
        /// </remarks>
        /// <remarks>
        /// Created by: ostenpe
        /// Created date: 2008-06-09
        /// </remarks>
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

            SetToolTip();
        }

        private void SetToolTip()
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

        #region zoominvocation

        private void SkillZoomButtonClicked(object sender, EventArgs e)
        {
            invokeZoomChanged((ZoomButtonsEventArgs)((ButtonAdv)sender).Tag);
        }

        private void WorkloadZoomButtonClicked(object sender, EventArgs e)
        {
            invokeZoomChanged((ZoomButtonsEventArgs)((ButtonAdv)sender).Tag);
        }

        /// <summary>
        /// Invokes the zoom changed.
        /// i changed principle after designing the buttons
        /// </summary>
        /// <param name="e">The <see cref="ZoomButtonsEventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: ostenpe
        /// Created date: 2008-06-09
        /// </remarks>
        private void invokeZoomChanged(ZoomButtonsEventArgs e)
        {
            if (ZoomChanged == null) return;
            ZoomChanged.Invoke(this, e);
            Refresh();
        }



        #endregion

		public override bool HasHelp
		{
			get
			{
				return false;
			}
		}

        /// <summary>
        /// marks the button that correspond tyo the selected view in chart.
        /// </summary>
        /// <param name="templateTarget">The template target.</param>
        /// <param name="interval">The interval.</param>
        /// <remarks>
        /// Created by: ostenpe
        /// Created date: 2008-06-09
        /// </remarks>
        internal void CheckSingleButton(TemplateTarget templateTarget, WorkingInterval interval)
        {
            foreach (var button in Enumerable.OfType<ButtonAdv>(this.tableLayoutPanel1.Controls))
            {
                button.State = ButtonAdvState.Default;
                var args = (ZoomButtonsEventArgs) button.Tag;
                if(args.Target==templateTarget && args.Interval == interval )
                {
                    button.State = ButtonAdvState.Pressed;
                }
            }
        }

    }
}