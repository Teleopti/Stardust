using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Cells;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Rows;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Chart;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Rows;
using Teleopti.Ccc.Win.Forecasting.Forms;
using Teleopti.Ccc.WinCode.Common.Chart;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public class BaseIntradayGridControl : TeleoptiGridControl, ITaskOwnerGrid
    {
        #region Fields

        private ITaskOwner _taskOwnerDay;
        private readonly IList<IntervalDefinition> _intervals = new List<IntervalDefinition>();
        private IList<IGridRow> _gridRows;
        private readonly TaskOwnerHelper _taskOwnerPeriodHelper;
        private AbstractDetailView _owner;
        private readonly int _resolution;
        private ContextMenu _menuMergeSplit;
        private MenuItem _menuItemSplit;
        private MenuItem _menuItemMerge;
        private MenuItem _menuItemSaveAsTemplate;
        private MenuItem _menuItemModifySelection;
        private readonly TimeZoneInfo _timeZone;
        private bool _timeSpanAsMinutes;
        private readonly ChartSettings _chartSettings;

        private IList<double> _modifiedItems;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the task owner day.
        /// </summary>
        /// <value>The task owner day.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-02-22
        /// </remarks>
        protected ITaskOwner TaskOwnerDay
        {
            get { return _taskOwnerDay; }
            set { _taskOwnerDay = value; }
        }

        /// <summary>
        /// Gets the intervals.
        /// </summary>
        /// <value>The intervals.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-02-22
        /// </remarks>
        protected IList<IntervalDefinition> Intervals
        {
            get { return _intervals; }
        }

        /// <summary>
        /// Gets the task owner helper.
        /// </summary>
        /// <value>The task owner helper.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-02-22
        /// </remarks>
        protected TaskOwnerHelper TaskOwnerHelper
        {
            get { return _taskOwnerPeriodHelper; }
        }

        /// <summary>
        /// Gets the grid rows.
        /// </summary>
        /// <value>The grid rows.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-02-22
        /// </remarks>
        protected IList<IGridRow> GridRows
        {
            get { return _gridRows; }
        }

        /// <summary>
        /// Gets the time zone.
        /// </summary>
        /// <value>The time zone.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-02-24
        /// </remarks>
        protected TimeZoneInfo TimeZone
        {
            get { return _timeZone; }
        }

        #endregion

        #region Constructor

        internal BaseIntradayGridControl(ITaskOwner taskOwnerDay, TaskOwnerHelper taskOwnerPeriodHelper,
            TimeZoneInfo timeZone, int resolution, AbstractDetailView owner, bool timeSpanAsMinutes, ChartSettings chartSettings)
        {
            _owner = owner;
            _taskOwnerDay = taskOwnerDay;
            _taskOwnerPeriodHelper = taskOwnerPeriodHelper;
            _resolution = resolution;
            _timeZone = timeZone;
            _timeSpanAsMinutes = timeSpanAsMinutes;
            _chartSettings = chartSettings;

			if (_owner != null)
				_owner.ChangeUnsavedDaysStyle += _owner_ChangeUnsavedDaysStyle;
	        TeleoptiStyling = true;
        }

        #endregion

        #region Declared events

        public event EventHandler<ModifyCellEventArgs> ModifyCells;

        protected void TriggerModifyCells(ModifyCellOption option, IEnumerable dataPeriodList)
        {
        	var handler = ModifyCells;
            if (handler != null)
            {
                ModifyCellEventArgs modifyCellEventArgs = new ModifyCellEventArgs(option, dataPeriodList);
                handler.Invoke(this, modifyCellEventArgs);
            }
        }

        public event EventHandler<ModifyRowEventArgs> ModifyRows;

        protected void TriggerModifyRows(TaskPeriodType type)
        {
        	var handler = ModifyRows;
            if (handler != null)
            {
                ModifyRowEventArgs modifyRowEventArgs = new ModifyRowEventArgs(type);
                handler.Invoke(this, modifyRowEventArgs);
            }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Crates an inits this class, did thos to get rid of the 
        /// strange calls to virtual methods in the constructor
        /// </summary>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-05-12
        /// </remarks>
        public virtual void Create()
        {
            OnCreating();
            CreateGridRows();
            InitializeGrid();
            OnCreated();
		}

        protected virtual void OnCreating()
        {
        }

        protected virtual void OnCreated()
        {
            //Adds the possibility not to do this call! :)
            UpdateDataPeriodList();
        }

        protected virtual void InitializeGrid()
        {
            CellModels.Add("NumericCellNoDecimal", InitializeCallNumericCellNoDecimal());
            CellModels.Add("NumericCellOneDecimal", InitializeCallNumericCellOneDecimal());
            CellModels.Add("NumericCellVariableDecimals", InitializeCallNumericCellVariableDecimals());


            Model.Options.MergeCellsMode = GridMergeCellsMode.OnDemandCalculation | GridMergeCellsMode.MergeColumnsInRow | GridMergeCellsMode.MergeRowsInColumn;

            RowCount = GridRows.Count - 1;
            ColCount = _intervals.Count + 1;
            Cols.HeaderCount = 1;
            Cols.SetFrozenCount(1, false);

            if (_taskOwnerDay != null)
            {
                Rows.HeaderCount = 2;
                Rows.SetFrozenCount(2, false);
            }

            BaseStylesMap["Header"].StyleInfo.HorizontalAlignment = GridHorizontalAlignment.Center;

            _menuMergeSplit = new ContextMenu();
            _menuItemMerge = new MenuItem(UserTexts.Resources.Merge, MergeOnClick);
            _menuItemSplit = new MenuItem(UserTexts.Resources.Split, SplitOnClick);
            _menuItemModifySelection = new MenuItem(UserTexts.Resources.ModifySelection, ModifySelectionOnClick);
            _menuItemSaveAsTemplate = new MenuItem(UserTexts.Resources.SaveAsTemplate, SaveAsTemplateOnClick);
            _menuItemSplit.Enabled = false;
            _menuItemMerge.Enabled = false;
            _menuItemModifySelection.Enabled = false;
            _menuItemSaveAsTemplate.Enabled = false;
            _menuMergeSplit.MenuItems.Add(_menuItemMerge);
            _menuMergeSplit.MenuItems.Add(_menuItemSplit);
            _menuMergeSplit.MenuItems.Add(_menuItemModifySelection);
            _menuMergeSplit.MenuItems.Add(_menuItemSaveAsTemplate);
            ContextMenu = _menuMergeSplit;
        }



        private NumericCellModel InitializeCallNumericCellNoDecimal()
        {
            NumericCellModel cellModel = new NumericCellModel(Model);
            cellModel.NumberOfDecimals = 0; //Get this from... ?
            return cellModel;
        }

        private NumericCellModel InitializeCallNumericCellOneDecimal()
        {
            NumericCellModel cellModel = new NumericCellModel(Model);
            cellModel.NumberOfDecimals = 1;
            return cellModel;
        }

        private NumericCellModel InitializeCallNumericCellVariableDecimals()
        {
            NumericCellModel cellModel = new NumericCellModel(Model);
            cellModel.NumberOfDecimals = CurrentForecasterSettings().NumericCellVariableDecimals;
            return cellModel;
        }

        protected virtual void CreateGridRows()
        {
            var date = (_taskOwnerDay != null) ? _taskOwnerDay.CurrentDate : DateOnly.MinValue;
            IList<DateOnly> dates = new List<DateOnly> { date };

            _gridRows = new List<IGridRow>();
            GridRows.Add(new DateHeaderGridRow(DateHeaderType.Date, dates));
            GridRows.Add(new DateHeaderGridRow(DateHeaderType.WeekdayName, dates));
            GridRows.Add(new IntervalHeaderGridRow(_intervals));
		}

        private void _owner_ChangeUnsavedDaysStyle(object sender, CustomEventArgs<IUnsavedDaysInfo> e)
    	{
			_unsavedDays = e.Value;
    	}

    	protected override void OnQueryCellInfo(GridQueryCellInfoEventArgs e)
        {
            if (e.ColIndex < 0 || e.RowIndex < 0) return;
            if (e.RowIndex >= GridRows.Count) return;

            if (e.Style.CellIdentity == null) return;
            GridRows[e.RowIndex].QueryCellInfo(GetCellInfo(e.Style, e.ColIndex, e.RowIndex));


            if (e.ColIndex == 1 && e.RowIndex == 0)
            {
                StringBuilder infoText = new StringBuilder();
                if (_taskOwnerDay != null)
                {
                    if (_taskOwnerDay is ISkillDay)
                    {
                        infoText.Append(UserTexts.Resources.Skill);
                    }
                    else
                    {
                        infoText.Append(UserTexts.Resources.Workload);
                    }
                    infoText.Append(" - ");
                }

                infoText.Append(UserTexts.Resources.Intraday);
                e.Style.CellValue = infoText.ToString();
            }

			if (_taskOwnerDay != null)
				changeUnsavedDaysStyle(_taskOwnerDay.CurrentDate);
            e.Handled = true;
        }

		private void changeUnsavedDaysStyle(DateOnly localCurrentDate)
		{
			if (_unsavedDays == null) return;
			if (_unsavedDays.ContainsDateTime(localCurrentDate))
				BackColor = ColorHelper.UnsavedDayColor;
			else BackColor = ColorHelper.DialogBackColor();
		}
        #endregion

        #region Merge handling events

        protected override void OnLeftColChanged(GridRowColIndexChangedEventArgs e)
        {
            base.OnLeftColChanged(e);
            RefreshRange(ViewLayout.VisibleCellsRange);
        }

        protected override void OnDrawCell(GridDrawCellEventArgs e)
        {
            base.OnDrawCell(e);
            if (e.ColIndex >= RowHeaderCount)
            {
                if (GridRows.Count <= e.RowIndex) return;
                if (GridRows[e.RowIndex] is DateHeaderGridRow)
                {
                    Rectangle visibleHeaderRect = ViewLayout.RangeInfoToRectangle(GridRangeInfo.Row(e.RowIndex), true, GridCellSizeKind.VisibleSize);
                    if (visibleHeaderRect.Width > ClientRectangle.Width - (ColWidths[0] + ColWidths[1]))
                    {
                        e.Renderer.DrawSingleCell(e.Graphics, visibleHeaderRect, e.RowIndex, ViewLayout.LastVisibleCol, e.Style, false);
                        e.Cancel = true;
                    }
                    return;
                }

                GridRangeInfo currentRange = CoveredRanges.FindRange(e.RowIndex, e.ColIndex);
                if (currentRange != null &&
                    currentRange.RangeType != GridRangeInfoType.Empty)
                {
                    Rectangle visibleCellRect = ViewLayout.RangeInfoToRectangle(currentRange, true, GridCellSizeKind.ActualSize);
                    int rightX = visibleCellRect.Right;
                    visibleCellRect.Width = Math.Max(visibleCellRect.Width / 10, ColWidths[e.ColIndex]); // Align text to sort of left aligned.
                    if (RightToLeft == RightToLeft.Yes) visibleCellRect.Location = new Point(rightX - visibleCellRect.Width, visibleCellRect.Y);
                    e.Renderer.DrawSingleCell(e.Graphics, visibleCellRect, e.RowIndex, e.ColIndex, e.Style, false);
                    e.Cancel = true;
                }
            }
        }

        protected override void OnSelectionChanged(GridSelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);
            _menuItemMerge.Enabled = false;
            _menuItemSplit.Enabled = false;
            if (((e.Range.IsCells ||
                e.Range.IsCols) &&
                e.Range.Left < e.Range.Right)
                || e.Range.IsRows)
                _menuItemMerge.Enabled = true;
            if ((e.Range.IsCells ||
                e.Range.IsCols ||
                e.Range.IsRows) &&
                CoveredRanges.Ranges.GetRangesContained(e.Range).Count > 0)
                _menuItemSplit.Enabled = true;
            /*bool enableMenu;
            GridHelper.ModifySelectionEnabled(this, out _modifiedItems, out enableMenu);
            if(enableMenu)*/
            _menuItemModifySelection.Enabled = false;
            if (((e.Range.IsCells ||
                e.Range.IsCols))
                || e.Range.IsRows)
                _menuItemModifySelection.Enabled = true;

            if (e.Range.Left > Cols.HeaderCount &&
                _owner != null)
            {
                _owner.CurrentTimeOfDay = _intervals[e.Range.Left - (Cols.HeaderCount + 1)].TimeSpan;
            }
        }

        private void MergeOnClick(object sender, EventArgs e)
        {
            MergeSplit(ModifyCellOption.Merge);
            OnSelectionChanged(new GridSelectionChangedEventArgs(Selections.Ranges[0], Selections.Ranges, GridSelectionReason.SelectRange));
            InitializeAllGridRowsToChart();
        }

        private void SplitOnClick(object sender, EventArgs e)
        {
            MergeSplit(ModifyCellOption.Split);
            OnSelectionChanged(new GridSelectionChangedEventArgs(Selections.Ranges[0], Selections.Ranges, GridSelectionReason.SelectRange));
            InitializeAllGridRowsToChart();
        }

        private void ModifySelectionOnClick(object sender, EventArgs e)
        {
            var modifySelectedList = _modifiedItems;
            var numbers = new ModifyCalculator(modifySelectedList);
            IList<double> receivedValues;
            using (ModifySelectionView modifySelection = new ModifySelectionView(numbers))
            {
                if (modifySelection.ShowDialog(this) != DialogResult.OK) return;
                receivedValues = modifySelection.ModifiedList;

                for (var i = 0; i < receivedValues.Count; i++)
                {
                     if (double.IsNaN(receivedValues[i]) || double.IsInfinity(receivedValues[i]))
                         receivedValues[i] = 0.0;
                }
            }
            GridHelper.ModifySelectionInput(this, receivedValues);
        }

        protected virtual void SaveAsTemplateOnClick(object sender, EventArgs e)
        {
        }

        protected override void OnShowContextMenu(Syncfusion.Windows.Forms.ShowContextMenuEventArgs e)
        {
            bool enableMenu;
            _modifiedItems = new List<double>();
            _modifiedItems.Clear();
            GridHelper.ModifySelectionEnabled(this, out _modifiedItems, out enableMenu);
            ContextMenu.MenuItems[2].Enabled = enableMenu;
            base.OnShowContextMenu(e);
        }

        #endregion

        bool _lockUpdate;
    	private IUnsavedDaysInfo _unsavedDays;
	    private bool _resize = true;

        protected override void OnSaveCellInfo(GridSaveCellInfoEventArgs e)
        {
            base.OnSaveCellInfo(e);

            if (e.ColIndex <= 0 || e.RowIndex < 0) return;
            if (e.RowIndex >= GridRows.Count) return;

            GridRows[e.RowIndex].SaveCellInfo(GetCellInfo(e.Style, e.ColIndex, e.RowIndex));
            if (!_lockUpdate)
            {
                GridRangeInfo range = GridRangeInfo.Col(e.ColIndex);
                if (_owner != null)
                {
                    _owner.TriggerValuesChanged();
                    if (_owner.SkillType.ForecastSource != ForecastSource.InboundTelephony && _owner.SkillType.ForecastSource!=ForecastSource.Retail)
                        range = GridRangeInfo.Table();
                }
                RefreshRange(range, true);
                TriggerDataToChart(GridRangeInfo.Cells(e.RowIndex, Cols.HeaderCount + 1, e.RowIndex, ColCount));
            }
            e.Handled = true;
        }

        protected override void OnQueryRowCount(GridRowColCountEventArgs e)
        {
            base.OnQueryRowCount(e);

            e.Count = GridRows.Count - 1;
            e.Handled = true;
        }

        protected override void OnQueryColCount(GridRowColCountEventArgs e)
        {
            base.OnQueryColCount(e);

            e.Count = _intervals.Count + Cols.HeaderCount;
            e.Handled = true;
        }

        protected override void OnAfterPaste()
        {
            _lockUpdate = false;
            if (_taskOwnerPeriodHelper != null)
            {
            	_taskOwnerPeriodHelper.EndUpdate();
            }
			if (_owner!=null)
			{
				_owner.TriggerValuesChanged();
			}
            InitializeAllGridRowsToChart();
        }

        protected override void OnBeforePaste()
        {
            if (_taskOwnerPeriodHelper != null) _taskOwnerPeriodHelper.BeginUpdate();
            _lockUpdate = true;
        }

        protected virtual void MergeSplit(ModifyCellOption options)
        {
        }

        /// <summary>
        /// Refreshes the grid.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-02-22
        /// </remarks>
        public void RefreshGrid()
        {
            UpdateDataPeriodList();
            Refresh();
        }

        /// <summary>
        /// Refreshes the grid without resizing the cells.
        /// </summary>
        /// <remarks>
        /// Created by: marias
        /// Created date: 2015-02-09
        /// </remarks>
        public void RefreshGridNoResize()
        {
	        _resize = false;	
			RefreshGrid();
        }

        protected virtual void OnUpdatingDataPeriodList()
        {
        }

        protected virtual void OnUpdatedDataPeriodList()
        {
			if(_resize)
				this.ResizeToFit();
            Cols.Size[1] = ColorHelper.GridHeaderColumnWidth();
            ResetGridCoveredRanges();
        }

        protected void UpdateDataPeriodList()
        {
            OnUpdatingDataPeriodList();
            OnUpdatedDataPeriodList();
        }

        protected void CreateIntervalList(DateTime firstTime, DateTime lastTime)
        {
            _intervals.Clear();
            if (firstTime == lastTime) return;

            var intervals = new DateTimePeriod(firstTime, lastTime).IntervalsFromHourCollection(_resolution, _timeZone);
            foreach (IntervalDefinition intervalDefinition in intervals)
            {
                _intervals.Add(intervalDefinition);
            }
        }

        protected void ResetGridCoveredRanges()
        {
            ResetVolatileData();
            CoveredRanges.Clear();
            CoveredRanges.Ranges.Clear();
            CoveredRanges.UpdateList();
            ColCount = _intervals.Count + Cols.HeaderCount;
            
            Model.MergeCells.DelayMergeCells(GridRangeInfo.Table());
        }

        #region Interface ITaskOwnerGrid

        /// <summary>
        /// Gets a value indicating whether this instance has columns.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance has columns; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-02-13
        /// </remarks>
        public bool HasColumns
        {
            get { return _intervals.Count > 0; }
        }

        /// <summary>
        /// Gets or sets the owner.
        /// </summary>
        /// <value>The owner.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-28
        /// </remarks>
        public AbstractDetailView Owner
        {
            get { return _owner; }
            set { _owner = value; }
        }

        /// <summary>
        /// Goes to date.
        /// </summary>
        /// <param name="theDate">The date.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-25
        /// </remarks>
        public void GoToDate(DateOnly theDate)
        {
            if (_taskOwnerDay == null ||
                _taskOwnerDay.CurrentDate != theDate)
            {
                RefreshGrid();
            }
        }

        public DateOnly GetLocalCurrentDate(int column)
        {
            int count = _intervals.Count;
            if (count == 0)
                return DateOnly.MaxValue;
            DateOnly returnDate;
            if (count == 0 && _intervals.Count == 0) return TaskOwnerDay.CurrentDate;//the f**cked up stuff downstairs is fixed by this 
            if (column > count)
                returnDate = TaskOwnerDay.CurrentDate.Add(_intervals[count - 1].TimeSpan);
            else if (column <= Cols.HeaderCount)
                returnDate = TaskOwnerDay.CurrentDate.Add(_intervals[0].TimeSpan);
            else
                returnDate = TaskOwnerDay.CurrentDate.Add(_intervals[column - (Cols.HeaderCount + 1)].TimeSpan);//this is f**cked up when grid & chart is zero - why they are is another question

            return returnDate;
        }

        public IDictionary<int, GridRow> EnabledChartGridRows
        {
            get
            {
                IDictionary<int, GridRow> settings = (from r in GridRows.OfType<GridRow>()
                                                      where r.ChartSeriesSettings != null &&
                                                            r.ChartSeriesSettings.Enabled
                                                      select r).ToDictionary(k => GridRows.IndexOf(k), v => v);

                return settings;
            }
        }

        public ReadOnlyCollection<GridRow> AllGridRows
        {
            get
            {
                if (GridRows == null) return new ReadOnlyCollection<GridRow>(new List<GridRow>());
                return new ReadOnlyCollection<GridRow>(new List<GridRow>(GridRows.OfType<GridRow>()));
            }
        }

        /// <summary>
        /// Gets the main header row index.
        /// </summary>
        /// <value>The main header row.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-05-15
        /// </remarks>
        public int MainHeaderRow
        {
            get { return 2; }
        }

        public virtual IList<GridRow> EnabledChartGridRowsMicke65()
        {
            IDictionary<int, GridRow> settings = EnabledChartGridRows;
            return new List<GridRow>(settings.Values);

        }

        public void SetRowVisibility(string key, bool enabled)
        {
            if (enabled)
                _chartSettings.SelectedRows.Add(key);
            else
            {
                _chartSettings.SelectedRows.Remove(key);
            }
        }

        protected IChartSeriesSetting ConfigureSetting(string key)
        {
            IChartSeriesSetting ret = _chartSettings.DefinedSetting(key, new ChartSettingsManager().ChartSettingsDefault);
            ret.Enabled = _chartSettings.SelectedRows.Contains(key);
            return ret;
        }

        /// <summary>
        /// Goes to time.
        /// </summary>
        /// <param name="theTime">The time.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-14
        /// </remarks>
        public void GoToTime(TimeSpan theTime)
        {
            var item = _intervals.FirstOrDefault(i => i.TimeSpan == theTime);
            int index = -1;
            if (item != null)
                index = _intervals.IndexOf(item);
            if (index >= 0)
            {
                Model.ScrollCellInView(GridRangeInfo.Col(index + 1), GridScrollCurrentCellReason.MoveTo);
            }
        }

        #endregion

        public override WorkingInterval WorkingInterval
        {
            get { return WorkingInterval.Intraday; }
        }

        protected int Resolution
        {
            get { return _resolution; }
        }

        /// <summary>
        /// Gets the resolution.
        /// Only for use with chart! This is not the default resolution for skill!
        /// </summary>
        /// <value>The resolution.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-17
        /// </remarks>
        public override TimeSpan ChartResolution
        {
            get { return TimeSpan.FromMinutes(60); }
        }

        public override DateTime FirstDate
        {
            get
            {
                if (_intervals.Count == 0) return SkillDayTemplate.BaseDate.Date;
                if (_taskOwnerDay != null)
                {
                    return TimeZoneInfo.ConvertTimeFromUtc(_intervals[0].DateTime, _timeZone);
                }

                //Template usage...
                return SkillDayTemplate.BaseDate.Date.Add(_intervals[0].TimeSpan);
            }
        }

        public override DateTime LastDate
        {
            get
            {
                if (_intervals.Count == 0) return SkillDayTemplate.BaseDate.Date;
                if (_taskOwnerDay != null)
                {
                    return TimeZoneInfo.ConvertTimeFromUtc(_intervals.Last().DateTime.Add(ChartResolution), _timeZone);
                }

                //Template usage...
                return SkillDayTemplate.BaseDate.Date.Add(_intervals.Last().TimeSpan.Add(ChartResolution));
            }
        }

        protected ISettingData SettingData { get; set; }

        protected override IDictionary<DateTime, double> GetRowDataForChart(GridRangeInfo gridRangeInfo)
        {
            var colHeaders = Cols.HeaderCount + 1;
            IDictionary<DateTime, double> keyValueCollection = new Dictionary<DateTime, double>();

            for (var i = 0; i <= ColCount; i++)
            {
                var cell = Model[gridRangeInfo.Top, i];
                if (cell.BaseStyle == "Header") continue;
                var cellValueAsObject = cell.CellValue;
                if (cellValueAsObject is string) continue;
                double cellValue;

				if (cellValueAsObject == null)
					cellValue = double.NegativeInfinity;
                else if (cellValueAsObject is int)
                    cellValue = Convert.ToDouble((int)cellValueAsObject);
                else if (cellValueAsObject is TimeSpan)
                {
                    cellValue = ((TimeSpan)cellValueAsObject).TotalSeconds;
                    //if (cellValue >= 120)
                    if (_timeSpanAsMinutes)
                        cellValue = cellValue / 60;
                }
                else if (cellValueAsObject is Percent)
                    cellValue = ((Percent)cellValueAsObject).Value * 100d;
                else
                    cellValue = (double)cellValueAsObject;

                var coveredRanges = CoveredRanges.Ranges.GetRangesContaining(GridRangeInfo.Cell(gridRangeInfo.Top, i));
                if (coveredRanges.Count > 0 &&
                    !coveredRanges[0].IsEmpty)
                {
                    var gridRow = GridRows[gridRangeInfo.Top] as GridRow;
                    if (gridRow != null)
                        switch (gridRow.DisplayMember)
                        {
                            case "Tasks":
                            case "TotalTasks":
                            case "TotalStatisticCalculatedTasks":
                            case "TotalStatisticAbandonedTasks":
                            case "TotalStatisticAnsweredTasks":
                                cellValue = cellValue / coveredRanges[0].Width;
                                break;
                        }
                }

                keyValueCollection.Add(TimeZoneInfo.ConvertTimeFromUtc(_intervals[i - colHeaders].DateTime, _timeZone), cellValue);
            }

            return keyValueCollection;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                if (_menuMergeSplit != null)
                    _menuMergeSplit.Dispose();
                _owner = null;
                if (_gridRows != null)
                    _gridRows.Clear();
            }
        }
    }
}
