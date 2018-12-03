using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Rows;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Rows;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Forecasting;
using Teleopti.Ccc.Win.Forecasting.Forms;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms.WFControls
{
	public class WorkloadDayFilterGridControl : TeleoptiGridControl
	{
		private readonly IWorkload _workload;
		private readonly IWorkloadDayTemplate _workloadDayTemplate;
		private readonly ISkillType _skillType;
		private IList<IGridRow> _gridRows;
		private IList<DateOnly> _dateTimes;
		private readonly TimeZoneInfo _timeZone;
		private readonly RowManager<TaskOwnerDayFilterGridRow, WorkloadDayWithFilterStatus> _rowManagerTaskOwner;
		private readonly IDictionary<DateOnly, Outlier> _outliers = new Dictionary<DateOnly, Outlier>();
		private IList<WorkloadDayWithFilterStatus> _workloadDays;
		private string _gridRowShowInChart = string.Empty;
		private readonly IList<int> _excludedColumns = new List<int>();
		private int _templateIndex;
		private readonly DateTime _firstTime;
		private readonly DateTime _lastTime;
		private readonly TextManager _manager;

		public WorkloadDayFilterGridControl(IWorkload workload, IWorkloadDayTemplate workloadDayTemplate)
		{
			if (workload == null) throw new ArgumentNullException(nameof(workload));
			if (workloadDayTemplate == null) throw new ArgumentNullException(nameof(workloadDayTemplate));
			_workload = workload;
			_workloadDayTemplate = workloadDayTemplate;
			_skillType = workload.Skill.SkillType;
			_timeZone = workload.Skill.TimeZone;
			_rowManagerTaskOwner = new RowManager<TaskOwnerDayFilterGridRow, WorkloadDayWithFilterStatus>(this, null, -1);
			_manager = new TextManager(_skillType);
			_firstTime = _workloadDayTemplate.TaskPeriodListPeriod.StartDateTime;
			_lastTime = _workloadDayTemplate.TaskPeriodListPeriod.EndDateTime;
			TeleoptiStyling = true;
		}

		public void LoadTaskOwnerDaysWithFilterStatus(WorkloadDayTemplateFilterStatus filterStatus)
		{
			if (filterStatus == null) throw new ArgumentNullException(nameof(filterStatus));
			var workloadDays = filterStatus.WorkloadDaysWithFilterStatus;
			_dateTimes = workloadDays.Select(v => v.WorkloadDay.CurrentDate).ToList();
			_workloadDays = workloadDays.ToList();
			_templateIndex = filterStatus.TemplateIndex;
			CreateGrid();
			_rowManagerTaskOwner.SetDataSource(workloadDays);
			InitializeGrid();
			InitializeExcludedColumns();
			Recalculate();
		}

		private void InitializeExcludedColumns()
		{
			for (var i = 1; i < _workloadDays.Count; i++)
				if (!_workloadDays[i].Included)
					_excludedColumns.Add(i + ColHeaderCount);
		}

		public void InitializeChart()
		{
			InitializeAllGridColumnsToChart();
		}
		
		protected override bool IsChartRow(int rowIndex)
		{
			return rowIndex != 1;
		}

		private void InitializeGrid()
		{
			TableStyle.CheckBoxOptions = new GridCheckBoxCellInfo("True", "False", string.Empty, false);
			Model.Options.MergeCellsMode = GridMergeCellsMode.OnDemandCalculation | GridMergeCellsMode.MergeColumnsInRow;
			Cols.Size[0] = ColorHelper.GridHeaderColumnWidth();
			DefaultColWidth = 80;
			RowCount = _gridRows.Count - 1;
			ColCount = _dateTimes.Count;
			BaseStylesMap["Header"].StyleInfo.HorizontalAlignment = GridHorizontalAlignment.Center;
			ExcelLikeCurrentCell = true;
			Rows.SetFrozenCount(1, false);
            Cols.SetFrozenCount(1, false);
			Model.MergeCells.DelayMergeCells(GridRangeInfo.Table());
			Refresh();
		}

		private void CreateGrid()
		{
			_gridRows = new List<IGridRow>();
			_gridRows.Add(new DateHeaderGridRow(DateHeaderType.Date, _dateTimes));

			CreateGridRows();

			SelectionChanged += WorkloadDayFilterGridControl_SelectionChanged;
		}

		private void WorkloadDayFilterGridControl_SelectionChanged(object sender, GridSelectionChangedEventArgs e)
		{
			if (!IsChartRow(e.Range.Top)) return;
			var rowHeaderName = GetRowHeaderName(e.Range.Top) as string;
			if(!string.IsNullOrEmpty(rowHeaderName))
			{
				if(rowHeaderName.Equals(GridRowShowInChart))
					return;
				GridRowShowInChart = rowHeaderName;
			}
		}

		private void CreateGridRows()
		{
			

			var gridRow = new TaskOwnerDayFilterGridRow(_rowManagerTaskOwner,
															"CheckBox", "Included",
															UserTexts.Resources.Include);
			gridRow.WorkloadDayIncludedChanged += gridRow_WorkloadDayIncludedChanged;
			_gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

			if (_skillType.ForecastSource != ForecastSource.InboundTelephony)
			{
				gridRow = new TaskOwnerDayFilterGridRow(_rowManagerTaskOwner,
														"NumericWorkloadDayTaskLimitedCell",
														"WorkloadDay.TotalStatisticCalculatedTasks",
														_manager.WordDictionary["Tasks"]);
				_gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

				gridRow = new TaskOwnerDayFilterGridRow(_rowManagerTaskOwner, "PositiveTimeSpanTotalSecondsCell",
														"WorkloadDay.TotalStatisticAverageTaskTime", _manager.WordDictionary["AverageTaskTime"]);
				_gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

				gridRow = new TaskOwnerDayFilterGridRow(_rowManagerTaskOwner, "PositiveTimeSpanTotalSecondsCell",
														"WorkloadDay.TotalStatisticAverageAfterTaskTime", _manager.WordDictionary["AverageAfterTaskTime"]);
				_gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));
			}
			else
			{
				gridRow = new TaskOwnerDayFilterGridRow(_rowManagerTaskOwner,
													"NumericWorkloadDayTaskLimitedCell",
													"WorkloadDay.TotalStatisticCalculatedTasks",
													_manager.WordDictionary["Tasks"]);
				_gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

				gridRow = new TaskOwnerDayFilterGridRow(_rowManagerTaskOwner, "NumericCell",
														"WorkloadDay.TotalStatisticAbandonedTasks",
														_manager.WordDictionary["TotalStatisticAbandonedTasks"]);
				_gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

				gridRow = new TaskOwnerDayFilterGridRow(_rowManagerTaskOwner, "PositiveTimeSpanTotalSecondsCell",
														"WorkloadDay.TotalStatisticAverageTaskTime", _manager.WordDictionary["AverageTaskTime"]);
				_gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

				gridRow = new TaskOwnerDayFilterGridRow(_rowManagerTaskOwner, "PositiveTimeSpanTotalSecondsCell",
														"WorkloadDay.TotalStatisticAverageAfterTaskTime", _manager.WordDictionary["AverageAfterTaskTime"]);
				_gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));
			}
		}

		private void gridRow_WorkloadDayIncludedChanged(object sender, WorkloadDayIncludedInTemplateEventArgs e)
		{
			if (e.CheckValue)
				_excludedColumns.Remove(e.ColIndex);
			else
				_excludedColumns.Add(e.ColIndex);
			Recalculate();
			Refresh();
			InitializeAllGridColumnsToChart();
		}

		private double GetIntervalValueFromRowName(IWorkloadDay workloadDay, TimeSpan interval, string rowName)
		{
			var taskPeriod = workloadDay.TaskPeriodList.FirstOrDefault(t => t.Period.StartDateTimeLocal(_timeZone).TimeOfDay == interval);
			if (taskPeriod == null) return 0d;

			if (rowName.Equals(_manager.WordDictionary["Tasks"]))
				return taskPeriod.StatisticTask.StatCalculatedTasks;
			if (rowName.Equals(_manager.WordDictionary["TotalStatisticAbandonedTasks"]))
				return taskPeriod.StatisticTask.StatAbandonedTasks;
			if (rowName.Equals(_manager.WordDictionary["AverageTaskTime"]))
				return taskPeriod.StatisticTask.StatAverageTaskTimeSeconds;
			if (rowName.Equals(_manager.WordDictionary["AverageAfterTaskTime"]))
				return taskPeriod.StatisticTask.StatAverageAfterTaskTimeSeconds;
			throw new ArgumentException("Row name is not matched.");
		}

		protected override void OnQueryCellInfo(GridQueryCellInfoEventArgs e)
		{
			base.OnQueryCellInfo(e);

			if (e.ColIndex < 0 || e.RowIndex < 0) return;
			if (e.ColIndex > _dateTimes.Count) return;
			if (e.ColIndex == 1 && e.RowIndex == 0)
			{
				e.Style.CellValue = UserTexts.Resources.Template;
                e.Handled = true;
				return;
			}
			if (e.ColIndex > 1)
			{
				formatCell(e.Style, _dateTimes[e.ColIndex - RowHeaderCount]);
			}
			_gridRows[e.RowIndex].QueryCellInfo(GetCellInfo(e.Style, e.ColIndex, e.RowIndex));

			e.Handled = true;
		}

		private void formatCell(GridStyleInfo gridStyleInfo, DateOnly dateTime)
		{
			bool headerCell = (gridStyleInfo.BaseStyle == "Header");
            weekendOrWeekday(gridStyleInfo, dateTime, !headerCell, headerCell);
		}

		private void weekendOrWeekday(GridStyleInfo gridStyleInfo, DateOnly date, bool backColor, bool textColor)
		{
			if (DateHelper.IsWeekend(date, CultureInfo.CurrentCulture))
			{
				if (backColor)
					gridStyleInfo.BackColor = ColorHolidayCell;
				if (textColor)
					gridStyleInfo.TextColor = ColorHolidayHeader;
			}
			else
			{
				if (backColor)
				{
					gridStyleInfo.BackColor = BackColor;
				}
				gridStyleInfo.TextColor = ForeColor;
			}

			Outlier outlier;
			if (_outliers.TryGetValue(date, out outlier))
			{
				gridStyleInfo.CellTipText = outlier.Description.Name;
				gridStyleInfo.BackColor = ColorHelper.GridControlOutlierColor();
			}
		}

		public override WorkingInterval WorkingInterval => WorkingInterval.Intraday;

		public override TimeSpan ChartResolution => TimeSpan.FromMinutes(60);

		public string GridRowShowInChart
		{
			get
			{
				return string.IsNullOrEmpty(_gridRowShowInChart) ? _manager.WordDictionary["Tasks"] : _gridRowShowInChart;
			}
			set {
				if(_gridRowShowInChart.Equals(value)) return;
				if (value != null) _gridRowShowInChart = value;
				if (_gridRowShowInChart.Equals(_manager.WordDictionary["TotalStatisticAbandonedTasks"]))
					_gridRowShowInChart = _manager.WordDictionary["Tasks"];
                Enabled = false;
                Cursor.Current = Cursors.WaitCursor;
				TriggerFilterDataSelectionChanged(new FilterDataSelectionChangedEventArgs(_gridRowShowInChart));
                Cursor.Current = Cursors.Default;
                Enabled = true;
			}
		}

		protected override void OnQueryRowCount(GridRowColCountEventArgs e)
		{
			base.OnQueryRowCount(e);

			e.Count = _gridRows.Count - 1;
			e.Handled = true;
		}

		protected override void OnQueryColCount(GridRowColCountEventArgs e)
		{
			base.OnQueryColCount(e);

			e.Count = _dateTimes.Count;
			e.Handled = true;
		}

		protected override void OnResizingColumns(GridResizingColumnsEventArgs e)
		{
			base.OnResizingColumns(e);
			if (e.Reason == GridResizeCellsReason.DoubleClick)
			{
				ColWidths.ResizeToFit(Selections.Ranges[0], GridResizeToFitOptions.IncludeCellsWithinCoveredRange);
				e.Cancel = true;
			}
		}

		protected override void OnSaveCellInfo(GridSaveCellInfoEventArgs e)
		{
			base.OnSaveCellInfo(e);

			if (e.ColIndex <= 1 || e.RowIndex < 0) return;
			_gridRows[e.RowIndex].SaveCellInfo(GetCellInfo(e.Style, e.ColIndex, e.RowIndex));
			RefreshRange(GridRangeInfo.Col(e.ColIndex), true);

			e.Handled = true;
		}

		protected IDictionary<DateTime, double> GetColumnDataForChart(GridRangeInfo gridRangeInfo)
		{
			if (gridRangeInfo == null) throw new ArgumentNullException(nameof(gridRangeInfo));
			var keyValueCollection = new Dictionary<DateTime, double>();
			
			var currentCellDate = _dateTimes[gridRangeInfo.Left - 1];
			var currentCellTaskOwnerDay = _workloadDays.First(d => d.WorkloadDay.CurrentDate == currentCellDate);
			var currentCellWorkloadDay = currentCellTaskOwnerDay.WorkloadDay as IWorkloadDay;
			if (currentCellWorkloadDay == null) return keyValueCollection;
			var intervals = new DateTimePeriod(_firstTime, _lastTime).IntervalsFromHourCollection(currentCellWorkloadDay.Workload.Skill.DefaultResolution, _timeZone);
			for (var rowIndex = 1; rowIndex <= RowCount; rowIndex++)
			{
				if (!GetRowHeaderName(rowIndex).Equals(GridRowShowInChart)) continue;
				for(var intervalIndex = 0; intervalIndex < intervals.Count; intervalIndex++)
				{
                    var key = TimeZoneInfo.ConvertTimeFromUtc(intervals[intervalIndex].DateTime, _timeZone);
					var	value = GetIntervalValueFromRowName(currentCellWorkloadDay, key.TimeOfDay, GridRowShowInChart);
					if (!keyValueCollection.ContainsKey(key))
						keyValueCollection.Add(key, value);
				}
			}
			return keyValueCollection;
		}

		private string GetColumnHeaderName(GridRangeInfo gridRangeInfo)
		{
			//the first column is Template column
			if (gridRangeInfo.Left == 1 && gridRangeInfo.Right == 1)
				return UserTexts.Resources.Template;
			var currentCellDate = _dateTimes[gridRangeInfo.Left - 1];
            return currentCellDate.ToShortDateString();
		}

		private object GetRowHeaderName(int rowIndex)
		{
			return this[rowIndex, 0].CellValue;
		}

		public int TemplateIndex => _templateIndex;

		public override DateTime FirstDate => TimeZoneInfo.ConvertTimeFromUtc(_firstTime, _timeZone);

		public override DateTime LastDate => TimeZoneInfo.ConvertTimeFromUtc(_lastTime, _timeZone);

		public event EventHandler<FilterDataToChartEventArgs> FilterDataToChart;

		protected void TriggerFilterDataToChart(GridRangeInfo gridRangeInfo)
		{
			if (gridRangeInfo == null) throw new ArgumentNullException(nameof(gridRangeInfo));

			var handler = FilterDataToChart;
			if (handler != null)
			{
			    bool removeColumn = _excludedColumns.Contains(gridRangeInfo.Left);
			    var eventArgs = new FilterDataToChartEventArgs(
					GetColumnHeaderName(gridRangeInfo),
					gridRangeInfo.Left, GetColumnDataForChart(gridRangeInfo), removeColumn);
				handler.Invoke(this, eventArgs);
			}
		}

		private void Recalculate()
		{
			var templateDayWithStatus = _workloadDays.First();
			var templateDay = new Statistic(_workload).GetTemplateWorkloadDay(_workloadDayTemplate,
			                                                                  _workloadDays.Skip(1).Where(d => d.Included).Select(
			                                                                  	d => d.WorkloadDay).ToList());
			templateDayWithStatus.WorkloadDay = templateDay;
		}

		public event EventHandler<FilterDataSelectionChangedEventArgs> FilterDataSelectionChanged;

		public void TriggerFilterDataSelectionChanged(FilterDataSelectionChangedEventArgs e)
		{
			FilterDataSelectionChanged?.Invoke(this, e);
		}
		
		public void InitializeAllGridColumnsToChart()
		{
		    Enabled = false;
		    Cursor.Current = Cursors.WaitCursor;
			for (var i = RowHeaderCount; i <= ColCount; i++)
			{
				var range = GridRangeInfo.Cells(ColHeaderCount, i, RowCount, i);
				TriggerFilterDataToChart(range);
			}
            Cursor.Current = Cursors.Default;
		    Enabled = true;
		}
		
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (disposing)
			{
				if (_rowManagerTaskOwner != null)
				{
					_rowManagerTaskOwner.DataSource.Clear();
					_rowManagerTaskOwner.Rows.Clear();
				}
			}
		}
	}

	public class FilterDataSelectionChangedEventArgs : EventArgs
	{
		private readonly string _selectedRow;

		public FilterDataSelectionChangedEventArgs(string selectedRow)
		{
			_selectedRow = selectedRow;
		}
	}
}
