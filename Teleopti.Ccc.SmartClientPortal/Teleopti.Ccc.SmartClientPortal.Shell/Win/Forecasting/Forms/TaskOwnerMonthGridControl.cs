using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Rows;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Chart;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Rows;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Forecasting.Forms;
using Teleopti.Ccc.WinCode.Common.Chart;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms
{
    public class TaskOwnerMonthGridControl : TeleoptiGridControl, ITaskOwnerGrid
    {
        private readonly IList<DateOnly> _dateTimes;
        private AbstractDetailView _owner;
        private readonly TaskOwnerHelper _taskOwnerPeriodHelper;
        private IList<IGridRow> _gridRows;
        private readonly RowManager<ITaskOwnerGridRow, ITaskOwner> _rowManagerTaskOwner;
        private readonly ChartSettings _chartSettings;

        private IList<double> _modifiedItems;
        
        public TaskOwnerMonthGridControl(IEnumerable<ITaskOwner> taskOwnerPeriods, AbstractDetailView owner, ChartSettings chartSettings)
        {
            _owner = owner;
            _chartSettings = chartSettings;
            _dateTimes = taskOwnerPeriods.Select(tp => tp.CurrentDate).ToList();
            _rowManagerTaskOwner = new RowManager<ITaskOwnerGridRow, ITaskOwner>(this, null, -1);
            _rowManagerTaskOwner.SetDataSource(taskOwnerPeriods.ToList());
            _taskOwnerPeriodHelper = new TaskOwnerHelper(taskOwnerPeriods);

            createGridRows();
            initializeGrid();
            CreateContextMenu();
			TeleoptiStyling = true;
        }

        public void CreateContextMenu()
        {
            var gridItemModify = new MenuItem(Resources.ModifySelection, ModifySelectionOnClick);
            var menu = new ContextMenu();
            menu.MenuItems.Add(gridItemModify);
            gridItemModify.Enabled = true;
            ContextMenu = menu;
        }

        private void ModifySelectionOnClick(object sender, EventArgs e)
        {
            var modifySelectedList = _modifiedItems;
            var numbers = new ModifyCalculator(modifySelectedList);
            var modifySelection = new ModifySelectionView(numbers);
            if (modifySelection.ShowDialog(this) != DialogResult.OK) return;
            var receivedValues = modifySelection.ModifiedList;
            GridHelper.ModifySelectionInput(this, receivedValues);
        }

        protected override void OnShowContextMenu(Syncfusion.Windows.Forms.ShowContextMenuEventArgs e)
        {
            bool enableMenu;
            _modifiedItems = new List<double>();
            _modifiedItems.Clear();
            GridHelper.ModifySelectionEnabled(this, out _modifiedItems, out enableMenu);
            ContextMenu.MenuItems[0].Enabled = enableMenu;
            base.OnShowContextMenu(e);
        }

        #region InitializeGrid

        private IChartSeriesSetting configureSetting(string key)
        {
            IChartSeriesSetting ret = _chartSettings.DefinedSetting(key, new ChartSettingsManager().ChartSettingsDefault);
            ret.Enabled = _chartSettings.SelectedRows.Contains(key);
            return ret;
        }

        private void createGridRows()
        {
            _gridRows = new List<IGridRow>();
            _gridRows.Add(new DateHeaderGridRow(DateHeaderType.Year, _dateTimes));
            _gridRows.Add(new DateHeaderGridRow(DateHeaderType.MonthName, _dateTimes));

            if (_owner.TargetType == TemplateTarget.Workload)
            {
				if (_owner.SkillType.ForecastSource != ForecastSource.InboundTelephony && _owner.SkillType.ForecastSource != ForecastSource.Chat)
                    CreateWorkloadNonTelephonyRows();
                else
                    CreateWorkloadTelephonyRows();
            }
            else if (_owner.TargetType == TemplateTarget.Skill)
            {
				if (_owner.SkillType.ForecastSource != ForecastSource.InboundTelephony && _owner.SkillType.ForecastSource != ForecastSource.Chat)
                    CreateSkillNonTelephonyRows();
                else
                    CreateSkillTelephonyRows();
            }
        }



        private void CreateSkillTelephonyRows()
        {
			var manager = GetManager(_owner);
            ITaskOwnerGridRow gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "NumericReadOnlyCell",
				"TotalTasks", manager.WordDictionary["TotalTasks"], Resources.Forecasted, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "TimeSpanTotalSecondsReadOnlyCell",
				"TotalAverageTaskTime", manager.WordDictionary["TotalAverageTaskTime"], Resources.Forecasted, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "TimeSpanTotalSecondsReadOnlyCell",
				"TotalAverageAfterTaskTime", manager.WordDictionary["TotalAverageAfterTaskTime"], Resources.Forecasted, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "TimeSpanReadOnlyCell",
				"ForecastedIncomingDemand", manager.WordDictionary["ForecastedIncomingDemand"], Resources.Forecasted, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "TimeSpanReadOnlyCell",
				"ForecastedIncomingDemandWithShrinkage", manager.WordDictionary["ForecastedIncomingDemandWithShrinkage"], Resources.Forecasted, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));
        }

        private void CreateSkillNonTelephonyRows()
        {
            var manager = GetManager(_owner);
            
            ITaskOwnerGridRow gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "NumericReadOnlyCell", 
				"TotalTasks", manager.WordDictionary["TotalTasks"], Resources.Forecasted, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "TimeSpanLongHourMinutesStaticCellModel", 
				"TotalAverageTaskTime", manager.WordDictionary["TotalAverageTaskTime"], Resources.Forecasted, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "TimeSpanLongHourMinutesStaticCellModel", 
				"TotalAverageAfterTaskTime", manager.WordDictionary["TotalAverageAfterTaskTime"], Resources.Forecasted, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "TimeSpanLongHourMinutesStaticCellModel",
				"ForecastedIncomingDemand", manager.WordDictionary["ForecastedIncomingDemand"], Resources.Forecasted, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "TimeSpanLongHourMinutesStaticCellModel",
				"ForecastedIncomingDemandWithShrinkage", manager.WordDictionary["ForecastedIncomingDemandWithShrinkage"], Resources.Forecasted, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));
        }

        private void CreateWorkloadTelephonyRows()
        {
			var manager = GetManager(_owner);
            ITaskOwnerGridRow gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner,
                                                              "NumericWorkloadMonthTaskLimitedCell", "Tasks",
															  manager.WordDictionary["Tasks"],
                                                              Resources.Forecasted, _dateTimes, 15);
            gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "PercentWithNegativeCell", "CampaignTasks",
											manager.WordDictionary["CampaignTasks"],
                                            Resources.Forecasted, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "PositiveTimeSpanTotalSecondsCell",
											"AverageTaskTime", manager.WordDictionary["AverageTaskTime"],
                                            Resources.Forecasted, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "PercentWithNegativeCell", "CampaignTaskTime",
											manager.WordDictionary["CampaignTaskTime"],
                                            Resources.Forecasted, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "PositiveTimeSpanTotalSecondsCell",
											"AverageAfterTaskTime", manager.WordDictionary["AverageAfterTaskTime"],
                                            Resources.Forecasted, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "PercentWithNegativeCell",
											"CampaignAfterTaskTime", manager.WordDictionary["CampaignAfterTaskTime"],
                                            Resources.Forecasted, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "NumericReadOnlyCell", "TotalTasks",
											manager.WordDictionary["TotalTasks"], Resources.Forecasted,
                                            _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "TimeSpanTotalSecondsReadOnlyCell",
											"TotalAverageTaskTime", manager.WordDictionary["TotalAverageTaskTime"],
                                            Resources.Forecasted, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "TimeSpanTotalSecondsReadOnlyCell",
											"TotalAverageAfterTaskTime", manager.WordDictionary["TotalAverageAfterTaskTime"],
                                            Resources.Forecasted, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "NumericReadOnlyCell",
											"TotalStatisticCalculatedTasks", manager.WordDictionary["TotalStatisticCalculatedTasks"],
                                            Resources.Actual, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "NumericReadOnlyCell",
											"TotalStatisticAbandonedTasks", manager.WordDictionary["TotalStatisticAbandonedTasks"],
                                            Resources.Actual, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "NumericReadOnlyCell",
											"TotalStatisticAnsweredTasks", manager.WordDictionary["TotalStatisticAnsweredTasks"],
                                            Resources.Actual, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "TimeSpanTotalSecondsReadOnlyCell",
											"TotalStatisticAverageTaskTime", manager.WordDictionary["TotalStatisticAverageTaskTime"],
                                            Resources.Actual, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "TimeSpanTotalSecondsReadOnlyCell",
											"TotalStatisticAverageAfterTaskTime", manager.WordDictionary["TotalStatisticAverageAfterTaskTime"],
                                            Resources.Actual, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));
        }

        private void CreateWorkloadNonTelephonyRows()
        {
            var manager = GetManager(_owner);

            ITaskOwnerGridRow gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "NumericWorkloadMonthTaskLimitedCell", 
				"Tasks", manager.WordDictionary["Tasks"], Resources.Forecasted, _dateTimes, 15);
            gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "PercentWithNegativeCell", 
				"CampaignTasks", manager.WordDictionary["CampaignTasks"], Resources.Forecasted, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "TimeSpanLongHourMinuteSecondOnlyPositiveCellModel",
				"AverageTaskTime", manager.WordDictionary["AverageTaskTime"], Resources.Forecasted, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "PercentWithNegativeCell", 
				"CampaignTaskTime", manager.WordDictionary["CampaignTaskTime"], Resources.Forecasted, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "TimeSpanLongHourMinuteSecondOnlyPositiveCellModel", 
				"AverageAfterTaskTime", manager.WordDictionary["AverageAfterTaskTime"], Resources.Forecasted, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "PercentWithNegativeCell", 
				"CampaignAfterTaskTime", manager.WordDictionary["CampaignAfterTaskTime"], Resources.Forecasted, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "NumericReadOnlyCell", 
				"TotalTasks", manager.WordDictionary["TotalTasks"], Resources.Forecasted, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "TimeSpanLongHourMinutesStaticCellModel", 
				"TotalAverageTaskTime", manager.WordDictionary["TotalAverageTaskTime"], Resources.Forecasted, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "TimeSpanLongHourMinutesStaticCellModel", 
				"TotalAverageAfterTaskTime", manager.WordDictionary["TotalAverageAfterTaskTime"], Resources.Forecasted, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "NumericReadOnlyCell", 
				"TotalStatisticCalculatedTasks", manager.WordDictionary["TotalStatisticCalculatedTasks"], Resources.Actual, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "NumericReadOnlyCell", 
				"TotalStatisticAbandonedTasks", manager.WordDictionary["TotalStatisticAbandonedTasks"], Resources.Actual, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "NumericReadOnlyCell", 
				"TotalStatisticAnsweredTasks", manager.WordDictionary["TotalStatisticAnsweredTasks"], Resources.Actual, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "TimeSpanLongHourMinutesStaticCellModel", 
				"TotalStatisticAverageTaskTime", manager.WordDictionary["TotalStatisticAverageTaskTime"], Resources.Actual, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "TimeSpanLongHourMinutesStaticCellModel", 
				"TotalStatisticAverageAfterTaskTime", manager.WordDictionary["TotalStatisticAverageAfterTaskTime"], Resources.Actual, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));
        }



        #endregion

        protected override void OnQueryRowCount(GridRowColCountEventArgs e)
        {
            base.OnQueryRowCount(e);

            e.Count = _gridRows.Count - 1;
            e.Handled = true;
        }

        protected override void OnQueryColCount(GridRowColCountEventArgs e)
        {
            base.OnQueryColCount(e);

           // e.Count = _rowManagerTaskOwner.DataSource.Count + 1;
            e.Count = _dateTimes.Count + 1;
            e.Handled = true;
        }


        private void initializeGrid()
        {
            Model.Options.MergeCellsMode = GridMergeCellsMode.OnDemandCalculation | GridMergeCellsMode.MergeColumnsInRow | GridMergeCellsMode.MergeRowsInColumn;
            Cols.Size[1] = ColorHelper.GridHeaderColumnWidth();
            DefaultColWidth = 100;
            RowCount = _gridRows.Count - 1;
            ColCount = _rowManagerTaskOwner.DataSource.Count + 1;
            BaseStylesMap["Header"].StyleInfo.HorizontalAlignment = GridHorizontalAlignment.Center;

            Rows.HeaderCount = 1;
            Cols.HeaderCount = 1;
            Cols.SetFrozenCount(1, false);
            Rows.SetFrozenCount(1, false);

            Model.MergeCells.DelayMergeCells(GridRangeInfo.Table());
            Refresh();
        }
        #region DrawValuesInGrid

        protected override void OnQueryCellInfo(GridQueryCellInfoEventArgs e)
        {
            base.OnQueryCellInfo(e);

            if (e.ColIndex < 0 || e.RowIndex < 0) return;
            if (e.RowIndex >= _gridRows.Count) return;

            if (e.Style.CellIdentity == null) return;
            _gridRows[e.RowIndex].QueryCellInfo(GetCellInfo(e.Style, e.ColIndex, e.RowIndex));
            if (e.ColIndex > 0 && (e.ColIndex - 1) < _rowManagerTaskOwner.DataSource.Count)
            formatCell(e.Style, _rowManagerTaskOwner.DataSource[e.ColIndex - 1]);

            if (e.ColIndex == 1 && e.RowIndex == 0)
            {
                StringBuilder infoText = new StringBuilder();
                if (_owner.TargetType != TemplateTarget.Workload)
                {
                    infoText.Append(UserTexts.Resources.Skill);
                }
                else
                {
                    infoText.Append(UserTexts.Resources.Workload);
                }
                infoText.Append(string.Concat(" - ", UserTexts.Resources.Month));

                e.Style.CellValue = infoText.ToString();
            }
            e.Handled = true;
        }

        #endregion

        #region SetValuesInGrid

        protected override void OnSaveCellInfo(GridSaveCellInfoEventArgs e)
        {
            base.OnSaveCellInfo(e);
            Cursor = Cursors.WaitCursor;
            _taskOwnerPeriodHelper.BeginUpdate();
            _gridRows[e.RowIndex].SaveCellInfo(GetCellInfo(e.Style, e.ColIndex, e.RowIndex));
            _taskOwnerPeriodHelper.EndUpdate();
            if (_owner != null) _owner.TriggerValuesChanged();
            RefreshRange(GridRangeInfo.Col(e.ColIndex));
            Cursor = Cursors.Default;

            e.Handled = true;
        }

        #endregion

        #region CopyAndPaste

        protected override void OnBeforePaste()
        {
            _taskOwnerPeriodHelper.BeginUpdate();
        }

        protected override void OnAfterPaste()
        {
            _taskOwnerPeriodHelper.EndUpdate();
            if (_owner != null) _owner.TriggerValuesChanged();
        }

        #endregion

        protected override void OnKeyDown(KeyEventArgs e)
        {
			int rowHeaders = Cols.HeaderCount + 1;

			if (CurrentCell.ColIndex < (rowHeaders - 1)) return;
			if (CurrentCell.RowIndex <= 1)
			{
				TaskOwnerPeriod period = null;
				
				if (CurrentCell.ColIndex - rowHeaders >= 0)
				{
					period = _rowManagerTaskOwner.DataSource[CurrentCell.ColIndex - rowHeaders] as TaskOwnerPeriod;
				}

                if (e.KeyCode == Keys.Delete && period != null)
                {
                    Cursor = Cursors.WaitCursor;
                    period.ResetTaskOwner();
                    Refresh();
                    Cursor = Cursors.Default;
                    return;
                }
                base.OnKeyDown(e);
                Refresh();
            }
            base.OnKeyDown(e);
        }

        bool _turnOffUpdate;
        protected override void OnSelectionChanged(GridSelectionChangedEventArgs e)
        {
            int rowHeaders = Rows.HeaderCount + 1;
            base.OnSelectionChanged(e);

            if (e.Range.Left >= rowHeaders)
            {
                
                var workloadDay = _rowManagerTaskOwner.DataSource[e.Range.Left - rowHeaders].CurrentDate;
                _turnOffUpdate = true;
                Owner.CurrentDay = workloadDay;
                _turnOffUpdate = false;
            }
           
            if (e.Range.Top > 1)
            {
                GridRow gridRow = _gridRows[e.Range.Top] as GridRow;

                if (gridRow != null)
                {
                    _owner.TriggerCellClicked(this,gridRow);
                }
            }
        }

       



        public bool HasColumns
        {
            get { return ColCount > 0; }
        }

        public void RefreshGrid()
        {
            Refresh();
            RefreshRange(ViewLayout.VisibleCellsRange);
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
            if (_turnOffUpdate) return;
            ITaskOwner taskOwnerPeriod = _rowManagerTaskOwner.DataSource.FirstOrDefault(wp =>
                {
                    TaskOwnerPeriod p = wp as TaskOwnerPeriod;
                    return p != null &&
                           p.StartDate <= theDate &&
                           p.EndDate >= theDate;
                });

            if (taskOwnerPeriod != null)
            {
                int index = _rowManagerTaskOwner.DataSource.IndexOf(taskOwnerPeriod);
                Model.ScrollCellInView(GridRangeInfo.Col(index + 1), GridScrollCurrentCellReason.MoveTo);
            }
        }

        public DateOnly GetLocalCurrentDate(int column)
        {
            int count = _rowManagerTaskOwner.DataSource.Count;
            if (count == 0)
                return DateOnly.MaxValue;
            DateOnly returnDate;
            if (column > count)
                returnDate = ((TaskOwnerPeriod)_rowManagerTaskOwner.DataSource[count - 1]).EndDate;
            else if (column <= 0)
                returnDate = ((TaskOwnerPeriod)_rowManagerTaskOwner.DataSource[0]).StartDate;
            else
                returnDate = ((TaskOwnerPeriod)_rowManagerTaskOwner.DataSource[column - 1]).StartDate;

            return returnDate;
        }

        public IDictionary<int, GridRow> EnabledChartGridRows
        {
            get
            {
                IDictionary<int, GridRow> settings = (from r in _gridRows.OfType<GridRow>()
                                                      where r.ChartSeriesSettings != null &&
                                                            r.ChartSeriesSettings.Enabled
                                                      select r).ToDictionary(k => _gridRows.IndexOf(k), v => v);

                return settings;
            }
        }

        public ReadOnlyCollection<GridRow> AllGridRows
        {
            get
            {
                return new ReadOnlyCollection<GridRow>(new List<GridRow>(_gridRows.OfType<GridRow>()));
            }
        }

        public int MainHeaderRow
        {
            get { return 1; }
        }

        public IList<GridRow> EnabledChartGridRowsMicke65()
        {
            IList<GridRow> ret = new List<GridRow>();
            foreach (string key in _chartSettings.SelectedRows)
            {
                foreach (GridRow gridRow in _gridRows.OfType<GridRow>())
                {
                    if (gridRow.DisplayMember == key)
                        ret.Add(gridRow);
                }
            }

            return ret; 
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

        public AbstractDetailView Owner
        {
            get { return _owner; }
            set { _owner = value; }
        }

        private static void formatCell(GridStyleInfo gridStyleInfo, ITaskOwner workloadPeriod)
        {
            if (!workloadPeriod.OpenForWork.IsOpen)
            {//note findme
                gridStyleInfo.TextColor = ColorFontClosedCell;
                gridStyleInfo.Font.FontStyle = FontClosedCell;
                //gridStyleInfo.BackColor = ColorEditableCell;
                gridStyleInfo.Enabled = false;
            }
            else
            {
                gridStyleInfo.Enabled = true;
                gridStyleInfo.BackColor = ColorEditableCell;
            }
        }

        public override WorkingInterval WorkingInterval
        {
            get { return WorkingInterval.Month; }
        }

        public override TimeSpan ChartResolution
        {
            get { return TimeSpan.FromDays(3); }
        }

        public override DateTime FirstDate
        {
            get { return _dateTimes.Min().Date; }
        }

        public override DateTime LastDate
        {
            get { return _dateTimes.Max().Date; }
        }

        protected override IDictionary<DateTime, double> GetRowDataForChart(GridRangeInfo gridRangeInfo)
        {
            throw new NotImplementedException();
        }
    }
}
