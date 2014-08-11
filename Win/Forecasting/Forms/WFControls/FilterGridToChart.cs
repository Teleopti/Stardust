using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Windows.Forms;
using Syncfusion.Drawing;
using Syncfusion.Windows.Forms.Chart;
using Syncfusion.Windows.Forms.Grid;
using System.Globalization;
using System.Threading;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.Win.Common.Controls;
using System.ComponentModel;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;

namespace Teleopti.Ccc.Win.Forecasting.Forms.WFControls
{
    public partial class FilterGridToChart : BaseUserControl
    {
    	private WorkloadDayFilterGridControl _gridControl;
        private IDictionary<int, ChartSeries> _cachedChartSeries = new Dictionary<int, ChartSeries>();
        private bool _showGridLines = true;
        private readonly IList<DateTime> _dateRange = new List<DateTime>();
    	private WorkingInterval _workingInterval;
		private IDictionary<int, bool> _cachedLegends = new Dictionary<int, bool>();

    	private FilterGridToChart()
        {
            InitializeComponent();
            if (!DesignMode) SetTexts();
        }

        public FilterGridToChart(TeleoptiGridControl grid) :this()
		{
        	var workloadDayFilterGridControl = grid as WorkloadDayFilterGridControl;
			if(workloadDayFilterGridControl != null)
			{
				_gridControl = workloadDayFilterGridControl;
			}
			SetupGridControl();
			InitiaizeChart();
        }

    	private void InitiaizeChart()
    	{
    		_workingInterval = WorkingInterval.Intraday;

    		SimpleTestCharter();
			var currentCulture = Thread.CurrentThread.CurrentUICulture;
    		if (currentCulture.Calendar.AlgorithmType != CalendarAlgorithmType.SolarCalendar)
    		{
    			currentCulture = CultureInfo.GetCultureInfo(1033);
    		}
    		using (new UICultureContext(currentCulture))
    		{
    			SetChartScale(GridControl.FirstDateTime, GridControl.LastDateTime,
    			              GridControl.ChartResolution);
    			chartControl1.BeginUpdate();
    			if (Enum.IsDefined(typeof (DayOfWeek), GridControl.TemplateIndex))
					chartControl1.Text = string.Format(currentCulture,
									GridControl.GridRowShowInChart + " - " +
									currentCulture.DateTimeFormat.DayNames[GridControl.TemplateIndex]);
                else
                    chartControl1.Text = string.Format(currentCulture,
                                                       GridControl.GridRowShowInChart);
				GridControl.InitializeAllGridColumnsToChart();
    			chartControl1.EndUpdate();
    		}

			GridToChart_SizeChanged(null, null);
			chartControl1.PrimaryXAxis.ValueType = ChartValueType.DateTime;
			chartControl1.EnableXZooming = !CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft;
    	}

    	private void SetupGridControl()
    	{
    		GridControl.FilterDataToChart += _gridControl_FilterDataToChart;
    		GridControl.FilterDataSelectionChanged += GridControl_FilterDataSelectionChanged;
    		GridControl.Dock = DockStyle.Fill;
    		GridControl.TabStop = true;
    		GridControl.TabIndex = 1;
    		splitContainerAdv1.SplitterDistance = splitContainerAdv1.Height -
    		                                      GridControl.RowHeights.GetTotal(0, GridControl.RowCount) - 48;

    		splitContainerAdv1.Panel2.Controls.Add(GridControl);
    		GridControl.Model.MergeCells.EvaluateMergeCells(GridRangeInfo.Table());
    	}

    	private void GridControl_FilterDataSelectionChanged(object sender, FilterDataSelectionChangedEventArgs e)
    	{
    		ClearChart();
    		InitiaizeChart();
    	}

    	private void SetChartScale(DateTime firstTime, DateTime lastTime, TimeSpan resolution)
        {
            int chartResolution;

            chartControl1.PrimaryXAxis.DateTimeFormat = "t";
			if ((int)resolution.TotalMinutes < 15)
				chartResolution = 20;//to make the labels readable...
			else
				chartResolution = (int) resolution.TotalMinutes;
			var  chartIntervalType = ChartDateTimeIntervalType.Minutes;

            //Because of bug concerning OADate
            if (firstTime < new DateTime(1920, 1, 1))
            {
                firstTime = firstTime.AddYears(200);
                lastTime = lastTime.AddYears(200);
            }

            chartControl1.Legend.BackInterior = ColorHelper.ChartControlBackInterior();
            chartControl1.Legend.BackColor = ColorHelper.ChartControlBackColor();
			chartControl1.LegendPosition = ChartDock.Right;
			chartControl1.LegendAlignment = ChartAlignment.Near;
			chartControl1.LegendsPlacement = ChartPlacement.Outside;
            chartControl1.ChartArea.AutoScale = false;
            chartControl1.ElementsSpacing = 1;
            chartControl1.TextRenderingHint = TextRenderingHint.AntiAlias;
            chartControl1.ChartArea.YAxesLayoutMode= ChartAxesLayoutMode.Stacking;
            chartControl1.ChartArea.XAxesLayoutMode = ChartAxesLayoutMode.Stacking;
            chartControl1.PrimaryXAxis.DateTimeRange = new ChartDateTimeRange(firstTime, lastTime, chartResolution, chartIntervalType);
            chartControl1.PrimaryXAxis.Inversed = (RightToLeft == RightToLeft.Yes);
            chartControl1.BackColor = ColorHelper.ChartControlBackColor();
            chartControl1.BackInterior = ColorHelper.ChartControlBackInterior();
            chartControl1.ChartArea.BackInterior = ColorHelper.ChartControlChartAreaBackInterior();
            chartControl1.ChartAreaMargins = ColorHelper.ChartMargins();
            chartControl1.ChartInterior = ColorHelper.ChartControlChartInterior();
            chartControl1.SmoothingMode = SmoothingMode.AntiAlias;
			
            SetPrimaryXAxis();
            SetPrimaryYAxis();
        }

        private void SetPrimaryXAxis()
        {
            chartControl1.PrimaryXAxis.ValueType = ChartValueType.DateTime;
            chartControl1.PrimaryXAxis.HidePartialLabels = true;
            chartControl1.PrimaryXAxis.LabelIntersectAction = ChartLabelIntersectAction.Rotate;
            chartControl1.PrimaryXAxis.DrawGrid = false;
            chartControl1.PrimaryXAxis.TickLabelsDrawingMode = ChartAxisTickLabelDrawingMode.AutomaticMode;
            chartControl1.PrimaryXAxis.RangeType = ChartAxisRangeType.Set;
        }

        private void SetPrimaryYAxis()
        {
			chartControl1.PrimaryYAxis.OpposedPosition = (RightToLeft == RightToLeft.Yes);
            chartControl1.PrimaryYAxis.ValueType = ChartValueType.Double;
            chartControl1.PrimaryYAxis.RangeType = ChartAxisRangeType.Auto;
            chartControl1.PrimaryYAxis.LineType.ForeColor = Color.Black;
            chartControl1.PrimaryYAxis.GridLineType.ForeColor = Color.Black;
            chartControl1.PrimaryYAxis.DrawGrid = ShowGridlines;
            chartControl1.PrimaryYAxis.Orientation = ChartOrientation.Vertical;
            chartControl1.PrimaryYAxis.AutoSize = true;
            chartControl1.PrimaryYAxis.LocationType = ChartAxisLocationType.Auto;
            chartControl1.PrimaryYAxis.RangeType = ChartAxisRangeType.Set;
			chartControl1.PrimaryYAxis.Range.Min = 0;
        }

        private void _gridControl_FilterDataToChart(object sender, FilterDataToChartEventArgs e)
        {
            if (e.Values.Count == 0)
            {
                ClearChart();
                return;
            }
            chartControl1.Series.BeginUpdate();
            chartControl1.PrimaryYAxis.Range.Min = 0;
            ChartSeries chartSerie;
            var serieIndex = e.ColumnIndex;
            var isVisibleColumn = !e.RemoveColumn;
			if (!_cachedLegends.ContainsKey(serieIndex - 1)) _cachedLegends.Add(serieIndex - 1, isVisibleColumn);
			else _cachedLegends[serieIndex - 1] = isVisibleColumn;
			if (!isVisibleColumn && _cachedChartSeries.ContainsKey(serieIndex))
			{
				RemoveSerie(serieIndex);
                GridToChart_SizeChanged(null, null);
                chartControl1.Series.EndUpdate();
                return;
			}
        	
			if (!_cachedChartSeries.ContainsKey(serieIndex))
            {
                chartSerie = new ChartSeries(e.HeaderText, ChartSeriesType.Line);
				chartSerie.Tag = serieIndex;
                chartSerie.PointsToolTipFormat = chartSerie.Name;
                chartControl1.ShowToolTips = true;
                chartSerie.Style.Border.Width = serieIndex == 1 ? 7f : 2f;
                chartSerie.Text = chartSerie.Name;
				chartSerie.Visible = isVisibleColumn;
				_cachedChartSeries.Add(serieIndex, chartSerie);
            }
            else
            {
				chartSerie = _cachedChartSeries[serieIndex];
                chartSerie.Points.Clear();
                chartSerie.Visible = true;
            }
            chartSerie.Name = e.HeaderText;
            _dateRange.Clear(); //ok this should be done once... but first make it work
			
            foreach (KeyValuePair<DateTime, double> pair in e.Values)
            {
                DateTime keyTime = pair.Key;
                if (keyTime < new DateTime(1920, 1, 1)) keyTime = keyTime.AddYears(200);
            	ChartPoint x1;
				x1 = pair.Value == double.NegativeInfinity ? new ChartPoint(keyTime, 0) : new ChartPoint(keyTime, pair.Value);
                chartSerie.Points.Add(x1);
                _dateRange.Add(keyTime );
            }

            if (e.Values.Count > 0 && _workingInterval==WorkingInterval.Day)
            {
                chartControl1.PrimaryXAxis.Range.Min = chartSerie.Points[0].X - 1;
                chartControl1.PrimaryXAxis.Range.Max = chartSerie.Points[e.Values.Count - 1].X + 1;
            }
            else if (e.Values.Count > 0)
            {
                double offset = Math.Round((chartControl1.PrimaryXAxis.DateTimeRange.DefaultInterval.Value / 5), 2);
                chartControl1.PrimaryXAxis.Range.Min = chartSerie.Points[0].X - offset;
                chartControl1.PrimaryXAxis.Range.Max = chartSerie.Points[e.Values.Count - 1].X + offset;
            }

            if (chartControl1.Series.IndexOf(chartSerie) < 0)
            {
                chartControl1.Series.Add(chartSerie);
				chartSerie.YAxis = chartControl1.PrimaryYAxis;
                chartSerie.XAxis = chartControl1.PrimaryXAxis;
			}
            else
            {
                chartControl1.Series.ResetCache(chartSerie);
            }
            GridToChart_SizeChanged(null, null);
            chartControl1.Series.EndUpdate();
			chartControl1.PrimaryXAxis.ValueType = ChartValueType.DateTime;
        }

		private void ClearChart()
    	{
			chartControl1.Series.BeginUpdate();
    		chartControl1.Series.Clear();
    		_cachedChartSeries.Clear();
    		foreach(ChartAxis axis in chartControl1.Axes)
    		{
				if (axis.Equals(chartControl1.PrimaryYAxis) || axis.Equals(chartControl1.PrimaryXAxis))
					continue;
    			chartControl1.Axes.Remove(axis);
    		}
    		GridToChart_SizeChanged(null, null);
			chartControl1.Series.EndUpdate();
    	}

    	private void RemoveSerie(int serieIndexToBeDeleted)
    	{
            ChartSeries chartSerie;
            if (_cachedChartSeries.ContainsKey(serieIndexToBeDeleted))
            {
                chartSerie = _cachedChartSeries[serieIndexToBeDeleted];
                chartSerie.Visible = false;
            }
    	}

        private void SimpleTestCharter()
        {
            chartControl1.Series3D =false;
            chartControl1.RealMode3D = false;
            chartControl1.Tilt = 0;
            chartControl1.Legend.VisibleCheckBox = false;
        }

        private void GridToChart_SizeChanged(object sender, EventArgs e)
        {
            SetPrimaryYAxis();
        }

        private void chartControl1_ChartRegionClick(object sender, ChartRegionMouseEventArgs e)
        {
			if (e.Region == null) return;
			var colIndex = e.Region.SeriesIndex + 1;
			if (colIndex > 0)
			{
				GridControl.Model.ScrollCellInView(GridRangeInfo.Col(colIndex), GridScrollCurrentCellReason.MoveTo);
				GridControl.CurrentCell.MoveTo(GridRangeInfo.Cell(0, colIndex));
			}
        }
		
    	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "Gridlines"), Browsable(false)]
        public bool ShowGridlines
        {
            get { return _showGridLines; }
            set { _showGridLines = value; }
        }

		public WorkloadDayFilterGridControl GridControl
        {
            get { return _gridControl; }
        }

        public override bool HasHelp
        {
            get
            {
                return false;
            }
        }

    	private void chartControl1_LayoutCompleted(object sender, EventArgs e)
    	{
    		foreach (var legend in _cachedLegends.Where(legend => legend.Key < chartControl1.Legend.Items.Length))
    		{
    			chartControl1.Legend.Items[legend.Key].Visible = legend.Value;
    		}
    	}
    }
}
