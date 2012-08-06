using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Globalization;
using System.Windows.Forms;
using Syncfusion.Drawing;
using Syncfusion.Windows.Forms.Chart;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.Win.Common.Controls.Chart;
using Teleopti.Ccc.Win.Common.Controls.DateSelection;
using Teleopti.Ccc.Win.Forecasting.Forms.WFControls;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Forecasting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Forecasting.Forms.SeasonPages
{
    public partial class WorkflowDataSummaryView : BaseUserControl
    {
        private WFSeasonalityTabs _owner;
        private readonly ChartControl _chartControl;
        private string _taskTypeName;

        private WorkflowDataSummaryView()
        {
            InitializeComponent();
            if (!DesignMode) SetTexts();
        }

        public WorkflowDataSummaryView(WFSeasonalityTabs owner) : this()
        {
            _owner = owner;
            DecideTaskName();
            
            SetColors();     
            _chartControl = new ChartControl();
            splitContainerAdv1.Panel1.Controls.Add(_chartControl);
            _chartControl.Dock = DockStyle.Fill;
            setChartRequirements();
            reloadSeasonData();
        }

		private void DecideTaskName()
		{
			var manager = new TextManager(_owner.Presenter.Model.Workload.Skill.SkillType);
			_taskTypeName = manager.WordDictionary["Tasks"];
		}

    	private void dateSelectionComposite1_DateRangeChanged(object sender, DateRangeChangedEventArgs e)
        {
            IList<DateOnlyPeriod> periodList = new List<DateOnlyPeriod>(e.SelectedDates);
            _owner.ReloadHistoricalDataDepth(periodList);
            reloadSeasonData();
        }
        
        private void SetColors()
        {
            BrushInfo brushInfo = ColorHelper.ControlGradientPanelBrush();
            //gradientPanel1.BackgroundColor = brushInfo;
            gradientPanel2.BackgroundColor = brushInfo;
            splitContainerAdv1.Panel1.BackgroundColor = brushInfo;
            splitContainerAdv1.Panel2.BackgroundColor = brushInfo;
            xpTaskBar1.BackColor = ColorHelper.XPTaskbarBackgroundColor();
            xpTaskBar1.ThemesEnabled = false;
            xpTaskBarBoxHistoricalDepth.HeaderBackColor = ColorHelper.XPTaskbarHeaderColor();
            //xpTaskBarBox2.HeaderBackColor = GuiHelper.XpTaskBarHeaderColor();
        }

        #region chart methods
        private void setChartRequirements()
        {
            int count = 365;
            _chartControl.Series.Clear();
            _chartControl.Series.Capacity = count;
            _chartControl.BackColor = ColorHelper.ChartControlBackColor();
            _chartControl.BackInterior = ColorHelper.ChartControlBackInterior();
            _chartControl.PrimaryXAxis.DrawGrid = false;
            _chartControl.PrimaryXAxis.HidePartialLabels = false;
            _chartControl.PrimaryYAxis.HidePartialLabels = false;
            _chartControl.ChartArea.BackInterior = ColorHelper.ChartControlChartAreaBackInterior();
            _chartControl.ChartAreaMargins = new ChartMargins(60, 15, 15, 25);
            _chartControl.ChartInterior = ColorHelper.ChartControlChartInterior();
            _chartControl.ElementsSpacing = 1;
            _chartControl.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            _chartControl.ChartArea.AutoScale = true;
            _chartControl.PrimaryXAxis.Range = new MinMaxInfo(1, count, 1);
            _chartControl.Legend.BackInterior = ColorHelper.ChartControlBackInterior();
            _chartControl.Legend.BackColor = ColorHelper.ChartControlBackColor();
            _chartControl.Legend.VisibleCheckBox = true;
            _chartControl.LegendPosition = ChartDock.Bottom;
            _chartControl.LegendAlignment = ChartAlignment.Center;
            _chartControl.LegendsPlacement = ChartPlacement.Outside;

            _chartControl.Series3D = false;
            _chartControl.TextRenderingHint = TextRenderingHint.AntiAlias;
            _chartControl.PrimaryYAxis.ValueType = ChartValueType.Double;
            _chartControl.PrimaryYAxis.RangeType = ChartAxisRangeType.Auto;
            _chartControl.PrimaryYAxis.LineType.ForeColor = Color.Black;
            _chartControl.PrimaryYAxis.GridLineType.ForeColor = Color.Black;
            if (RightToLeft == RightToLeft.Yes) _chartControl.PrimaryXAxis.Inversed = true;

            _chartControl.PrimaryXAxis.ValueType = ChartValueType.Custom;
        }

        //Cannot figuer out how to handle days in the grid and months on the label
        //without getting day resolution on the gridlines, therefor I have to draw
        //something called striplines
        private void addStripLines()
        {
            _chartControl.PrimaryXAxis.StripLines.Clear();
            ChartStripLine sl = new ChartStripLine();

            sl.Enabled = true;
            sl.Vertical = true;
            sl.StartAtAxisPosition = true;
            sl.Offset = 0;
            sl.Width = 0.3;
            sl.Period = 30.5;
            sl.Text = "";
            sl.Interior = new BrushInfo(100, new BrushInfo(GradientStyle.None, Color.Black, Color.Black));

            //Add the stripline to the X Axis.
            _chartControl.PrimaryXAxis.StripLines.Add(sl);
        }

        private void reloadSeasonData()
        {
            Cursor = Cursors.WaitCursor;

            var taskOwnerPeriods = _owner.Presenter.CreateYearTaskOwnerPeriods();
           
            _chartControl.PrimaryYAxis.RangeType = ChartAxisRangeType.Auto;
            _chartControl.BeginUpdate();
            var xAxisLabels = new string[365];          
            _chartControl.Series.Clear();


            createPrimaryAxisLabels(xAxisLabels);

            if (taskOwnerPeriods.Count == 0)
                clearChartDefaultJunkData();
            else
                drawValuesInChart(taskOwnerPeriods);
            
            _chartControl.PrimaryXAxis.LabelsImpl = new LabelModel(xAxisLabels);
            _chartControl.EndUpdate();
            _chartControl.PrimaryXAxis.HidePartialLabels = false;
            _chartControl.PrimaryYAxis.Range.Min = 0;
            setYAxisRange();
            Cursor = Cursors.Default;
        }



        //TODO:Fix suppresson
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void createPrimaryAxisLabels(string[] xAxisLabels)
        {
            int month = 1;
            int accumulatedMonthPosition = 0;
            foreach (string monthName in CultureInfo.CurrentUICulture.DateTimeFormat.MonthNames)
            {
                //TODO how do we handle this?, The MonthNames collection seem to have 13 months last one is "" (other cultures?)
                //and it crashes on GetDaysInMonth if we are in a culture that only has 12 months
                //But I guess there is cultures with more then 12 months 
                try
                {  
                    int numberOfDaysinMonth = CultureInfo.CurrentCulture.Calendar.GetDaysInMonth(2007, month);  
                    int position = accumulatedMonthPosition;// -(numberOfDaysinMonth / 2);
                    accumulatedMonthPosition += numberOfDaysinMonth;
                    addStripLines();
                    
                    xAxisLabels.SetValue(StringHelper.Capitalize(monthName),position+1);
                }
                catch (Exception){}
                month++;
            }
        }

        private void drawValuesInChart(IList<TaskOwnerPeriod> taskOwnerPeriods)
        {
            ChartSeries chartSeries;
            _chartControl.Legend.Visible = true;
            foreach (TaskOwnerPeriod taskOwnerPeriod in taskOwnerPeriods)
            {
                int year = CultureInfo.CurrentCulture.Calendar.GetYear(taskOwnerPeriod.CurrentDate);
                chartSeries = new ChartSeries(string.Concat(_taskTypeName, " ", year.ToString(CultureInfo.CurrentCulture)), ChartSeriesType.Line);
                chartSeries.Style.Border.Width = 2f;
                _chartControl.Series.Add(chartSeries);

                foreach (ITaskOwner taskOwner in taskOwnerPeriod.TaskOwnerDayCollection)
                {
                    ChartPoint chartPoint =
                        new ChartPoint(taskOwner.CurrentDate.Date.DayOfYear, taskOwner.TotalStatisticCalculatedTasks);
                    chartSeries.Points.Add(chartPoint);
                }
            }
        }
        private void setYAxisRange()
        {

            if (_chartControl.PrimaryYAxis.Range.NumberOfIntervals > 50)
            {
                _chartControl.PrimaryYAxis.Range.Interval = Math.Round( _chartControl.PrimaryYAxis.Range.Max / 10 ,0);
            }
        }



        //This method exists cause the default data in the grid is showing when clearing the series?!?
        private void clearChartDefaultJunkData()
        {
            _chartControl.Legend.Visible = false;
            var chartSeries = new ChartSeries("");
            chartSeries.Points.Add(0, 0);
            _chartControl.Series.Add(chartSeries);
        }

        #endregion
        #region events
        private void splitContainerAdv1_DoubleClick(object sender, EventArgs e)
        {
            splitContainerAdv1.Panel2Collapsed = !splitContainerAdv1.Panel2Collapsed;

        }
        #endregion

        private void UnhookEvents()
        {
            dateSelectionCompositeHistoricalPeriod.DateRangeChanged -= dateSelectionComposite1_DateRangeChanged;
            splitContainerAdv1.DoubleClick -= splitContainerAdv1_DoubleClick;
        }

        private void ReleaseManagedResources()
        {
            _owner = null;
            if (_chartControl != null)
            {
                _chartControl.Dispose();
            }
        }
    }
}