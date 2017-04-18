using System;
using System.Globalization;
using Syncfusion.Windows.Forms.Chart;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Chart
{
	public class ChartManager
	{
		public void SetChartToolTip(ChartRegion chartRegion, ChartControl chartControl)
		{
			if (chartRegion.SeriesIndex < 0 || chartRegion.SeriesIndex >= chartControl.Series.Count)
				return;

			if (chartRegion.IsChartPoint)
			{
				var labelModel =
					chartControl.Series[chartRegion.SeriesIndex].XAxis.LabelsImpl as LabelModel;
				if (chartRegion.PointIndex >= chartControl.Series[chartRegion.SeriesIndex].Points.Count)
					return;

				if (labelModel != null)
				{
					string toolTip = string.Format(CultureInfo.CurrentCulture, "{0} - {1}",
												   labelModel.GetToolTipLabelAt(chartRegion.PointIndex + 1).Text
												   ,
												   Math.Round(
													   chartControl.Series[chartRegion.SeriesIndex].Points[
														   chartRegion.PointIndex].YValues[0], 2));
					chartRegion.ToolTip = toolTip;
				}
			}
		} 
	}
}