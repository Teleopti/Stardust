using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Globalization;
using System.Windows.Forms;
using Syncfusion.Drawing;
using Syncfusion.Windows.Forms.Chart;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Cells;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Chart;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Rows;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Chart
{
    public class GridChartManager:IDisposable
    {
        private readonly ChartControl _chartControl;
        private ChartAxis _secondaryYAxis;
        private ITaskOwnerGrid _currentGrid;
        private string _name;
        private readonly bool _timeSpanAsMinutes;
        private readonly bool _andThenToHours;
        private bool _allowNegativeValues;

	    public GridChartManager(ChartControl chartControl, bool timeSpanAsMinutes, bool andThenToHours, bool allowNegativeValues)
        {
            _chartControl = chartControl;
            _timeSpanAsMinutes = timeSpanAsMinutes;
            if (_andThenToHours)
                _timeSpanAsMinutes = true;
            _andThenToHours = andThenToHours;
            _allowNegativeValues = allowNegativeValues;
        }

        
        public bool AllowNegativeValues
        {
            get { return _allowNegativeValues; }
            set { _allowNegativeValues = value; }
        }

        public ITaskOwnerGrid CurrentGrid
        {
            get { return _currentGrid; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GridChartManager"/> class.
        /// </summary>
        /// <param name="chartControl">The chart control.</param>
        /// <param name="timeSpanAsMinutes">if set to <c>true</c> [time span as minutes].</param>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-05-16
        /// </remarks>
        public GridChartManager(ChartControl chartControl, bool timeSpanAsMinutes)
        {
            _chartControl = chartControl;
            _timeSpanAsMinutes = timeSpanAsMinutes;
        }

        /// <summary>
        /// Creates this instance.
        /// </summary>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-05-16
        /// </remarks>
        public void Create()
        {
            setChartRequirements();
        }

        /// <summary>
        /// Sets the chart requirements.
        /// </summary>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-05-16
        /// </remarks>
        private void setChartRequirements()
        {
            _chartControl.Series.Clear();
            _chartControl.BackColor = ColorHelper.ChartControlBackColor();
            _chartControl.BackInterior = ColorHelper.ChartControlBackInterior();
            _chartControl.ChartArea.BackInterior = ColorHelper.ChartControlChartAreaBackInterior();
            _chartControl.ChartAreaMargins = ColorHelper.ChartMargins();
            _chartControl.ChartInterior = ColorHelper.ChartControlChartInterior();
            _chartControl.ElementsSpacing = 1;
            _chartControl.ChartArea.AutoScale = false;
            _chartControl.Legend.BackColor = ColorHelper.ChartControlBackColor();
            _chartControl.Legend.VisibleCheckBox = false;
            _chartControl.LegendPosition = ChartDock.Bottom;
            _chartControl.LegendAlignment = ChartAlignment.Center;
            _chartControl.LegendsPlacement = ChartPlacement.Outside;
            _chartControl.Legend.Enabled = false;
            _chartControl.Series3D = false;
            _chartControl.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            _chartControl.PrimaryYAxis.ValueType = ChartValueType.Double;
            _chartControl.PrimaryYAxis.DrawGrid = false;
            _chartControl.PrimaryXAxis.ValueType = ChartValueType.Custom;
            _chartControl.PrimaryXAxis.LabelIntersectAction = ChartLabelIntersectAction.Rotate;
            _chartControl.PrimaryXAxis.RangePaddingType = ChartAxisRangePaddingType.None;
            _chartControl.PrimaryXAxis.LabelIntersectAction = ChartLabelIntersectAction.Rotate;
            _chartControl.PrimaryXAxis.RangePaddingType = ChartAxisRangePaddingType.None;

            _chartControl.ChartArea.PrimaryXAxis.HidePartialLabels = false;
            _chartControl.ChartArea.PrimaryYAxis.HidePartialLabels = true;
            _chartControl.TextRenderingHint = TextRenderingHint.AntiAlias;
            _chartControl.EnableXZooming = false; //Removed zooming, it just doesnt work
            _chartControl.EnableYZooming = false;

            _secondaryYAxis = new ChartAxis();
            _secondaryYAxis.DrawGrid = false;
            _secondaryYAxis.OpposedPosition = true;
            _secondaryYAxis.Orientation = ChartOrientation.Vertical;
            _secondaryYAxis.LabelIntersectAction = ChartLabelIntersectAction.Rotate;
            _secondaryYAxis.ValueType = ChartValueType.Double;
            _secondaryYAxis.RangeType = ChartAxisRangeType.Auto;
            _secondaryYAxis.LocationType = ChartAxisLocationType.Auto;
            _chartControl.Axes.Add(_secondaryYAxis);
            _secondaryYAxis.VisibleRange.Max = 100;
        }

        /// <summary>
        /// Gets the date by column.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <param name="oldDate">The old date.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-06-09
        /// </remarks>
        public DateOnly GetDateByColumn(int column, DateOnly oldDate)
        {
            var returnDate = oldDate;
            ITaskOwnerGrid grid = _currentGrid;
            if(grid!=null)
            {
                var locatedDate = grid.GetLocalCurrentDate(column);
                if (locatedDate != DateOnly.MaxValue)
                    returnDate = locatedDate;
            }
            return returnDate;
        }

        /// <summary>
        /// Reloads the chart.
        /// </summary>
        /// <param name="currentGrid">The current grid.</param>
        /// <param name="name">The name.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-09-23
        /// </remarks>
        public void ReloadChart(ITaskOwnerGrid currentGrid, string name)
        {
            _currentGrid = currentGrid;
            _name = name;
            ReloadChart(); 
        }

        /// <summary>
        /// Reloads the chart.
        /// </summary>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-05-16
        /// </remarks>
        public void ReloadChart()
        {
            if (_currentGrid == null)
                return;

            using (PerformanceOutput.ForOperation("ReloadChart"))
            {
                //Check that the grid is ok
                if (!gridValidated()) return;

                GridControl grid = _currentGrid as GridControl;
				grid.Refresh();

                //Init chart
                init(grid, _name);

                _chartControl.BeginUpdate();
                _chartControl.PrimaryYAxis.RangeType = ChartAxisRangeType.Auto;

                //create axislabels, grid null check is done in gridValidated() 
                string[] xAxisLabels = new string[grid.ColCount + 1];

                //IDictionary<int, GridRow> rows = _currentGrid.EnabledChartGridRows;
                IList<GridRow> rows = _currentGrid.EnabledChartGridRowsMicke65();

                _chartControl.Series.Clear();

                //Loop the gridrows that will be displayed
                foreach (GridRow row in rows)
                {
                    ChartSeries chartSeries = new ChartSeries("series");
                    createSeriesType(row, chartSeries);
                    chartSeries.Style.Border.Color = row.ChartSeriesSettings.Color;

                    int pointIndex = 1; //Point index in chart
                    int headerCountIndex = grid.Cols.HeaderCount + 1; //HeaderCount 1 in Syncfusion means 2 headers 0,1 ?!? 

                    //add the points to to the chart series
                    for (int i = headerCountIndex; i < grid.ColCount + 1; i++)
                    {
                        string columnheaderName = Convert.ToString(grid[_currentGrid.MainHeaderRow, i].CellValue,
                                                                   CultureInfo.CurrentCulture);

                        xAxisLabels.SetValue(columnheaderName, pointIndex);

                        var index = _currentGrid.AllGridRows.IndexOf(row) + grid.Rows.HeaderCount + 1;
                        var cellValue = getCellValue(grid[index, i], _timeSpanAsMinutes, _andThenToHours);
                        //the infinitybug - if i handle it in the getcellvalue i have to do it on all returnvalues 
                        if (double.IsInfinity(cellValue)) cellValue = 0;
                        //----------
                        chartSeries.Points.Add(new ChartPoint(pointIndex, cellValue));
                        ++pointIndex;
                    }

                    setChartSeriesText(row, chartSeries);

                    chartSeries.Style.ToolTip = "serie";
                    _chartControl.Series.Add(chartSeries);
                }

                _chartControl.PrimaryXAxis.LabelsImpl = new LabelModel(xAxisLabels, 50);
                _chartControl.EndUpdate();

                checkNegativeValues();
                
                _chartControl.PrimaryXAxis.RangePaddingType = ChartAxisRangePaddingType.Calculate;

                fixOutOfMemoryProblem();

            }
        }

        private void fixOutOfMemoryProblem()
        {
            //This is a fix for bug: 6319 OutOfMemoeyException 
            //Sometimes?!? the auto calculation of the secondary axis
            // calculates the axis visible range interval 
            //to the stupid value of: 0.000000000000000050000000000000005
            //Might be a bug in Syncfusion chart?
            if (_secondaryYAxis.VisibleRange.Interval < 0.001)
            {
                _secondaryYAxis.VisibleRange.Interval = 1;
            }
            if (_chartControl.PrimaryYAxis.VisibleRange.Interval < 0.001)
            {
                _chartControl.PrimaryYAxis.VisibleRange.Interval = 1;
            }
        }

        private void checkNegativeValues()
        {
            if (!_allowNegativeValues)
            {
                _chartControl.PrimaryYAxis.Range.Min = 0;
                _secondaryYAxis.Range.Min = 0;
                _chartControl.PrimaryYAxis.RangeType = ChartAxisRangeType.Auto;
                _secondaryYAxis.RangeType = ChartAxisRangeType.Auto;
            }
        }

        private static void createSeriesType(GridRow row, ChartSeries chartSeries)
        {
            if (row.ChartSeriesSettings.SeriesType == ChartSeriesDisplayType.Line)
            {
                createLine(row, chartSeries);
            }
            else
            {
                createColumn(row, chartSeries);
            }
        }

        private void setChartSeriesText(GridRow row, ChartSeries chartSeries)
        {
            if (row.ChartSeriesSettings.AxisLocation == AxisLocation.Left)
            {
                chartSeries.Text = string.Concat(row.RowHeaderText, " (<)");
            }
            else
            {
                chartSeries.Text = string.Concat(row.RowHeaderText, " (>)");
                chartSeries.YAxis = _secondaryYAxis;
            }
        }

        private bool gridValidated()
        {
            bool returnValue = true;
            GridControl grid = _currentGrid as GridControl;
            if (grid == null)
            {
                returnValue = false;
            }else if (grid.ColCount <= 0)
            {
                //Just add a fake series, coz Syncfusion cant handle "No" series
                _chartControl.Series.Clear();
                ChartSeries fakeSeries = new ChartSeries(UserTexts.Resources.None);
                _chartControl.Series.Add(fakeSeries);
                _chartControl.Text = "";
                _chartControl.PrimaryXAxis.LabelsImpl = new LabelModel(new string[0]);
                returnValue = false;
            }
            if (_currentGrid.EnabledChartGridRows.Count == 0)
            {
				clearChart();
                returnValue = false;
            }
            return returnValue;
        }

        private static void createColumn(GridRow row, ChartSeries chartSeries)
        {
            chartSeries.ConfigItems.ColumnItem.ColumnType = ChartColumnType.Cylinder;
            chartSeries.ConfigItems.ColumnItem.CornerRadius = new SizeF(1,1);
            chartSeries.Style.Interior =
                new BrushInfo(190, new BrushInfo(GradientStyle.Vertical, row.ChartSeriesSettings.Color, Color.FromArgb(10,row.ChartSeriesSettings.Color)));
        }

        private static void createLine(GridRow row, ChartSeries chartSeries)
        {
            chartSeries.Type = (ChartSeriesType)row.ChartSeriesSettings.SeriesType;
            chartSeries.Style.Border.Width = 3f;
            chartSeries.Style.Interior = new BrushInfo(140, new BrushInfo(row.ChartSeriesSettings.Color));
        }

        /// <summary>
        /// Clears the chart.
        /// </summary>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-05-16
        /// </remarks>
        private void clearChart()
        {
            //Syncfusion Chart cant "live" without at least 1 serie
            _chartControl.Series.Clear();
            ChartSeries dummySeries = new ChartSeries(string.Empty);
            _chartControl.Series.Add(dummySeries);

            if (dummySeries.LegendItem!=null)
                dummySeries.LegendItem.Visible = false;

            _chartControl.Text = UserTexts.Resources.NoChartDataRowsEnabled;
        }

        /// <summary>
        /// Gets the cell value.
        /// </summary>
        /// <param name="cellValueAsObject">The cell value as object.</param>
        /// <param name="timeSpanAsMinutes">if set to <c>true</c> [time span as minutes].</param>
        /// <param name="andThenToMinutes">if set to <c>true</c> [and then to minutes].</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-05-16
        /// </remarks>
        private static double getCellValue(GridStyleInfo cell, bool timeSpanAsMinutes, bool andThenToMinutes)
        {
            var cellValueAsObject = cell.CellValue;
            if (cell.CellModel is TimeSpanDurationStaticCellModel)
            {
                timeSpanAsMinutes = true;
                andThenToMinutes = true;
            }
            double cellValue;
            if (cellValueAsObject == null || string.IsNullOrEmpty(cellValueAsObject.ToString())) return 0;
         

            if (cellValueAsObject is int)
                cellValue = Convert.ToDouble((int)cellValueAsObject);
            else if (cellValueAsObject is TimeSpan)
            {
                cellValue = ((TimeSpan)cellValueAsObject).TotalSeconds;
                //if (cellValue >= 120)
                if (timeSpanAsMinutes)
                    cellValue = cellValue / 60;
                if (andThenToMinutes)
                    cellValue = cellValue / 60;
            }
            else if (cellValueAsObject is Percent)
                cellValue = ((Percent)cellValueAsObject).Value * 100d;
            else if (cellValueAsObject is double)
                cellValue = (double)cellValueAsObject;
            else
                cellValue = 0;

            if (double.IsNaN(cellValue))
            {
                cellValue = 0;
            }
          
            return cellValue;
        }

        /// <summary>
        /// Inits the specified grid (needs to be done every time we draw new chart).
        /// </summary>
        /// <param name="grid">The grid.</param>
        /// <param name="name">The name.</param>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-05-16
        /// </remarks>
        private void init(GridControl grid, string name)
        {
            if (grid.RightToLeft == RightToLeft.Yes) _chartControl.PrimaryXAxis.Inversed = true;
            _chartControl.PrimaryXAxis.ValueType = ChartValueType.Custom;
            _chartControl.Series.Clear();
            _chartControl.Series.Capacity = grid.ColCount;
            _chartControl.PrimaryXAxis.Range = new MinMaxInfo(0, grid.ColCount-grid.Cols.HeaderCount+1, 0); //One extra for the space to the right in the chart
            _secondaryYAxis.RangeType = ChartAxisRangeType.Auto;
            _chartControl.PrimaryYAxis.RangeType = ChartAxisRangeType.Auto;
            _chartControl.PrimaryXAxis.LabelIntersectAction = ChartLabelIntersectAction.Rotate;
            _chartControl.ChartArea.PrimaryXAxis.HidePartialLabels = false;
            _chartControl.PrimaryXAxis.RangePaddingType = ChartAxisRangePaddingType.None;
            _chartControl.ShowToolTips = true;
            _chartControl.Text = name;
        }

        /// <summary>
        /// Updates the chart settings.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="enabled">if set to <c>true</c> [enabled].</param>
        /// <param name="type">The type.</param>
        /// <param name="axis">The axis.</param>
        /// <param name="color">The color.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-09-23
        /// </remarks>
        public void UpdateChartSettings(GridRow row, bool enabled, ChartSeriesDisplayType type, AxisLocation axis, Color color)
        {
            if (row == null) return;
            row.ChartSeriesSettings.Enabled = enabled;
            row.ChartSeriesSettings.Color = color;
            row.ChartSeriesSettings.SeriesType = type;
            row.ChartSeriesSettings.AxisLocation = axis;
            ReloadChart();
        }

        /// <summary>
        /// Updates the chart settings.
        /// </summary>
        /// <param name="grid">The grid.</param>
        /// <param name="row">The row.</param>
        /// <param name="buttons">The buttons.</param>
        /// <param name="enabled">if set to <c>true</c> [enabled].</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-09-23
        /// </remarks>
        public void UpdateChartSettings(ITaskOwnerGrid grid, GridRow row, GridRowInChartSettingButtons buttons, bool enabled)
        {
            if (row == null) return;
            if (row.ChartSeriesSettings == null) return;

            row.ChartSeriesSettings.Enabled = enabled;
            var test = row.ChartSeriesSettings;
            buttons.SetButtons(test.Enabled, test.AxisLocation, test.SeriesType, test.Color);
            grid.SetRowVisibility(row.DisplayMember, enabled);

            //We only need to reload the chart if the changed grid is the current one
            //if (grid==_currentGrid)
            {
                ReloadChart();
            }
        }

        /// <summary>
        /// Updates the chart settings.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="buttons">The buttons.</param>
        /// <param name="enabled">if set to <c>true</c> [enabled].</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2009-11-13
        /// </remarks>
        public void UpdateChartSettings(GridRow row, GridRowInChartSettingButtons buttons, bool enabled)
        {
            UpdateChartSettings(_currentGrid,row,buttons,enabled);
        }

        public static DateTime GetDateForChartPoint(ChartControl control1, Point point)
        {
            try
            {
                return control1.ChartArea.GetValueByPoint(point).DateX;
            }
            catch (ArgumentException)
            {
                //sometimes we seems to get an error when getting the date but we do not know when.
                //Micke I know I am not supposed to do this... /NH
                return new DateTime(2000, 1, 1, 0, 0, 0);
            }
        }

        public static double GetIntervalValueForChartPoint(ChartControl chartControl, Point chartPoint)
        {

            return chartControl.ChartArea.GetValueByPoint(chartPoint).X;

        }

        /// <summary>
        /// Sets the chart tool tip.
        /// </summary>
        /// <param name="chartRegion">The chart region.</param>
        /// <param name="chartControl">The chart control.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-09-23
        /// </remarks>
        public static void SetChartToolTip(ChartRegion chartRegion, ChartControl chartControl)
        {
			new ChartManager().SetChartToolTip(chartRegion, chartControl);
        }


        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _chartControl.Dispose();
                _secondaryYAxis.Dispose();
                _currentGrid = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}