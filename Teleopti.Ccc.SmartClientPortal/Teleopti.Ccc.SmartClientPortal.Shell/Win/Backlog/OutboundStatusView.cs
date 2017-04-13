using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Teleopti.Ccc.Domain.Backlog;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Backlog
{
	public partial class OutboundStatusView : Form
	{
		public OutboundStatusView()
		{
			InitializeComponent();
		}

		public OutboundStatusView(IncomingTask incomingTask, string campaignName)
		{
			InitializeComponent();
			updateChartChart(incomingTask, campaignName);
		}

		private void updateChartChart(IncomingTask incomingTask, string campaignName)
		{
			chart1.Series.Clear();
			chart1.ChartAreas[0].AxisX.Interval = 1;
			chart1.ChartAreas[0].AxisX.IntervalAutoMode = IntervalAutoMode.VariableCount;
			chart1.Titles[0].Text = campaignName;
			var series1 = chart1.Series.Add("Estimated outgoing backlog");
			var series2 = chart1.Series.Add("Planned");		
			var series3 = chart1.Series.Add("Scheduled");
			var series4 = chart1.Series.Add("Overstaff");
			var series5 = chart1.Series.Add("Outside SLA");

			series1.ChartType = SeriesChartType.StackedColumn;
			series1.XValueType = ChartValueType.Date;
			series1.YAxisType = AxisType.Primary;
			series1.Color = Color.LightCoral;

			series2.ChartType = SeriesChartType.StackedColumn;
			series2.XValueType = ChartValueType.Date;
			series2.YAxisType = AxisType.Primary;
			series2.Color = Color.LightGreen;

			series3.ChartType = SeriesChartType.StackedColumn;
			series3.XValueType = ChartValueType.Date;
			series3.YAxisType = AxisType.Primary;
			series3.Color = Color.DarkCyan;

			series4.ChartType = SeriesChartType.StackedColumn;
			series4.YAxisType = AxisType.Primary;
			series4.XValueType = ChartValueType.Date;
			series4.Color = Color.Red;

			series5.ChartType = SeriesChartType.StackedColumn;
			series5.YAxisType = AxisType.Primary;
			series5.XValueType = ChartValueType.Date;
			series5.Color = Color.Yellow;

			var dataPointIndex = 0;
			foreach (var date in incomingTask.SpanningPeriod.DayCollection())
			{
				DataPoint datapoint = new DataPoint(dataPointIndex, incomingTask.GetEstimatedOutgoingBacklogOnDate(date).TotalHours);
				datapoint.AxisLabel = date.ToShortDateString();
				series1.Points.Add(datapoint);
				datapoint.ToolTip = toolTip(series1, datapoint);

				datapoint = new DataPoint(dataPointIndex, incomingTask.GetPlannedTimeOnDate(date).TotalHours);
				datapoint.AxisLabel = date.ToShortDateString();
				series2.Points.Add(datapoint);
				datapoint.ToolTip = toolTip(series2, datapoint);

				datapoint = new DataPoint(dataPointIndex, incomingTask.GetScheduledTimeOnDate(date).TotalHours);
				datapoint.AxisLabel = date.ToShortDateString();
				series3.Points.Add(datapoint);
				datapoint.ToolTip = toolTip(series3, datapoint);

				datapoint = new DataPoint(dataPointIndex, incomingTask.GetOverstaffTimeOnDate(date).TotalHours);
				datapoint.AxisLabel = date.ToShortDateString();
				series4.Points.Add(datapoint);
				datapoint.ToolTip = toolTip(series4, datapoint);

				datapoint = date == incomingTask.SpanningPeriod.EndDate ? new DataPoint(dataPointIndex, incomingTask.GetTimeOutsideSLA().TotalHours) : new DataPoint(dataPointIndex, 0);
				datapoint.AxisLabel = date.ToShortDateString();
				series5.Points.Add(datapoint);
				datapoint.ToolTip = toolTip(series5, datapoint);

				dataPointIndex++;
			}
		}

		private static string toolTip(Series series, DataPoint datapoint)
		{
			return series.Name + ": " + Math.Round(datapoint.YValues[0],0).ToString(CultureInfo.InvariantCulture) + " hours";
		}
	}
}
