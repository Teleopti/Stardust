using System.Windows.Forms;
using Syncfusion.Windows.Forms.Chart;

namespace Teleopti.Ccc.Win.Scheduling.PropertyPanel
{
	public class PerShiftCategoryChart : ChartControl
	{
		public PerShiftCategoryChart()
		{
			initializeComponent();
		}

		private void initializeComponent()
		{
			Dock = DockStyle.Fill;
			PrimaryXAxis.ForceZero = true;
			PrimaryYAxis.ForceZero = true;
		}
	}
}
