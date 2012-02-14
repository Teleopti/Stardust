﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.Win.Common.Controls;
using Teleopti.Ccc.Win.Common.Controls.Rows;
using Teleopti.Ccc.WinCode.Common.Chart;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Common.Rows;
using Teleopti.Ccc.WinCode.Forecasting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Forecasting.Forms
{
    public class TaskOwnerWeekGridControl : TeleoptiGridControl, ITaskOwnerGrid
    {
        private readonly IList<DateOnly> _dateTimes;
        private AbstractDetailView _owner;
        private readonly TaskOwnerHelper _taskOwnerPeriodHelper;
        private IList<IGridRow> _gridRows;
        private readonly RowManager<ITaskOwnerGridRow, ITaskOwner> _rowManagerTaskOwner;
        private readonly ChartSettings _chartSettings;

        private IList<double> _modifiedItems;

        public TaskOwnerWeekGridControl(IEnumerable<ITaskOwner> taskOwnerPeriods, AbstractDetailView owner, ChartSettings chartSettings)
        {
            _owner = owner;
            _chartSettings = chartSettings;
            _dateTimes = taskOwnerPeriods.Select(tp => tp.CurrentDate).ToList();
            _rowManagerTaskOwner = new RowManager<ITaskOwnerGridRow, ITaskOwner>(this, null, -1);
            _rowManagerTaskOwner.SetDataSource(taskOwnerPeriods);
            _taskOwnerPeriodHelper = new TaskOwnerHelper(taskOwnerPeriods);
            createGridRows();
            initializeGrid();
            CreateContextMenu();
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
            _gridRows.Add(new DateHeaderGridRow(DateHeaderType.MonthNameYear, _dateTimes));
            _gridRows.Add(new DateHeaderGridRow(DateHeaderType.WeekDates, _dateTimes));

            if (_owner.TargetType == TemplateTarget.Workload)
            {
                if (_owner.SkillType.ForecastSource!=ForecastSource.InboundTelephony)
                    CreateWorkloadNonTelephonyRows();
                else
                    CreateWorkloadTelephonyRows();
            }
            else if (_owner.TargetType == TemplateTarget.Skill)
            {
                if (_owner.SkillType.ForecastSource != ForecastSource.InboundTelephony)
                    CreateSkillNonTelephonyRows();
                else
                    CreateSkillTelephonyRows();
            }
        }

        private void CreateSkillTelephonyRows()
        {
            ITaskOwnerGridRow gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "NumericReadOnlyCell", "TotalTasks", UserTexts.Resources.TotalCalls, UserTexts.Resources.Forecasted, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting("TotalTasks");
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "TimeSpanTotalSecondsReadOnlyCell", "TotalAverageTaskTime", UserTexts.Resources.TotalTalkTime, UserTexts.Resources.Forecasted, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting("TotalAverageTaskTime");
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "TimeSpanTotalSecondsReadOnlyCell", "TotalAverageAfterTaskTime", UserTexts.Resources.TotalACW, UserTexts.Resources.Forecasted, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting("TotalAverageAfterTaskTime");
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "TimeSpanReadOnlyCell", "ForecastedIncomingDemand", UserTexts.Resources.Hours, UserTexts.Resources.Forecasted, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting("ForecastedIncomingDemand");
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "TimeSpanReadOnlyCell", "ForecastedIncomingDemandWithShrinkage", UserTexts.Resources.HoursIncShrinkage, UserTexts.Resources.Forecasted, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting("ForecastedIncomingDemandWithShrinkage");
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));
        }

        private void CreateSkillNonTelephonyRows()
        {
            TextManager manager = GetManager(_owner);
            
            ITaskOwnerGridRow gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "NumericReadOnlyCell", "TotalTasks", manager.WordDictionary["TotalTasks"], UserTexts.Resources.Forecasted, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting("TotalTasks");
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "TimeSpanLongHourMinutesStaticCellModel", "TotalAverageTaskTime", manager.WordDictionary["TotalAverageTaskTime"], UserTexts.Resources.Forecasted, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting("TotalAverageTaskTime");
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "TimeSpanLongHourMinutesStaticCellModel", "TotalAverageAfterTaskTime", manager.WordDictionary["TotalAverageAfterTaskTime"], UserTexts.Resources.Forecasted, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting("TotalAverageAfterTaskTime");
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "TimeSpanLongHourMinutesStaticCellModel", "ForecastedIncomingDemand", manager.WordDictionary["ForecastedIncomingDemand"], UserTexts.Resources.Forecasted, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting("ForecastedIncomingDemand");
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "TimeSpanLongHourMinutesStaticCellModel", "ForecastedIncomingDemandWithShrinkage", manager.WordDictionary["ForecastedIncomingDemandWithShrinkage"], UserTexts.Resources.Forecasted, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting("ForecastedIncomingDemandWithShrinkage");
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));
        }

        private void CreateWorkloadTelephonyRows()
        {
            ITaskOwnerGridRow gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner,
                                                              "NumericWorkloadDayTaskLimitedCell", "Tasks",
                                                              UserTexts.Resources.Calls,
                                                              UserTexts.Resources.Forecasted, _dateTimes, 13);
            gridRow.ChartSeriesSettings = configureSetting("Tasks");
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "PercentWithNegativeCell", "CampaignTasks",
                                            UserTexts.Resources.CampaignCallsPercentSign,
                                            UserTexts.Resources.Forecasted, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting("CampaignTasks");
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "PositiveTimeSpanTotalSecondsCell",
                                            "AverageTaskTime", UserTexts.Resources.TalkTime,
                                            UserTexts.Resources.Forecasted, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting("AverageTaskTime");
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "PercentWithNegativeCell", "CampaignTaskTime",
                                            UserTexts.Resources.CampaignTalkTimePercentSign,
                                            UserTexts.Resources.Forecasted, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting("CampaignTaskTime");
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "PositiveTimeSpanTotalSecondsCell",
                                            "AverageAfterTaskTime", UserTexts.Resources.ACW,
                                            UserTexts.Resources.Forecasted, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting("AverageAfterTaskTime");
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "PercentWithNegativeCell",
                                            "CampaignAfterTaskTime", UserTexts.Resources.CampaignACWPercentSign,
                                            UserTexts.Resources.Forecasted, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting("CampaignAfterTaskTime");
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "NumericReadOnlyCell", "TotalTasks",
                                            UserTexts.Resources.TotalCalls, UserTexts.Resources.Forecasted,
                                            _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting("TotalTasks");
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "TimeSpanTotalSecondsReadOnlyCell",
                                            "TotalAverageTaskTime", UserTexts.Resources.TotalTalkTime,
                                            UserTexts.Resources.Forecasted, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting("TotalAverageTaskTime");
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "TimeSpanTotalSecondsReadOnlyCell",
                                            "TotalAverageAfterTaskTime", UserTexts.Resources.TotalACW,
                                            UserTexts.Resources.Forecasted, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting("TotalAverageAfterTaskTime");
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "NumericReadOnlyCell",
                                            "TotalStatisticCalculatedTasks", UserTexts.Resources.CalculatedCalls,
                                            UserTexts.Resources.Actual, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting("TotalStatisticCalculatedTasks");
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "NumericReadOnlyCell",
                                            "TotalStatisticAbandonedTasks", UserTexts.Resources.AbandonedCalls,
                                            UserTexts.Resources.Actual, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting("TotalStatisticAbandonedTasks");
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "NumericReadOnlyCell",
                                            "TotalStatisticAnsweredTasks", UserTexts.Resources.AnsweredCalls,
                                            UserTexts.Resources.Actual, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting("TotalStatisticAnsweredTasks");
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "TimeSpanTotalSecondsReadOnlyCell",
                                            "TotalStatisticAverageTaskTime", UserTexts.Resources.TalkTime,
                                            UserTexts.Resources.Actual, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting("TotalStatisticAverageTaskTime");
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "TimeSpanTotalSecondsReadOnlyCell",
                                            "TotalStatisticAverageAfterTaskTime", UserTexts.Resources.ACW,
                                            UserTexts.Resources.Actual, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting("TotalStatisticAverageAfterTaskTime");
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));
        }

        private void CreateWorkloadNonTelephonyRows()
        {
            TextManager manager = new TextManager(_owner.SkillType);

            ITaskOwnerGridRow gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "NumericWorkloadDayTaskLimitedCell", "Tasks", manager.WordDictionary["Tasks"], UserTexts.Resources.Forecasted, _dateTimes, 13);
            gridRow.ChartSeriesSettings = configureSetting("Tasks");
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "PercentWithNegativeCell", "CampaignTasks", manager.WordDictionary["CampaignTasks"], UserTexts.Resources.Forecasted, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting("CampaignTasks");
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "TimeSpanLongHourMinuteSecondOnlyPositiveCellModel", "AverageTaskTime", manager.WordDictionary["AverageTaskTime"], UserTexts.Resources.Forecasted, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting("AverageTaskTime");
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "PercentWithNegativeCell", "CampaignTaskTime", manager.WordDictionary["CampaignTaskTime"], UserTexts.Resources.Forecasted, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting("CampaignTaskTime");
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "TimeSpanLongHourMinuteSecondOnlyPositiveCellModel", "AverageAfterTaskTime", manager.WordDictionary["AverageAfterTaskTime"], UserTexts.Resources.Forecasted, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting("AverageAfterTaskTime");
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "PercentWithNegativeCell", "CampaignAfterTaskTime", manager.WordDictionary["CampaignAfterTaskTime"], UserTexts.Resources.Forecasted, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting("CampaignAfterTaskTime");
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "NumericReadOnlyCell", "TotalTasks", manager.WordDictionary["TotalTasks"], UserTexts.Resources.Forecasted, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting("TotalTasks");
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "TimeSpanLongHourMinutesStaticCellModel", "TotalAverageTaskTime", manager.WordDictionary["TotalAverageTaskTime"], UserTexts.Resources.Forecasted, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting("TotalAverageTaskTime");
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "TimeSpanLongHourMinutesStaticCellModel", "TotalAverageAfterTaskTime", manager.WordDictionary["TotalAverageAfterTaskTime"], UserTexts.Resources.Forecasted, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting("TotalAverageAfterTaskTime");
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));


            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "NumericReadOnlyCell", "TotalStatisticCalculatedTasks", manager.WordDictionary["TotalStatisticCalculatedTasks"], UserTexts.Resources.Actual, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting("TotalStatisticCalculatedTasks");
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "NumericReadOnlyCell", "TotalStatisticAbandonedTasks", manager.WordDictionary["TotalStatisticAbandonedTasks"], UserTexts.Resources.Actual, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting("TotalStatisticAbandonedTasks");
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "NumericReadOnlyCell", "TotalStatisticAnsweredTasks", manager.WordDictionary["TotalStatisticAnsweredTasks"], UserTexts.Resources.Actual, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting("TotalStatisticAnsweredTasks");
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "TimeSpanLongHourMinutesStaticCellModel", "TotalStatisticAverageTaskTime", manager.WordDictionary["TotalStatisticAverageTaskTime"], UserTexts.Resources.Actual, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting("TotalStatisticAverageTaskTime");
            _gridRows.Add(_rowManagerTaskOwner.AddRow(gridRow));

            gridRow = new ITaskOwnerGridRow(_rowManagerTaskOwner, "TimeSpanLongHourMinutesStaticCellModel", "TotalStatisticAverageAfterTaskTime", manager.WordDictionary["TotalStatisticAverageAfterTaskTime"], UserTexts.Resources.Actual, _dateTimes);
            gridRow.ChartSeriesSettings = configureSetting("TotalStatisticAverageAfterTaskTime");
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

            e.Count = _rowManagerTaskOwner.DataSource.Count + 1;
            e.Handled = true;
        }

        private void initializeGrid()
        {
            Model.Options.MergeCellsMode = GridMergeCellsMode.OnDemandCalculation | GridMergeCellsMode.MergeColumnsInRow | GridMergeCellsMode.MergeRowsInColumn;
            Cols.Size[1] = ColorHelper.GridHeaderColumnWidth();//100;
            DefaultColWidth = 180;
            RowCount = _gridRows.Count - 1;
            ColCount = _rowManagerTaskOwner.DataSource.Count+1;
            BaseStylesMap["Header"].StyleInfo.HorizontalAlignment = GridHorizontalAlignment.Center;

            Rows.HeaderCount = 1;
            Cols.HeaderCount = 1;
            Rows.SetFrozenCount(1,false);
            Cols.SetFrozenCount(1,false);

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

            //int colHeaders = Cols.HeaderCount + 1;
            //if (e.ColIndex < 0 || e.RowIndex < 0) return;
            //if (Math.Max(colHeaders, e.ColIndex) - colHeaders >= _rowManagerTaskOwner.DataSource.Count) return;
            //if (e.RowIndex >= _gridRows.Count) return;

            //_gridRows[e.RowIndex].QueryCellInfo(GetCellInfo(e.Style, e.ColIndex, e.RowIndex));
            //if (e.ColIndex > 0 && (e.ColIndex - 1) < _rowManagerTaskOwner.DataSource.Count)
            //formatCell(e.Style, _rowManagerTaskOwner.DataSource[e.ColIndex - 1]);

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
                infoText.Append(string.Concat(" - ", UserTexts.Resources.Week));

                e.Style.CellValue = infoText.ToString();
            }

            e.Handled = true;
        }

        #endregion

        #region SetValuesInGrid

        protected override void OnSaveCellInfo(GridSaveCellInfoEventArgs e)
        {
            base.OnSaveCellInfo(e);

            _taskOwnerPeriodHelper.BeginUpdate();
            _gridRows[e.RowIndex].SaveCellInfo(GetCellInfo(e.Style, e.ColIndex, e.RowIndex));
            _taskOwnerPeriodHelper.EndUpdate();
            if (_owner != null) _owner.TriggerValuesChanged();
            RefreshRange(GridRangeInfo.Col(e.ColIndex));

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

            if (CurrentCell.ColIndex < (rowHeaders-1)) return;
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
                DateOnly workloadDay = _rowManagerTaskOwner.DataSource[e.Range.Left - rowHeaders].CurrentDate;
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
        public void GoToDate(DateTime theDate)
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

        public DateTime GetLocalCurrentDate(int column)
        {
            int count = _rowManagerTaskOwner.DataSource.Count;

            DateTime returnDate;
            if (column > count)
                returnDate = ((TaskOwnerPeriod)_rowManagerTaskOwner.DataSource[count - 1]).EndDate;
            else if (column <= 0)
                returnDate = ((TaskOwnerPeriod)_rowManagerTaskOwner.DataSource[0]).StartDate;
            else
                returnDate = ((TaskOwnerPeriod) _rowManagerTaskOwner.DataSource[column-1]).StartDate;

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
            if (workloadPeriod.IsClosed)
            {//note findme
                gridStyleInfo.TextColor = ColorFontClosedCell;
                gridStyleInfo.Font.FontStyle = FontClosedCell;
               // gridStyleInfo.BackColor = ColorEditableCell;
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
            get { return WorkingInterval.Week; }
        }

        public override TimeSpan ChartResolution
        {
            get { return TimeSpan.FromDays(3); }
        }

        public override DateTime FirstDateTime
        {
            get { return _dateTimes.Min(); }
        }

        public override DateTime LastDateTime
        {
            get { return _dateTimes.Max(); }
        }

        protected override IDictionary<DateTime, double> GetRowDataForChart(GridRangeInfo gridRangeInfo)
        {
            throw new NotImplementedException();
        }
    }
}
