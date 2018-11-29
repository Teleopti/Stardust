using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Globalization;
using System.Windows.Forms;
using Syncfusion.Drawing;
using Syncfusion.Windows.Forms.Chart;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms.WFControls;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Forecasting;
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms.SeasonPages
{
	 public partial class WorkflowTrendView : BaseUserControl
	 {
		  private IList<IVolumeYear> _volumes;
		  private TaskOwnerPeriod _currentHistoricPeriod;
		  private ChartControl _chartControl;
		  private VolumeTrend _volumeTrend;
		  private readonly IList<ChartSeries> _currentHistoricPeriodSeries = new List<ChartSeries>();

		  private double _originalTrendFactor;
		  private WFSeasonalityTabs _owner;
		  private bool fromPercentBox;
		  private ISkillType _skillType;

		  public WorkflowTrendView()
		  {
				InitializeComponent();
				if (!DesignMode) SetTexts();
		  }

		  protected override void OnLoad(EventArgs e)
		  {
			  _chartControl = new ChartControl {Dock = DockStyle.Fill};
			  base.OnLoad(e);
		  }

		 public void LoadAllComponents(IList<IVolumeYear> volumes, IList<ITaskOwner> historicDepth, WFSeasonalityTabs owner)
		 {
			 _skillType = owner.Presenter.Model.Workload.Skill.SkillType;
			 _volumes = volumes;
			 _owner = owner;
			 splitContainerAdv2.Panel1.Controls.Add(_chartControl);
			 _currentHistoricPeriod = new TaskOwnerPeriod(historicDepth[0].CurrentDate, historicDepth, TaskOwnerPeriodType.Other);
			 setStaticChartRequiremants();
			 drawValuesInChart();
			 _owner.ReportChanges(false);
			 _chartControl.Name = "Trend";
		 }

		  private string getChartSeriesString()
		  {
			var textManager = new TextManager(_skillType);
			return textManager.WordDictionary["ValidatedTasks"];
		  }

		  #region chart methods

		 private void setStaticChartRequiremants()
		 {
			 if (_chartControl == null)
				 MessageBox.Show(this, "Diagnostic: _chartControl is null");
			 _chartControl.BackColor = ColorHelper.ChartControlBackColor();
			 _chartControl.BackInterior = ColorHelper.ChartControlBackInterior();
			 if (_chartControl.PrimaryXAxis == null)
				 MessageBox.Show(this, "Diagnostic: _chartControl.PrimaryXAxis is null");
			 _chartControl.PrimaryXAxis.DrawGrid = true;
			 _chartControl.PrimaryXAxis.HidePartialLabels = false;
			 if (_chartControl.PrimaryYAxis == null)
				 MessageBox.Show(this, "Diagnostic: _chartControl.PrimaryYAxis is null");
			 _chartControl.PrimaryYAxis.HidePartialLabels = false;
			 if (_chartControl.ChartArea == null)
				 MessageBox.Show(this, "Diagnostic: _chartControl.ChartArea is null");
			 _chartControl.ChartArea.BackInterior = ColorHelper.ChartControlChartAreaBackInterior();
			 _chartControl.ChartAreaMargins = ColorHelper.ChartMargins();
			 _chartControl.ChartInterior = ColorHelper.ChartControlChartInterior();
			 _chartControl.ElementsSpacing = 1;
			 _chartControl.ChartArea.AutoScale = true;
			 _chartControl.EnableXZooming = !CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft;
			 _chartControl.EnableYZooming = true;
			 _chartControl.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
			 if (_chartControl.Legend == null)
				 MessageBox.Show(this, "Diagnostic: _chartControl.Legend is null");
			 _chartControl.Legend.BackInterior = ColorHelper.ChartControlBackInterior();
			 _chartControl.Legend.BackColor = ColorHelper.ChartControlBackColor();
			 _chartControl.Legend.VisibleCheckBox = true;
			 _chartControl.LegendPosition = ChartDock.Bottom;
			 _chartControl.LegendAlignment = ChartAlignment.Center;
			 _chartControl.LegendsPlacement = ChartPlacement.Outside;
			 _chartControl.Series3D = false;
			 _chartControl.PrimaryYAxis.ValueType = ChartValueType.Double;
			 _chartControl.PrimaryYAxis.RangeType = ChartAxisRangeType.Auto;
			 if (_chartControl.PrimaryYAxis.LineType == null)
				 MessageBox.Show(this, "Diagnostic: _chartControl.PrimaryYAxis.LineType is null");
			 _chartControl.PrimaryYAxis.LineType.ForeColor = Color.Black;
			 if (_chartControl.PrimaryYAxis.GridLineType == null)
				 MessageBox.Show(this, "Diagnostic: _chartControl.PrimaryYAxis.GridLineType is null");
			 _chartControl.PrimaryYAxis.GridLineType.ForeColor = Color.Black;
			 if (RightToLeft == RightToLeft.Yes) _chartControl.PrimaryXAxis.Inversed = true;
			 _chartControl.TextRenderingHint = TextRenderingHint.AntiAlias;
			 _chartControl.PrimaryXAxis.HidePartialLabels = true;
			 _chartControl.PrimaryXAxis.LabelIntersectAction = ChartLabelIntersectAction.Rotate;
			 _chartControl.PrimaryXAxis.DateTimeFormat = "d";
			 if (_chartControl.PrimaryYAxis.Range == null)
				 MessageBox.Show(this, "Diagnostic: _chartControl.PrimaryYAxis.Range is null");
			 _chartControl.PrimaryYAxis.Range.Min = 0;

			 // ErikS: BUG: 26483
			 // Ugly hack to make syncfusion to work
			 // Throws ArgumentOutOfRangeException if set to DateTime when using ar-SA
			 _chartControl.PrimaryXAxis.ValueType = CultureInfo.CurrentCulture.Calendar.AlgorithmType ==
													CalendarAlgorithmType.SolarCalendar
				 ? ChartValueType.DateTime
				 : ChartValueType.Custom;
			 _chartControl.PrimaryXAxis.TickLabelsDrawingMode = ChartAxisTickLabelDrawingMode.UserMode;
		 }

		 private void setChartRequirements()
		  {
			  int count = _currentHistoricPeriod.TaskOwnerDayCollection.Count;
			  _chartControl.PrimaryYAxis.RangeType = ChartAxisRangeType.Auto;
			  _chartControl.Series.Clear();
			  _chartControl.Series.Capacity = count;
			_chartControl.PrimaryXAxis.DateTimeRange =
				new ChartDateTimeRange(_currentHistoricPeriod.TaskOwnerDayCollection[0].CurrentDate.Date,
										_currentHistoricPeriod.TaskOwnerDayCollection[count - 1].CurrentDate.Date, 10,
										ChartDateTimeIntervalType.Days);
		  }

		 private void drawValuesInChart()
		  {
				//Here we need to create series and add them to a list,
				//this is because if there are historical data with "holes" in it.

				_chartControl.BeginUpdate();
				setChartRequirements();
				_currentHistoricPeriodSeries.Clear();
				_chartControl.Series.Clear();
				IList<double> doubles = new List<double>();
				DateOnly currentDate = DateOnly.MinValue;
				int currentChartSerie = -1;


				foreach (ITaskOwner taskOwner in _currentHistoricPeriod.TaskOwnerDayCollection)
				{
					 ChartPoint chartPoint =
						  new ChartPoint(taskOwner.CurrentDate.Date, taskOwner.TotalStatisticCalculatedTasks);
					 
					 if (currentDate.AddDays(2) < taskOwner.CurrentDate)
					 {
						  ChartSeries historicalSeries = new ChartSeries(getChartSeriesString(), ChartSeriesType.Line);           
						  historicalSeries.Style.Interior = new BrushInfo(Color.FromArgb(46, 66, 124));
						  historicalSeries.Style.Border.Width = 3f;
						  _currentHistoricPeriodSeries.Add(historicalSeries);
						  currentChartSerie++;
					 }
					 _currentHistoricPeriodSeries[currentChartSerie].Points.Add(chartPoint);
					currentDate = taskOwner.CurrentDate;
					 doubles.Add(chartPoint.YValues[0]);

				// ErikS: BUG: 26483
				// Writing out labels manually since syncfusion doesnt
				if (doubles.Count > 0 && doubles.Count % 10 == 0)
					_chartControl.PrimaryXAxis.Labels.Add(new ChartAxisLabel(taskOwner.CurrentDate.Date, "d"));
				}

				ChartSeries chartSeriesTrend = calculateChartSeriesTrend(doubles);

				//Hmm... clear and reset to get the trend line on top?!?
				_chartControl.Series.Clear();
				_chartControl.Series.Add(chartSeriesTrend);
				foreach (ChartSeries chartSeries in _currentHistoricPeriodSeries)
				{
					 _chartControl.Series.Add(chartSeries);     
				}
				_chartControl.PrimaryYAxis.Range.Min = 0;
				_chartControl.PrimaryYAxis.RangeType = ChartAxisRangeType.Auto;

				_chartControl.EndUpdate();

				//Stupid bug fix, implemented here as well as in GridChartManager
				if (_chartControl.PrimaryYAxis.VisibleRange.Interval < 0.001)
				{
					 _chartControl.PrimaryYAxis.VisibleRange.Interval = 1;
				}
		  }

		  private ChartSeries calculateChartSeriesTrend(IList<double> doubles)
		  {
				_volumeTrend = new VolumeTrend(doubles);

				double trendFactor = _volumeTrend.Trend.Value * 100;
				if (trendFactor < int.MinValue)
					 trendFactor = -100;
				trackBarExTrend.Value = Convert.ToInt32(trendFactor);
				percentTextBoxTrend.PercentValue = trendFactor;

				_originalTrendFactor = trendFactor;
				ChartSeries chartSeriesTrend = new ChartSeries(Resources.Trend, ChartSeriesType.Line);
				chartSeriesTrend.Style.Border.Width = 4f;
				_chartControl.Series.Add(chartSeriesTrend);

				int count = _currentHistoricPeriod.TaskOwnerDayCollection.Count;
				var startDate = _currentHistoricPeriod.TaskOwnerDayCollection[0].CurrentDate;
				var endDate = _currentHistoricPeriod.TaskOwnerDayCollection[count - 1].CurrentDate;

				ChartPoint chartPointStart =
					 new ChartPoint(startDate.Date, _volumeTrend.Start.Value);
				chartSeriesTrend.Points.Add(chartPointStart);

				ChartPoint chartPointEnd =
					 new ChartPoint(endDate.Date, _volumeTrend.End.Value);
				chartSeriesTrend.Points.Add(chartPointEnd);
				return chartSeriesTrend;
		  }


		  private void drawValuesInChart(Percent percent)
		  {
				_chartControl.BeginUpdate();
				setChartRequirements();
				_chartControl.Series.Clear();
			  
				_volumeTrend.ChangeTrendLine(percent);
				percentTextBoxTrend.PercentValue = _volumeTrend.Trend.Value*100;

				ChartSeries chartSeriesTrend = new ChartSeries(Resources.Trend, ChartSeriesType.Line);
				chartSeriesTrend.Style.Border.Width = 4f;
				_chartControl.Series.Add(chartSeriesTrend);

				int count = _currentHistoricPeriod.TaskOwnerDayCollection.Count;
				var startDate = _currentHistoricPeriod.TaskOwnerDayCollection[0].CurrentDate;
				var endDate = _currentHistoricPeriod.TaskOwnerDayCollection[count - 1].CurrentDate;

				ChartPoint chartPointStart =
								new ChartPoint(startDate.Date, _volumeTrend.Start.Value);
				chartSeriesTrend.Points.Add(chartPointStart);

				ChartPoint chartPointEnd =
								new ChartPoint(endDate.Date, _volumeTrend.End.Value);
				chartSeriesTrend.Points.Add(chartPointEnd);

				//Hmm... clear and reset to get the trend line on top?!?
				_chartControl.Series.Clear();
				_chartControl.Series.Add(chartSeriesTrend);

				foreach (ChartSeries chartSeries in _currentHistoricPeriodSeries)
				{
					 _chartControl.Series.Add(chartSeries);
				}

				_chartControl.EndUpdate();

				//Stupid bug fix, implemented here as well as in GridChartManager
				if (_chartControl.PrimaryYAxis.VisibleRange.Interval < 0.001)
				{
					 _chartControl.PrimaryYAxis.VisibleRange.Interval = 1;
				}
		  }

		  #endregion

		  #region trend changes
		  //Demofrossa!!! Trackbaren är integer baserad och percent boxen double
		  //för att kunna sätta decimaler i procentboxen får inte valuchanged eventet på trackbaren köras 
		  //efter värdet i percentboxen har ändrats (23,5 -> 24)
		  //fick använda en variabel för detta (fromPercentBox)?!?.

		  private void trackBarExTrendValueChanged(object sender, EventArgs e)
		  {
				if (!fromPercentBox)
				{
					 percentTextBoxTrend.PercentValue = trackBarExTrend.Value;
					 drawValuesInChart(new Percent(percentTextBoxTrend.PercentValue/100));
					 updateTrendValues();
				}
				fromPercentBox = false;
		  }

		  private void percentTextBoxTrendKeyDown(object sender, KeyEventArgs e)
		  {
				if (e.KeyCode == Keys.Enter)
				{
					 fromPercentBox = true;
					 trackBarExTrend.Value = Convert.ToInt32(percentTextBoxTrend.PercentValue);
					 drawValuesInChart(new Percent(percentTextBoxTrend.PercentValue / 100));
					 updateTrendValues();
				}
		  }

		  private void percentTextBoxTrendLeave(object sender, EventArgs e)
		  {
				fromPercentBox = true;
				trackBarExTrend.Value = Convert.ToInt32(percentTextBoxTrend.PercentValue);
				drawValuesInChart(new Percent(percentTextBoxTrend.PercentValue / 100));
				updateTrendValues();
		  }

		  private void xxResetYearlyTrendClick(object sender, EventArgs e)
		  {
				fromPercentBox = true;
				percentTextBoxTrend.PercentValue = _originalTrendFactor;
				trackBarExTrend.Value = Convert.ToInt32(percentTextBoxTrend.PercentValue);
				drawValuesInChart(new Percent(percentTextBoxTrend.PercentValue / 100));
				updateTrendValues();
		  }

		  private void updateTrendValues()
		  {
				_owner.SetTrendValues(new Percent(percentTextBoxTrend.PercentValue / 100), checkBoxAdvUseTrend.Checked);
				_owner.ReportChanges(true);
		  }

		  private void checkBoxAdvUseTrendCheckStateChanged(object sender, EventArgs e)
		  {
				updateTrendValues();
		  }

		  #endregion

		  private void splitContainerAdv2DoubleClick(object sender, EventArgs e)
		  {
				splitContainerAdv2.Panel2Collapsed = !splitContainerAdv2.Panel2Collapsed;
		  }

		  private void unhookEvents()
		  {
			trackBarExTrend.ValueChanged -= trackBarExTrendValueChanged;
			checkBoxAdvUseTrend.CheckStateChanged -= checkBoxAdvUseTrendCheckStateChanged;
				percentTextBoxTrend.KeyDown -= percentTextBoxTrendKeyDown;
				percentTextBoxTrend.Leave -= percentTextBoxTrendLeave;
		  }

		  private void releaseManagedResources()
		  {
				_owner = null;
				if (_chartControl != null)
				{
					 _chartControl.Dispose();
				}
				if (_currentHistoricPeriodSeries != null)
				{
					 _currentHistoricPeriodSeries.Clear();
				}
				_currentHistoricPeriod = null;
				_skillType = null;
				_volumeTrend = null;
				if (_volumes != null)
				{
					 _volumes.Clear();
				}
		  }
	 }
}