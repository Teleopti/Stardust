using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Syncfusion.Drawing;
using Syncfusion.Grouping;
using Syncfusion.Windows.Forms.Chart;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Cells;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.Win.Forecasting.Forms;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Chart
{
    public partial class GridToChart : BaseUserControl
    {
        private TeleoptiGridControl _gridControl = new TeleoptiGridControl();
        private readonly ChartAxis _secYAxis = new ChartAxis();
        private readonly IDictionary<int, ChartSeries> _cachedChartSeries = new Dictionary<int, ChartSeries>();
        private bool _showGridLines = true;
        private readonly IList<DateTime> _dateRange = new List<DateTime>();
        private  WorkingInterval _workingInterval;
		private readonly IDictionary<ChartSeries, bool> _cachedSeriesVisibility = new Dictionary<ChartSeries, bool>(); 

        private GridToChart()
        {
            InitializeComponent();
            if (!DesignMode) SetTexts();
        }

        public GridToChart(TeleoptiGridControl grid): this()
        {
            _workingInterval = WorkingInterval.Intraday;
            _gridControl = grid;
            
         
            SimpleTestCharter();

            CultureInfo currentCulture = Thread.CurrentThread.CurrentUICulture;
            if (currentCulture.Calendar.AlgorithmType != CalendarAlgorithmType.SolarCalendar)
            {
                currentCulture = CultureInfo.GetCultureInfo(1033);
            }
            using (new UICultureContext(currentCulture))
            {
                SetChartScale(GridControl.WorkingInterval, GridControl.FirstDate, GridControl.LastDate,
                              GridControl.ChartResolution);

                chartControl1.BeginUpdate();

                GridControl.DataToChart += _gridControl_DataToChart;
                GridControl.Dock = DockStyle.Fill;
                GridControl.TabStop = true;
                GridControl.TabIndex = 1;
                splitContainerAdv1.SplitterDistance = splitContainerAdv1.Height -
                                                      GridControl.RowHeights.GetTotal(0, GridControl.RowCount) - 48;

                splitContainerAdv1.Panel2.Controls.Add(GridControl);
                SetColors();

                GridControl.InitializeAllGridRowsToChart();
                GridControl.Model.MergeCells.EvaluateMergeCells(GridRangeInfo.Table());

                chartControl1.EndUpdate();
            }

            chartControl1.EnableXZooming = !CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft; //To avoid bugs concerning zoom in chart control
        }

        private void SetChartScale(WorkingInterval interval, DateTime firstTime, DateTime lastTime, TimeSpan resolution)
        {
            ChartDateTimeIntervalType chartIntervalType;
            int chartResolution;
            int diff = lastTime.Subtract(firstTime).Days;
            _workingInterval = interval;
            switch (interval)
            {
                case WorkingInterval.Intraday:
                    chartControl1.PrimaryXAxis.DateTimeFormat = "t";
                    if ((int)resolution.TotalMinutes < 15)
                        chartResolution = 20;//to make the labels readable...
                    else
                        chartResolution = (int)resolution.TotalMinutes;

                    chartIntervalType = ChartDateTimeIntervalType.Minutes;
                    break;
                case WorkingInterval.Day:
                    chartControl1.PrimaryXAxis.DateTimeFormat = "d";
                    if (diff > 61)
                        chartResolution = diff / 60;
                    else
                        chartResolution = (int) resolution.TotalDays;
                    chartIntervalType = ChartDateTimeIntervalType.Days;
                    break;
                default:
                    chartControl1.PrimaryXAxis.DateTimeFormat = "d";
                    chartResolution = (int)resolution.TotalDays;
                    chartIntervalType = ChartDateTimeIntervalType.Days;
                    break;
            }

            //Because of bug concerning OADate
            if (firstTime < new DateTime(1920, 1, 1))
            {
                firstTime = firstTime.AddYears(200);
                lastTime = lastTime.AddYears(200);
            }

            chartControl1.Legend.BackInterior = ColorHelper.ChartControlBackInterior();
            chartControl1.Legend.BackColor = ColorHelper.ChartControlBackColor();
            chartControl1.LegendPosition = ChartDock.Bottom;
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
            chartControl1.LegendPosition = ChartDock.Bottom;
            chartControl1.LegendAlignment = ChartAlignment.Center;
            chartControl1.LegendsPlacement = ChartPlacement.Outside;

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
            chartControl1.PrimaryYAxis.OpposedPosition = true;
            chartControl1.PrimaryYAxis.ValueType = ChartValueType.Double;
            chartControl1.PrimaryYAxis.RangeType = ChartAxisRangeType.Auto;
            chartControl1.PrimaryYAxis.LineType.ForeColor = Color.Black;
            chartControl1.PrimaryYAxis.GridLineType.ForeColor = Color.Black;
            chartControl1.PrimaryYAxis.DrawGrid = ShowGridlines;
            chartControl1.PrimaryYAxis.Orientation = ChartOrientation.Vertical;
            chartControl1.PrimaryYAxis.AutoSize = true;
            chartControl1.PrimaryYAxis.LocationType = ChartAxisLocationType.Auto;
            chartControl1.PrimaryYAxis.RangeType = ChartAxisRangeType.Set;
        }

        private void _gridControl_DataToChart(object sender, DataToChartEventArgs e)
        {
            chartControl1.Series.BeginUpdate();
            chartControl1.PrimaryYAxis.Range.Min = 0;
            ChartSeries chartSerie;
       
            if (!_cachedChartSeries.ContainsKey(e.RowIndex))
            {
                chartSerie = new ChartSeries(e.HeaderText, ChartSeriesType.Line);
                chartSerie.Tag = e.RowIndex;
                chartSerie.Style.ToolTip = chartSerie.Name;
                chartSerie.Style.Border.Width = 2f;
                chartSerie.Text = chartSerie.Name;
                _cachedChartSeries.Add(e.RowIndex, chartSerie);
            }
            else
            {
                chartSerie = _cachedChartSeries[e.RowIndex];       
                chartSerie.Points.Clear();
            }

            if (string.IsNullOrEmpty(e.HeaderText))
            { 
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
                if (e.CellModelType is NumericCellModel ||
                    e.CellModelType is NumericReadOnlyCellModel)
                {

                    SetSecondaryYAxis();

                    chartControl1.Axes.Add(_secYAxis);
                    chartSerie.YAxis = _secYAxis;
                    chartSerie.XAxis = chartControl1.PrimaryXAxis;
                    chartSerie.Text += "(<)";
                }
                else
                {
                    chartSerie.YAxis = chartControl1.PrimaryYAxis;
                    chartSerie.Text += "(>)";
                }
            }
            else
            {
                chartControl1.Series.ResetCache(chartSerie);
            }
            chartSerie.AppearanceChanged += ChartSerieOnAppearanceChanged;
            GridToChart_SizeChanged(null, null);
            chartControl1.Series.EndUpdate();
            //Stupid chart have to do this HERE to make the 
            //chart y axis to start from ZERO.
            _secYAxis.Range.Min = 0;
            chartControl1.PrimaryYAxis.Range.Min = 0;
            chartControl1.PrimaryXAxis.ValueType = ChartValueType.DateTime;
			FixOutOfMemoryProblem();
			
			foreach (var axis in chartControl1.Axes
				.Cast<ChartAxis>()
				.Where(a => a.ValueType == ChartValueType.Double))
				ZoomYAxis(chartControl1.PrimaryXAxis.VisibleRange, axis);
        }

		private void FixOutOfMemoryProblem()
		{
			//This is a fix for bug: 15957 Chosing a Smoothing hangs CCC 
			//Sometimes?!? the auto calculation of the secondary axis
			// calculates the axis visible range interval 
			//to the stupid value of: 2e-15
			//Might be a bug in Syncfusion chart?
			if (_secYAxis.VisibleRange.Interval < 1)
			{
				_secYAxis.VisibleRange.Interval = 1;
			}
			if (chartControl1.PrimaryYAxis.VisibleRange.Interval < 0.01)
			{
				chartControl1.PrimaryYAxis.VisibleRange.Interval = 1;
			}
		}

        private void SetSecondaryYAxis()
        {
            chartControl1.Indexed = false ;
            _secYAxis.LocationType = ChartAxisLocationType.Auto;
	        _secYAxis.LineType.ForeColor = Color.Gray;
	        _secYAxis.DrawGrid = ShowGridlines;
            _secYAxis.GridLineType.ForeColor = Color.Gray;
            _secYAxis.OpposedPosition = false;
            _secYAxis.Orientation = ChartOrientation.Vertical;
            _secYAxis.LabelIntersectAction = ChartLabelIntersectAction.Rotate;
            _secYAxis.ValueType = ChartValueType.Double;
            _secYAxis.RangeType = ChartAxisRangeType.Set;
        }

        private void SimpleTestCharter()
        {
            chartControl1.Series3D =false;
            chartControl1.RealMode3D = false;
            chartControl1.Tilt = 0;
            chartControl1.Legend.VisibleCheckBox = true;
        }

        private void SetColors()
        {
            BrushInfo myBrush = ColorHelper.ControlGradientPanelBrush();
            GridControl.Properties.BackgroundColor = ColorHelper.GridControlGridExteriorColor();
            GridControl.BackColor = ColorHelper.GridControlGridInteriorColor();
            splitContainerAdv1.BackgroundColor = myBrush;
            splitContainerAdv1.Panel1.BackgroundColor = myBrush;
            splitContainerAdv1.Panel2.BackgroundColor = myBrush;
        }

        private void GridToChart_SizeChanged(object sender, EventArgs e)
        {
            SetPrimaryYAxis();
            SetSecondaryYAxis();
        }

        private void chartControl1_ChartRegionClick(object sender, ChartRegionMouseEventArgs e)
        {
            DateTime date = GridChartManager.GetDateForChartPoint(chartControl1, e.Point);
            int column2 = GetColumnFromDate(date);
            if (column2 >0)
            {
                GridControl.Model.ScrollCellInView(GridRangeInfo.Col(column2), GridScrollCurrentCellReason.MoveTo);
                GridControl.Model.Selections.SelectRange(GridRangeInfo.Cell(0, column2), true);
           }
        }

        private int  GetColumnFromDate(DateTime date)
        {
            int i = 0;
            switch (_workingInterval)
            {

                case WorkingInterval.Intraday:
                
                    foreach (var time in _dateRange)
                    {
                        i += 1;
                        if (time.AddMinutes(-3) <= date && time.AddMinutes(3) >= date)//same date
                        {

                            return i;
                        }
                    }

                    break;
                default:
              
                    foreach (var time in _dateRange)
                    {
                        i += 1;
                        if (time.ToShortDateString() == date.ToShortDateString())//same date
                        {

                            return i;
                        }
                    }
                    break;
            }



            return 0;
        }

	    private void ChartSerieOnAppearanceChanged(object sender, EventArgs e)
	    {
		    var xInfo = chartControl1.PrimaryXAxis.VisibleRange;
		    var serie = sender as ChartSeries;
		    if (serie == null)
			    return;

		    if (!IsThisFirstTimeRunningForThisChange(serie))
			    return;

		    ZoomYAxis(xInfo, serie.ActualYAxis);
		    AddOrUpdateVisibleDictionary(serie);
	    }

	    private bool IsThisFirstTimeRunningForThisChange(ChartSeries serie)
	    {
		    if (_cachedSeriesVisibility.ContainsKey(serie))
			    return _cachedSeriesVisibility[serie] != serie.Visible;
		 
			_cachedSeriesVisibility.Add(serie, serie.Visible);
		    return true;
	    }

	    private void ZoomYAxis(MinMaxInfo xInfo, ChartAxis axis)
	    {
		    var max = GetHighestYValueForYAxis(xInfo, axis);
		    var roundedMinMax = GetRoundedMinMax(max);

		    axis.RoundingPlaces = max > 3
			                          ? 0
			                          : 2;
		    axis.Range = roundedMinMax;
	    }

	    private double GetHighestYValueForYAxis(MinMaxInfo xInfo, ChartAxis axis)
	    {
		    var visibleSeriesOnCurrentYAxis = chartControl1.Series
		                                                   .Cast<ChartSeries>()
		                                                   .Where(serie => serie.ActualYAxis == axis &&
		                                                                   serie.Visible)
		                                                   .ToList();
		    var max = double.MinValue;
		    var engine = new Engine();
		    foreach (var points in visibleSeriesOnCurrentYAxis.Select(s => s.Points))
		    {
			    engine.SetSourceList(points);
			    var recordFilterDescriptor = new RecordFilterDescriptor("X", FilterLogicalOperator.And, new[]
				    {
					    new FilterCondition(FilterCompareOperator.GreaterThanOrEqualTo, xInfo.Min),
					    new FilterCondition(FilterCompareOperator.LessThanOrEqualTo, xInfo.Max)
				    });
			    engine.TableDescriptor.RecordFilters.Add(recordFilterDescriptor);

			    foreach (var record in engine.Table.FilteredRecords)
			    {
				    var yValues = (double[]) record["YValues"];
				    var recordMax = yValues[0];
				    max = recordMax > max ? recordMax : max;
			    }
		    }
		    max = max > 0 ? max : 1;
		    return max;
	    }

	    private static MinMaxInfo GetRoundedMinMax(double value)
	    {
		    var roundedStep = CalculateInterval(value, 0.1);
		    roundedStep = roundedStep.Equals(0)
			                  ? 1
			                  : roundedStep;

		    var roundedValue = (((int) Math.Round(value/roundedStep))*roundedStep);
		    roundedValue = roundedValue.Equals(0)
			                   ? 1
			                   : roundedValue;

		    return new MinMaxInfo(0, roundedValue + roundedStep, roundedStep);
	    }

	    private static double CalculateInterval(double value, double increment)
	    {
		    if (value < 6*increment)
			    return 1*increment;
		    if (value < 12*increment)
			    return 2*increment;
		    if (value <= 30*increment)
			    return 5*increment;
		    return CalculateInterval(value, increment*10);
	    }

	    private void AddOrUpdateVisibleDictionary(ChartSeries serie)
		{
			if (_cachedSeriesVisibility.ContainsKey(serie))
				_cachedSeriesVisibility[serie] = serie.Visible;
			else
				_cachedSeriesVisibility.Add(serie, serie.Visible);

		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "Gridlines"), Browsable(false)]
        public bool ShowGridlines
        {
            get { return _showGridLines; }
            set { _showGridLines = value; }
        }

        public TeleoptiGridControl GridControl
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


    }
}
