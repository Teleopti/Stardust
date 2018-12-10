using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Chart;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.DateSelection;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms.WFControls;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Chart;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms.SeasonPages
{
	public partial class WorkflowSeasonView : BaseUserControl
	{
		private WFSeasonalityTabs _owner;
		private VolumeYear _volumeYear;
		private ChartControl _chartControl;
		private SeasonVolumeGridControl _seasonVolumeGridControl;
		private ChartAxis _secondaryYAxis;

		private WorkflowSeasonView()
		{
			InitializeComponent();
			if (!DesignMode) SetTexts();
		}

		public WorkflowSeasonView(VolumeYear volumeYear, IList<DateOnlyPeriod> preselectedDates, WFSeasonalityTabs owner)
			: this()
		{
			_owner = owner;
			_volumeYear = volumeYear;

			_seasonVolumeGridControl = new SeasonVolumeGridControl(volumeYear, this);
			_seasonVolumeGridControl.CellsChanged += _seasonVolumeGridControl_CellsChanged;
			_seasonVolumeGridControl.ClipboardPaste += _seasonVolumeGridControl_Paste;
			_seasonVolumeGridControl.Dock = DockStyle.Fill;
			splitContainerAdv2.Panel2.Controls.Add(_seasonVolumeGridControl);

			_chartControl = new ChartControl();
			_chartControl.AddRandomSeries = false;
			splitContainerAdv2.Panel1.Controls.Add(_chartControl);
			_chartControl.Dock = DockStyle.Fill;

			dateSelectionComposite1.AddSelectedDates(preselectedDates);
			setChartRequirements();
			reloadSeasonData();
			_chartControl.ChartRegionClick += _chartControl_ChartRegionClick;
		}

		public WFSeasonalityTabs Owner => _owner;

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			_seasonVolumeGridControl.SetupColumnWidths();
		}

		private void _seasonVolumeGridControl_Paste(object sender, GridCutPasteEventArgs e)
		{
			reloadSeasonData();
		}

		void _seasonVolumeGridControl_CellsChanged(object sender, GridCellsChangedEventArgs e)
		{
			reloadSeasonData();
		}


		#region Chart methods

		private void setChartRequirements()
		{
			int count = _volumeYear.PeriodTypeCollection.Count;
			_chartControl.Series.Clear();
			_chartControl.Series.Capacity = count;
			_chartControl.BackColor = ColorHelper.ChartControlBackColor();
			_chartControl.BackInterior = ColorHelper.ChartControlBackInterior();

			_chartControl.ChartArea.BackInterior = ColorHelper.ChartControlChartAreaBackInterior();
			_chartControl.ChartAreaMargins = ColorHelper.ChartMargins();
			_chartControl.ChartInterior = ColorHelper.ChartControlChartInterior();
			_chartControl.ElementsSpacing = 1;
			//_chartControl.Font = new Font("Verdana", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);

			_chartControl.ChartArea.AutoScale = false;
			_chartControl.PrimaryXAxis.Range = new MinMaxInfo(1, count, 1);
			_chartControl.Legend.BackInterior = ColorHelper.ChartControlBackInterior();
			_chartControl.Legend.BackColor = ColorHelper.ChartControlBackColor();
			_chartControl.Legend.VisibleCheckBox = true;
			_chartControl.LegendPosition = ChartDock.Bottom;
			_chartControl.LegendAlignment = ChartAlignment.Center;
			_chartControl.LegendsPlacement = ChartPlacement.Outside;

			_chartControl.Series3D = false;
			_chartControl.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
			_chartControl.PrimaryYAxis.ValueType = ChartValueType.Double;
			_chartControl.PrimaryYAxis.RangeType = ChartAxisRangeType.Auto;
			_chartControl.PrimaryYAxis.LineType.ForeColor = Color.Black;
			
			_chartControl.PrimaryYAxis.GridLineType.ForeColor = Color.Gray;

			if (RightToLeft == RightToLeft.Yes) _chartControl.PrimaryXAxis.Inversed = true;
			_chartControl.TextRenderingHint = TextRenderingHint.AntiAlias;
			_secondaryYAxis = new ChartAxis();
			_secondaryYAxis.LineType.ForeColor = Color.Gray;
			_secondaryYAxis.DrawGrid = true;
			_secondaryYAxis.GridLineType.ForeColor = Color.Gray;
			_secondaryYAxis.OpposedPosition = true; // will be positioned opposite the regular axis
			_secondaryYAxis.Orientation = ChartOrientation.Vertical;
			_secondaryYAxis.LabelIntersectAction = ChartLabelIntersectAction.Rotate;
			_secondaryYAxis.ValueType = ChartValueType.Double;
			_secondaryYAxis.RangeType = ChartAxisRangeType.Auto;
			_secondaryYAxis.LocationType = ChartAxisLocationType.Auto;
			
			_chartControl.PrimaryYAxis.LocationType = ChartAxisLocationType.Auto;

			_chartControl.Axes.Add(_secondaryYAxis);
			_chartControl.PrimaryXAxis.ValueType = ChartValueType.Custom;
			_chartControl.PrimaryYAxis.HidePartialLabels = true;
		}

		//This looks a bit primitive (it is!), might need to be breaken out to
		//a dynamic control later, it just loops through the grid and creates series for the chart
		private void reloadSeasonData()
		{
			_chartControl.PrimaryYAxis.RangeType = ChartAxisRangeType.Auto;
			_secondaryYAxis.RangeType = ChartAxisRangeType.Auto;

			_chartControl.BeginUpdate();
			string[] xAxisLabels = new string[_volumeYear.PeriodTypeCollection.Count + 1];
			ChartSeries secondSeries = null;
			ChartSeries chartSeries = null;

			_chartControl.Series.Clear();

			//Loop through the number of rows in the grid
			for (int i = 1; i < _seasonVolumeGridControl.RowCount + 1; i++)
			{
				//Create only 3 series
				if (i == (int)SeasonVolumeGridControl.GridRowTypes.AverageTasks)
				{
					chartSeries =
						new ChartSeries((string)_seasonVolumeGridControl[i, 0].CellValue + " (<)", ChartSeriesType.Line);
					chartSeries.Style.Border.Width = 3f;
					_chartControl.Series.Add(chartSeries);
				}
				if (i == (int)SeasonVolumeGridControl.GridRowTypes.AverageTalkTime)
				{
					secondSeries =
						new ChartSeries((string)_seasonVolumeGridControl[i, 0].CellValue + " (>)", ChartSeriesType.Line);
					secondSeries.Style.Border.Width = 3f;
					_chartControl.Series.Add(secondSeries);
				}
				if (i == (int)SeasonVolumeGridControl.GridRowTypes.AverageAfterWorkTime)
				{
					secondSeries =
						new ChartSeries((string)_seasonVolumeGridControl[i, 0].CellValue + " (>)", ChartSeriesType.Line);
					secondSeries.Style.Border.Width = 3f;
					_chartControl.Series.Add(secondSeries);
				}

				for (int j = 1; j < _seasonVolumeGridControl.ColCount + 1; j++)
				{
					string columnheaderName = (string)_seasonVolumeGridControl[0, j].CellValue;
					xAxisLabels.SetValue(columnheaderName, j - 1);

					ChartPoint chartPoint;
					switch (i)
					{
						case (int)SeasonVolumeGridControl.GridRowTypes.AverageTasks:
							chartPoint = new ChartPoint(j, (double)_seasonVolumeGridControl[i, j].CellValue);
							chartSeries?.Points.Add(chartPoint);
							break;
						case (int)SeasonVolumeGridControl.GridRowTypes.AverageTalkTime:
						case (int)SeasonVolumeGridControl.GridRowTypes.AverageAfterWorkTime:
							chartPoint = _seasonVolumeGridControl.SkillType.DisplayTimeSpanAsMinutes
								? new ChartPoint(j,
									((TimeSpan) _seasonVolumeGridControl[i, j].CellValue).TotalSeconds / 60)
								: new ChartPoint(j, ((TimeSpan) _seasonVolumeGridControl[i, j].CellValue).TotalSeconds);
							secondSeries?.Points.Add(chartPoint);
							break;
					}
				}
				if (secondSeries != null) secondSeries.YAxis = _secondaryYAxis;
			}
			_chartControl.PrimaryXAxis.LabelsImpl = new LabelModel(xAxisLabels);
			_chartControl.EndUpdate();

			_chartControl.PrimaryYAxis.Range.Min = 0;
	 
			_secondaryYAxis.Range.Min = 0;
			_chartControl.PrimaryYAxis.ForceZero = true;
			
			//These are special cases ***** 
			//The chart sometimes get Crazy and adds like 200000 labels to Y axis ?!?
			//Might need to talk to Syncfusion about this, looks like a bug anyway.
			if (_chartControl.PrimaryYAxis.Range.Interval < 2)
			{
				_chartControl.PrimaryYAxis.Range.Interval = Math.Truncate(_chartControl.PrimaryYAxis.Range.Max / 5);
				_chartControl.PrimaryYAxis.Range.Max = _chartControl.PrimaryYAxis.Range.Max + _chartControl.PrimaryYAxis.Range.Interval;
			}

			if (_secondaryYAxis.Range.Interval < 0.1)
			{
				_secondaryYAxis.Range.Interval = Math.Truncate(_secondaryYAxis.Range.Max / 5);
				_secondaryYAxis.Range.Max = _secondaryYAxis.Range.Max + _secondaryYAxis.Range.Interval;
			}

			if (_chartControl.PrimaryYAxis.Range.NumberOfIntervals > 7)
			{
				_chartControl.PrimaryYAxis.Range.Interval = _chartControl.PrimaryYAxis.Range.Interval * 4;
			}
			if (_chartControl.PrimaryYAxis.Range.NumberOfIntervals > 100)
			{
				_chartControl.PrimaryYAxis.Range.Interval = _chartControl.PrimaryYAxis.Range.Interval * 100;
			}
			//*******
		   
		}

		#endregion

		private void dateSelectionCompositeHistoricalPeriod_DateRangeChanged(object sender, DateRangeChangedEventArgs e)
		{
			Owner.ReloadHistoricalDataDepth(_volumeYear, e.SelectedDates);
			_seasonVolumeGridControl.Refresh();
			reloadSeasonData();
		}

		public void ReportChanges(bool changes)
		{
			Owner.ReportChanges(changes);
		}


		void _chartControl_ChartRegionClick(object sender, ChartRegionMouseEventArgs e)
		{
			int column = Math.Max(GetIntervalValueForChartPoint(e.Point),1);
			_seasonVolumeGridControl.Model.ScrollCellInView(GridRangeInfo.Col(column),
															GridScrollCurrentCellReason.MoveTo);
		}

		private int GetIntervalValueForChartPoint(Point chartPoint)
		{
			double xValueWithinXAxis = chartPoint.X - _chartControl.PrimaryXAxis.Rect.X;
			double xAxisLength = _chartControl.PrimaryXAxis.Rect.Width;

			double xValueFactor = 0d;
			if (_seasonVolumeGridControl.ColCount != 0)
				xValueFactor = xAxisLength / _seasonVolumeGridControl.ColCount;

			int convertedXValue = 0;
			if (xValueFactor != 0)
				convertedXValue = Convert.ToInt32(xValueWithinXAxis / xValueFactor);

			return convertedXValue;
		}

		private void splitContainerAdv1_DoubleClick(object sender, EventArgs e)
		{
			splitContainerAdv1.Panel2Collapsed = !splitContainerAdv1.Panel2Collapsed;
		}

		private void UnhookEvents()
		{
			splitContainerAdv1.DoubleClick -= splitContainerAdv1_DoubleClick;
			dateSelectionComposite1.DateRangeChanged -= dateSelectionCompositeHistoricalPeriod_DateRangeChanged;
		}

		private void ReleaseManagedResources()
		{
			_secondaryYAxis?.Dispose();
			_owner = null;
			_volumeYear = null;
			_chartControl?.Dispose();
			_seasonVolumeGridControl?.Dispose();
			_seasonVolumeGridControl = null;
		}
	}
}