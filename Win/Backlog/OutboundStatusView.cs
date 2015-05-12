using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Teleopti.Ccc.Domain.Backlog;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Backlog
{
	public partial class OutboundStatusView : Form
	{
		public OutboundStatusView()
		{
			InitializeComponent();
		}

		public OutboundStatusView(IncomingTask incomingTask)
		{
			InitializeComponent();
			updateChartChart(incomingTask);
		}

		private void updateChartChart(IncomingTask incomingTask)
		{
			chart1.Series.Clear();
			chart1.ChartAreas[0].AxisX.Interval = 1;
			chart1.ChartAreas[0].AxisX.IntervalAutoMode = IntervalAutoMode.VariableCount;
			var series1 = chart1.Series.Add("Estimated outgoing backlog");
			var series2 = chart1.Series.Add("Planned");		
			var series3 = chart1.Series.Add("Scheduled");
			//var series4 = chart1.Series.Add("Estimated total backlog");
			//var series5 = chart1.Series.Add("Planned time");

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

			//series4.ChartType = SeriesChartType.Line;
			//series4.YAxisType = AxisType.Primary;
			//series4.XValueType = ChartValueType.Date;
			//series4.Color = Color.Red;
			//series5.ChartType = SeriesChartType.Line;
			//series5.YAxisType = AxisType.Primary;
			//series5.XValueType = ChartValueType.Date;
			//series5.Color = Color.Blue;

			var dataPointIndex = 0;
			foreach (var date in incomingTask.SpanningPeriod.DayCollection())
			{
				DataPoint datapoint;
				datapoint = new DataPoint(dataPointIndex, incomingTask.GetEstimatedOutgoingBacklogOnDate(date).TotalHours);
				datapoint.AxisLabel = date.ToShortDateString();
				series1.Points.Add(datapoint);

				datapoint = new DataPoint(dataPointIndex, incomingTask.GetPlannedTimeOnDate(date).TotalHours);
				datapoint.AxisLabel = date.ToShortDateString();
				series2.Points.Add(datapoint);

				datapoint = new DataPoint(dataPointIndex, incomingTask.GetScheduledTimeOnDate(date).TotalHours);
				datapoint.AxisLabel = date.ToShortDateString();
				series3.Points.Add(datapoint);

				//datapoint = new DataPoint(i, _model.GetScheduledBacklogTimeOnIncomingIndex(i, skill).GetValueOrDefault(TimeSpan.Zero).TotalHours);
				//series2.Points.Add(datapoint);
				//datapoint = new DataPoint(i, _model.GetScheduledOverstaffOnIncomming(i, skill).GetValueOrDefault(TimeSpan.Zero).TotalHours);
				//series3.Points.Add(datapoint);
				//datapoint = new DataPoint(i, _model.GetScheduledBacklogTimeOnIndex(i, skill).GetValueOrDefault(TimeSpan.Zero).TotalHours);
				//series4.Points.Add(datapoint);
				//datapoint = new DataPoint(i, _model.GetScheduledTimeOnIndex(i, skill).GetValueOrDefault(TimeSpan.Zero).TotalHours);
				//series5.Points.Add(datapoint);

				dataPointIndex++;
			}
		}
	}
}
