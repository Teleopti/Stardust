using System.Drawing.Text;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Chart;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.PropertyPanel
{
	public class PerShiftCategoryChart : ChartControl
	{
		public PerShiftCategoryChart()
		{
			initializeComponent();
		}

		private void initializeComponent()
		{
			Series.Clear();

			Dock = DockStyle.Fill;
			//PrimaryXAxis.ForceZero = true;
			PrimaryYAxis.ForceZero = true;
			BackColor = ColorHelper.ChartControlBackColor();
			BackInterior = ColorHelper.ChartControlBackInterior();
			ChartArea.BackInterior = ColorHelper.ChartControlChartAreaBackInterior();
			ChartAreaMargins = ColorHelper.ChartMargins();
			ChartInterior = ColorHelper.ChartControlChartInterior();
			ElementsSpacing = 1;
			ChartArea.AutoScale = false;
			Series3D = false;
			SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
			PrimaryYAxis.ValueType = ChartValueType.Double;
			PrimaryXAxis.DrawGrid = false;
			PrimaryXAxis.ValueType = ChartValueType.Double;
			PrimaryXAxis.RangePaddingType = ChartAxisRangePaddingType.None;
			PrimaryXAxis.RangePaddingType = ChartAxisRangePaddingType.None;
			TextRenderingHint = TextRenderingHint.AntiAlias;
			EnableXZooming = false; 
			EnableYZooming = false;
			Text = UserTexts.Resources.Distribution;
			PrimaryXAxis.LabelIntersectAction = ChartLabelIntersectAction.Rotate;
		}
	}
}
