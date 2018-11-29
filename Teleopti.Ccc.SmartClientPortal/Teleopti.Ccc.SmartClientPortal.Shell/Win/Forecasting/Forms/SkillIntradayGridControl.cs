using System;
using System.Collections.Generic;
using System.Linq;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Rows;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Rows;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Forecasting;
using Teleopti.Ccc.WinCode.Common.Chart;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms
{
    public class SkillIntradayGridControl : BaseIntradayGridControl
    {
        private readonly IList<IMultisiteDay> _multisiteDays;
        private readonly RowManager<SkillDataGridRow, ISkillData> _rowManagerSkillDataPeriod;
        private readonly RowManager<SkillStaffPeriodGridRow, ISkillStaffPeriod> _rowManagerSkillStaffPeriod;
        private readonly RowManager<MultisitePeriodGridRow, IMultisitePeriod> _rowManagerMultisitePeriod;
        private readonly IMultisiteSkill _multisiteSkill;
        
        internal SkillIntradayGridControl(ITaskOwner skillDay, TaskOwnerHelper taskOwnerPeriodHelper, TimeZoneInfo timeZone, int resolution, AbstractDetailView owner,ChartSettings chartSettings)
            : this(skillDay, new List<IMultisiteDay>(), taskOwnerPeriodHelper, null, timeZone, resolution, owner, chartSettings)
        {
        }

        internal SkillIntradayGridControl(ITaskOwner skillDay, IList<IMultisiteDay> multisiteDays, TaskOwnerHelper taskOwnerPeriodHelper, IMultisiteSkill multisiteSkill, TimeZoneInfo timeZone, int resolution, AbstractDetailView owner, ChartSettings chartSettings)
            : base(skillDay, taskOwnerPeriodHelper, timeZone, resolution, owner, owner.SkillType.DisplayTimeSpanAsMinutes,chartSettings)
        {
            _multisiteDays = multisiteDays;
            _multisiteSkill = multisiteSkill;

            _rowManagerSkillDataPeriod = new RowManager<SkillDataGridRow, ISkillData>(this, Intervals, Resolution);
            _rowManagerSkillStaffPeriod = new RowManager<SkillStaffPeriodGridRow, ISkillStaffPeriod>(this, Intervals, Resolution);
            _rowManagerMultisitePeriod = new RowManager<MultisitePeriodGridRow, IMultisitePeriod>(this, Intervals, Resolution);
        }

        #region InitializeGrid

        private void CreateMultisiteGridRows(IMultisiteSkill multisiteSkill)
        {
            if (multisiteSkill == null) return;
            foreach (IChildSkill childSkill in multisiteSkill.ChildSkills)
            {
                GridRows.Add(_rowManagerMultisitePeriod.AddRow(new MultisitePeriodGridRow(_rowManagerMultisitePeriod, "MultiSitePercentCell",
                    "", childSkill.Name, childSkill)));
            }
        }

        private bool IsChildSkill
        {
            get { return (((ISkillDay)TaskOwnerDay).Skill is IChildSkill); }
        }

        protected override void CreateGridRows()
        {
            base.CreateGridRows(); //Creates list and add headers

			if (Owner.SkillType.ForecastSource != ForecastSource.InboundTelephony && Owner.SkillType.ForecastSource != ForecastSource.Chat)
                NonTelephonyRows();
            else
                TelephonyRows();

            CreateMultisiteGridRows(_multisiteSkill);
        }

        private void TelephonyRows()
        {
			var manager = GetManager(Owner);
            var gridRow = new SkillStaffPeriodGridRow(_rowManagerSkillStaffPeriod,
                                                                          "NumericReadOnlyCell",
                                                                          "Payload.TaskData.Tasks",
																		  manager.WordDictionary["TotalTasks"])
                          	{
                          		ChartSeriesSettings = ConfigureSetting("TotalTasks")
                          	};
        	GridRows.Add(_rowManagerSkillStaffPeriod.AddRow(gridRow));

            gridRow = new SkillStaffPeriodGridRow(_rowManagerSkillStaffPeriod, "TimeSpanTotalSecondsReadOnlyCell",
                                                  "Payload.TaskData.AverageTaskTime",
												  manager.WordDictionary["TotalAverageTaskTime"])
                      	{
                      		ChartSeriesSettings = ConfigureSetting("TotalAverageTaskTime")
                      	};
        	GridRows.Add(_rowManagerSkillStaffPeriod.AddRow(gridRow));

            gridRow = new SkillStaffPeriodGridRow(_rowManagerSkillStaffPeriod, "TimeSpanTotalSecondsReadOnlyCell",
                                                  "Payload.TaskData.AverageAfterTaskTime",
												  manager.WordDictionary["TotalAverageAfterTaskTime"]);
            gridRow.ChartSeriesSettings = ConfigureSetting("TotalAverageAfterTaskTime");
            GridRows.Add(_rowManagerSkillStaffPeriod.AddRow(gridRow));

            SkillDataGridRow gridDataPeriodRow;
            if (!IsChildSkill)
            {
                gridDataPeriodRow = new SkillDataGridRow(_rowManagerSkillDataPeriod, "ServicePercentCell",
                                                               "ServiceLevelPercent",
                                                               UserTexts.Resources.
                                                                   ServiceLevelParenthesisPercentSign);
                gridDataPeriodRow.SaveCellValue += gridRow_SaveCellValue;
                gridDataPeriodRow.ChartSeriesSettings = ConfigureSetting(gridDataPeriodRow.DisplayMember);
                GridRows.Add(_rowManagerSkillDataPeriod.AddRow(gridDataPeriodRow));

                gridDataPeriodRow = new SkillDataGridRow(_rowManagerSkillDataPeriod,
                                                               "NumericServiceTargetLimitedCell",
                                                               "ServiceLevelSeconds",
                                                               UserTexts.Resources.ServiceLevelSParenthesis);
                gridDataPeriodRow.SaveCellValue += gridRow_SaveCellValue;
                gridDataPeriodRow.ChartSeriesSettings = ConfigureSetting(gridDataPeriodRow.DisplayMember);
                GridRows.Add(_rowManagerSkillDataPeriod.AddRow(gridDataPeriodRow));

                gridDataPeriodRow = new SkillDataGridRow(_rowManagerSkillDataPeriod, "PercentCell",
                                                               "MinOccupancy", UserTexts.Resources.MinimumOccupancy);
                gridDataPeriodRow.SaveCellValue += gridRow_SaveCellValue;
                gridDataPeriodRow.ChartSeriesSettings = ConfigureSetting(gridDataPeriodRow.DisplayMember);
                GridRows.Add(_rowManagerSkillDataPeriod.AddRow(gridDataPeriodRow));

                gridDataPeriodRow = new SkillDataGridRow(_rowManagerSkillDataPeriod, "PercentCell",
                                                               "MaxOccupancy", UserTexts.Resources.MaximumOccupancy);
                gridDataPeriodRow.SaveCellValue += gridRow_SaveCellValue;
                gridDataPeriodRow.ChartSeriesSettings = ConfigureSetting(gridDataPeriodRow.DisplayMember);
                GridRows.Add(_rowManagerSkillDataPeriod.AddRow(gridDataPeriodRow));
            }

            gridDataPeriodRow = new SkillDataGridRow(_rowManagerSkillDataPeriod, "IntegerMinMaxAgentCell",
                                                           "MinimumPersons", UserTexts.Resources.MinimumAgents);
            gridDataPeriodRow.SaveCellValue += gridRow_SaveCellValue;
            gridDataPeriodRow.ChartSeriesSettings = ConfigureSetting(gridDataPeriodRow.DisplayMember);
            GridRows.Add(_rowManagerSkillDataPeriod.AddRow(gridDataPeriodRow));

            gridDataPeriodRow = new SkillDataGridRow(_rowManagerSkillDataPeriod, "IntegerMinMaxAgentCell",
                                                           "MaximumPersons", UserTexts.Resources.MaximumAgents);
            gridDataPeriodRow.SaveCellValue += gridRow_SaveCellValue;
            gridDataPeriodRow.ChartSeriesSettings = ConfigureSetting(gridDataPeriodRow.DisplayMember);
            GridRows.Add(_rowManagerSkillDataPeriod.AddRow(gridDataPeriodRow));

			if (!IsChildSkill)
			{
                gridDataPeriodRow = new SkillDataGridRow(_rowManagerSkillDataPeriod, "NullableNumericCell",
                                                               "ManualAgents", UserTexts.Resources.ManualAgents);
				gridDataPeriodRow.SaveCellValue += gridRow_SaveCellValue;
				gridDataPeriodRow.ChartSeriesSettings = ConfigureSetting(gridDataPeriodRow.DisplayMember);
				GridRows.Add(_rowManagerSkillDataPeriod.AddRow(gridDataPeriodRow));

                gridDataPeriodRow = new SkillDataGridRow(_rowManagerSkillDataPeriod, "PercentShrinkageCell",
                                                               "Shrinkage", UserTexts.Resources.Shrinkage);
                gridDataPeriodRow.SaveCellValue += gridRow_SaveCellValue;
                gridDataPeriodRow.ChartSeriesSettings = ConfigureSetting(gridDataPeriodRow.DisplayMember);
                GridRows.Add(_rowManagerSkillDataPeriod.AddRow(gridDataPeriodRow));

                gridDataPeriodRow = new SkillDataGridRow(_rowManagerSkillDataPeriod, "PercentEfficiencyCell",
                                                               "Efficiency", UserTexts.Resources.Efficiency);
                gridDataPeriodRow.SaveCellValue += gridRow_SaveCellValue;
                gridDataPeriodRow.ChartSeriesSettings = ConfigureSetting(gridDataPeriodRow.DisplayMember);
                GridRows.Add(_rowManagerSkillDataPeriod.AddRow(gridDataPeriodRow));
            }

            gridRow = new SkillStaffPeriodGridRow(_rowManagerSkillStaffPeriod, "PercenFromPercentReadOnlyCellModel",
                                                  "Payload.CalculatedOccupancyPercent",
                                                  UserTexts.Resources.CalculatedOccupancyPercentSign);
            gridRow.ChartSeriesSettings = ConfigureSetting("CalculatedOccupancyPercent");
            GridRows.Add(_rowManagerSkillStaffPeriod.AddRow(gridRow));

            gridRow = new SkillStaffPeriodGridRow(_rowManagerSkillStaffPeriod, "NumericReadOnlyCell",
                                                  "Payload.ForecastedIncomingDemand", UserTexts.Resources.Agents);
            gridRow.ChartSeriesSettings = ConfigureSetting(gridRow.DisplayMember);
            GridRows.Add(_rowManagerSkillStaffPeriod.AddRow(gridRow));

            gridRow = new SkillStaffPeriodGridRow(_rowManagerSkillStaffPeriod, "NumericReadOnlyCell",
                                                  "Payload.CalculatedTrafficIntensityWithShrinkage",
                                                  UserTexts.Resources.AgentsWithShrinkage);
            gridRow.ChartSeriesSettings = ConfigureSetting(gridRow.DisplayMember);
            GridRows.Add(_rowManagerSkillStaffPeriod.AddRow(gridRow));

        }

        private void NonTelephonyRows()
        {
            TextManager manager = GetManager(Owner);

            SkillStaffPeriodGridRow gridRow = new SkillStaffPeriodGridRow(_rowManagerSkillStaffPeriod,
                                                                          "NumericReadOnlyCell",
                                                                          "Payload.TaskData.Tasks",
                                                                          manager.WordDictionary["TotalTasks"]);
            gridRow.ChartSeriesSettings = ConfigureSetting("TotalTasks");
            //gridRow.DisplayMember]; Need to go with TotalTasks here instead of Payload.TaskData.Task
            GridRows.Add(_rowManagerSkillStaffPeriod.AddRow(gridRow));

            gridRow = new SkillStaffPeriodGridRow(_rowManagerSkillStaffPeriod, "TimeSpanLongHourMinutesStaticCellModel",
                                                  "Payload.TaskData.AverageTaskTime",
                                                  manager.WordDictionary["TotalAverageTaskTime"]);
            gridRow.ChartSeriesSettings = ConfigureSetting("TotalAverageTaskTime");
            //gridRow.DisplayMember]; Need to go with TotalAverageTaskTime instead of Payload.TaskData.AverageTaskTime
            GridRows.Add(_rowManagerSkillStaffPeriod.AddRow(gridRow));

            gridRow = new SkillStaffPeriodGridRow(_rowManagerSkillStaffPeriod, "TimeSpanLongHourMinutesStaticCellModel",
                                                  "Payload.TaskData.AverageAfterTaskTime",
                                                  manager.WordDictionary["TotalAverageAfterTaskTime"]);
            gridRow.ChartSeriesSettings = ConfigureSetting("TotalAverageAfterTaskTime");
            //gridRow.DisplayMember]; Need to go with TotalAverageTaskTime instead of Payload.TaskData.AverageTaskTime
            GridRows.Add(_rowManagerSkillStaffPeriod.AddRow(gridRow));

            SkillDataGridRow gridDataPeriodRow;
            if (!IsChildSkill)
            {
                gridDataPeriodRow = new SkillDataGridRow(_rowManagerSkillDataPeriod,
                                                               "TimeSpanLongHourMinuteSecondOnlyPositiveCellModel",
                                                               "ServiceLevelTimeSpan",
                                                               manager.WordDictionary["HandledWithin"]);
                gridDataPeriodRow.SaveCellValue += gridRow_SaveCellValue;
                gridDataPeriodRow.ChartSeriesSettings = ConfigureSetting(gridDataPeriodRow.DisplayMember);
                GridRows.Add(_rowManagerSkillDataPeriod.AddRow(gridDataPeriodRow));

                    
            }

            gridDataPeriodRow = new SkillDataGridRow(_rowManagerSkillDataPeriod, "IntegerMinMaxAgentCell",
														   "MinimumPersons", manager.WordDictionary["MinimumAgents"]);
            gridDataPeriodRow.SaveCellValue += gridRow_SaveCellValue;
            gridDataPeriodRow.ChartSeriesSettings = ConfigureSetting(gridDataPeriodRow.DisplayMember);
            GridRows.Add(_rowManagerSkillDataPeriod.AddRow(gridDataPeriodRow));

            gridDataPeriodRow = new SkillDataGridRow(_rowManagerSkillDataPeriod, "IntegerMinMaxAgentCell",
														   "MaximumPersons", manager.WordDictionary["MaximumAgents"]);
            gridDataPeriodRow.SaveCellValue += gridRow_SaveCellValue;
            gridDataPeriodRow.ChartSeriesSettings = ConfigureSetting(gridDataPeriodRow.DisplayMember);
            GridRows.Add(_rowManagerSkillDataPeriod.AddRow(gridDataPeriodRow));

            if (!IsChildSkill)
            {
                gridDataPeriodRow = new SkillDataGridRow(_rowManagerSkillDataPeriod, "PercentShrinkageCell",
                                                               "Shrinkage", UserTexts.Resources.Shrinkage);
                gridDataPeriodRow.SaveCellValue += gridRow_SaveCellValue;
                gridDataPeriodRow.ChartSeriesSettings = ConfigureSetting(gridDataPeriodRow.DisplayMember);
                GridRows.Add(_rowManagerSkillDataPeriod.AddRow(gridDataPeriodRow));

                gridDataPeriodRow = new SkillDataGridRow(_rowManagerSkillDataPeriod, "PercentEfficiencyCell",
                                                               "Efficiency", UserTexts.Resources.Efficiency);
                gridDataPeriodRow.SaveCellValue += gridRow_SaveCellValue;
                gridDataPeriodRow.ChartSeriesSettings = ConfigureSetting(gridDataPeriodRow.DisplayMember);
                GridRows.Add(_rowManagerSkillDataPeriod.AddRow(gridDataPeriodRow));
            }

            gridRow = new SkillStaffPeriodGridRow(_rowManagerSkillStaffPeriod, "NumericReadOnlyCell",
                                                  "Payload.ForecastedIncomingDemand", manager.WordDictionary["AgentsInc"]);
            gridRow.ChartSeriesSettings = ConfigureSetting(gridRow.DisplayMember);
            GridRows.Add(_rowManagerSkillStaffPeriod.AddRow(gridRow));

            gridRow = new SkillStaffPeriodGridRow(_rowManagerSkillStaffPeriod, "NumericReadOnlyCell",
                                                  "Payload.CalculatedTrafficIntensityWithShrinkage",
                                                  manager.WordDictionary["AgentsIncWithShrinkage"]);
            gridRow.ChartSeriesSettings = ConfigureSetting(gridRow.DisplayMember);
            GridRows.Add(_rowManagerSkillStaffPeriod.AddRow(gridRow));

            gridRow = new SkillStaffPeriodGridRow(_rowManagerSkillStaffPeriod, "NumericReadOnlyCell",
												  "ForecastedDistributedDemand", manager.WordDictionary["Agents"]);
            gridRow.ChartSeriesSettings = ConfigureSetting(gridRow.DisplayMember);
            GridRows.Add(_rowManagerSkillStaffPeriod.AddRow(gridRow));

            gridRow = new SkillStaffPeriodGridRow(_rowManagerSkillStaffPeriod, "NumericReadOnlyCell",
                                                  "ForecastedDistributedDemandWithShrinkage",
												  manager.WordDictionary["AgentsWithShrinkage"]);
            gridRow.ChartSeriesSettings = ConfigureSetting(gridRow.DisplayMember);
            GridRows.Add(_rowManagerSkillStaffPeriod.AddRow(gridRow));
        }

        private void gridRow_SaveCellValue(object sender, FromCellEventArgs<ISkillData> e)
        {
            if (!pasting)
            {
                //e.Item.MinOccupancy
                UpdateDataPeriodList();
            }
        }

        private bool pasting;
        protected override void OnBeforePaste()
        {
            pasting = true;
            base.OnBeforePaste();
        }

        protected override void OnAfterPaste()
        {
            base.OnAfterPaste();
            UpdateDataPeriodList();
            pasting = false;
        }

        protected override void MergeSplit(ModifyCellOption options)
        {
            GridRangeInfoList rangelist;

            if (Selections.GetSelectedRanges(out rangelist, true) &&
                rangelist.Count == 1)
            {
                int leftmostcellAfterHeader = Cols.HeaderCount + 1;
                int leftMostCell = rangelist[0].Left;
                int rightMostCell = rangelist[0].Right;
            	int lastColumn = ColCount;
                int row = rangelist[0].Top;

                leftMostCell = Math.Max(leftMostCell, leftmostcellAfterHeader);
                rightMostCell = (rightMostCell == 0) ? lastColumn : Math.Min(rightMostCell,lastColumn);

                if (rangelist[0].IsCols && GridRows.Count > 0)
                    row = GridRows.IndexOf(_rowManagerSkillDataPeriod.Rows[0]);

                SkillDataGridRow gridRowSkillData = GridRows[row] as SkillDataGridRow;
                if (gridRowSkillData != null)
                {
                    IList<ISkillData> taskPeriods = gridRowSkillData.GetMergeData(leftMostCell, rightMostCell);
                    TriggerModifyCells(options, taskPeriods);
                }

                if (rangelist[0].IsCols && _rowManagerMultisitePeriod != null && _rowManagerMultisitePeriod.Rows.Count > 0)
                    row = GridRows.IndexOf(_rowManagerMultisitePeriod.Rows[0]);

                MultisitePeriodGridRow gridRowMultisite = GridRows[row] as MultisitePeriodGridRow;
                if (gridRowMultisite != null)
                {

                    IList<IMultisitePeriod> multisitePeriods = gridRowMultisite.GetMergeData(leftMostCell, rightMostCell);
                    TriggerModifyCells(options, multisitePeriods);
                }
            }
        }

        #endregion

        protected override void OnUpdatingDataPeriodList()
        {
            TaskOwnerDay = TaskOwnerHelper.TaskOwnerDays
                .FirstOrDefault(t => t.CurrentDate == Owner.CurrentDay);

            IList<IMultisitePeriod> multisitePeriods = GetMultisitePeriodsForDay();
            IList<ISkillData> skillDataPeriods;
            IList<ISkillStaffPeriod> skillStaffPeriods;

            ISkillDay skillDay = TaskOwnerDay as ISkillDay;
            if (skillDay != null)
            {
                skillDataPeriods = skillDay.SkillDataPeriodCollection
                    .OrderBy(t => t.Period.StartDateTime)
                    .OfType<ISkillData>()
                    .ToList();
                skillStaffPeriods = skillDay.SkillStaffPeriodCollection
                    .OrderBy(t => t.Period.StartDateTime)
                    .ToList();
            }
            else
            {
                skillDataPeriods = new List<ISkillData>();
                skillStaffPeriods = new List<ISkillStaffPeriod>();
            }

            if (skillStaffPeriods.Count > 0)
            {
                DateTime firstTime = skillStaffPeriods[0].Period.StartDateTime;
                DateTime lastTime = skillStaffPeriods.Last().Period.EndDateTime;
                CreateIntervalList(firstTime, lastTime);
            }
            else
            {
                Intervals.Clear();
            }
            if (TaskOwnerDay != null)
            {
                _rowManagerSkillStaffPeriod.BaseDate = TaskOwnerDay.CurrentDate.Date;
                _rowManagerSkillDataPeriod.BaseDate = TaskOwnerDay.CurrentDate.Date;
                _rowManagerMultisitePeriod.BaseDate = TaskOwnerDay.CurrentDate.Date;
                GridRows[0] = new DateHeaderGridRow(DateHeaderType.Date, new List<DateOnly> { TaskOwnerDay.CurrentDate });
                GridRows[1] = new DateHeaderGridRow(DateHeaderType.WeekdayName, new List<DateOnly> { TaskOwnerDay.CurrentDate });
            }
            _rowManagerSkillDataPeriod.SetDataSource(skillDataPeriods);
            _rowManagerSkillStaffPeriod.SetDataSource(skillStaffPeriods);
            _rowManagerMultisitePeriod.SetDataSource(multisitePeriods);
        }

        private IList<IMultisitePeriod> GetMultisitePeriodsForDay()
        {
            IMultisiteDay multisiteDay = _multisiteDays.FirstOrDefault(t => t.MultisiteDayDate == Owner.CurrentDay);
            if (multisiteDay != null)
            {
                return multisiteDay
                    .MultisitePeriodCollection
                    .OrderBy(t => t.Period.StartDateTime)
                    .ToList();
            }

            return new List<IMultisitePeriod>();
        }

        protected override void OnSelectionChanged(GridSelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);
            if (Owner!=null && e.Range.Top > 1)
            {
                var gridRow = GridRows[e.Range.Top] as GridRow;

                if (gridRow != null)
                {
                    Owner.TriggerCellClicked(this, gridRow);
                }
            }
        }

		protected override void OnShowContextMenu(Syncfusion.Windows.Forms.ShowContextMenuEventArgs e)
		{
			ContextMenu.MenuItems[3].Enabled = true;
			base.OnShowContextMenu(e);
		}

		protected override void SaveAsTemplateOnClick(object sender, EventArgs e)
		{
			var skillDay = TaskOwnerDay as SkillDay;
			if (skillDay == null)
				return;
			using(var editSkillDayTemplate = new EditSkillDayTemplate(skillDay))
				editSkillDayTemplate.ShowDialog(this);
		}

        protected override void TriggerGridRangePasted(GridRangeInfo gridRangeInfo)
        {
            base.TriggerGridRangePasted(gridRangeInfo);
            var skillDay = TaskOwnerDay as SkillDay;
            if (skillDay == null)
                return;
            skillDay.RecalculateStaff();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var row in GridRows.OfType<SkillDataGridRow>())
                {
                    row.SaveCellValue -= gridRow_SaveCellValue;
                }
                if (_rowManagerMultisitePeriod!=null)
                {
                    _rowManagerMultisitePeriod.DataSource.Clear();
                    _rowManagerMultisitePeriod.Rows.Clear();
                }
                if(_rowManagerSkillDataPeriod!=null)
                {
                    _rowManagerSkillDataPeriod.DataSource.Clear();
                    _rowManagerSkillDataPeriod.Rows.Clear();
                }
                if(_rowManagerSkillStaffPeriod!=null)
                {
                    _rowManagerSkillStaffPeriod.DataSource.Clear();
                    _rowManagerSkillStaffPeriod.Rows.Clear();
                }
            }
            base.Dispose(disposing);
        }
    }
}
