using System;
using System.Collections.Generic;
using System.Linq;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Rows;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Rows;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Forecasting;
using Teleopti.Ccc.WinCode.Common.Chart;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms
{
    public class WorkloadIntradayGridControl : BaseIntradayGridControl
    {
	    private readonly ISkillType _skillType;
	    private RowManager<TemplateTaskPeriodGridRow, ITemplateTaskPeriod> _rowManagerTemplateTaskPeriod;
	    private IStatisticHelper _satisticHelper;

	    protected RowManager<TemplateTaskPeriodGridRow, ITemplateTaskPeriod> RowManagerTemplateTaskPeriod
        {
            get { return _rowManagerTemplateTaskPeriod; }
        }

        public WorkloadIntradayGridControl(ITaskOwner taskOwner, TaskOwnerHelper taskOwnerPeriodHelper, TimeZoneInfo timeZone, 
			int resolution, AbstractDetailView owner, ChartSettings chartSettings, ISkillType skillType, IStatisticHelper satisticHelper)
            : base(taskOwner, taskOwnerPeriodHelper, timeZone, resolution, owner, ((IWorkloadDayBase)taskOwner).Workload.Skill.SkillType.DisplayTimeSpanAsMinutes, chartSettings)
        {
	        _skillType = skillType;
	        _satisticHelper = satisticHelper;
	        _rowManagerTemplateTaskPeriod = new RowManager<TemplateTaskPeriodGridRow, ITemplateTaskPeriod>(this, Intervals, Resolution);
        }

	    protected override void MergeSplit(ModifyCellOption options)
        {
            GridRangeInfoList rangelist;

            if (Selections.GetSelectedRanges(out rangelist, true) &&
                rangelist.Count == 1)
            {
                int leftMostCell = rangelist[0].Left;
                int rightMostCell = rangelist[0].Right;
                int row = rangelist[0].Top;

                leftMostCell = Math.Max(leftMostCell, 1);
                rightMostCell = (rightMostCell == 0) ? ColCount : rightMostCell;
                if (rangelist[0].IsCols && GridRows.Count > 0)
                    row = GridRows.IndexOf(_rowManagerTemplateTaskPeriod.Rows[0]);

                TemplateTaskPeriodGridRow gridRow = GridRows[row] as TemplateTaskPeriodGridRow;
                if (gridRow == null) return;
                IList<ITemplateTaskPeriod> taskPeriods = gridRow.GetMergeData(leftMostCell, rightMostCell);
                TriggerModifyCells(options, taskPeriods);
            }
        }

        protected override void CreateGridRows()
        {
            base.CreateGridRows(); //Creates list and add headers


            _rowManagerTemplateTaskPeriod = new RowManager<TemplateTaskPeriodGridRow, ITemplateTaskPeriod>(this,
                                                                                                           Intervals,
                                                                                                           Resolution);

            if (Owner != null)
            {
				if (Owner.SkillType.ForecastSource != ForecastSource.InboundTelephony && Owner.SkillType.ForecastSource != ForecastSource.Chat)
                {
                    TextManager manager = new TextManager(Owner.SkillType);

                    TemplateTaskPeriodGridRow gridRow = new TemplateTaskPeriodGridRow(_rowManagerTemplateTaskPeriod,
                                                                                      "NumericWorkloadIntradayTaskLimitedCell",
                                                                                      "Tasks", manager.WordDictionary["Tasks"],
                                                                                      9);
                    gridRow.ChartSeriesSettings = ConfigureSetting("Tasks");
                    GridRows.Add(_rowManagerTemplateTaskPeriod.AddRow(gridRow));

                    gridRow = new TemplateTaskPeriodGridRow(_rowManagerTemplateTaskPeriod, "PercentWithNegativeCell",
                                                            "CampaignTasks",
                                                            manager.WordDictionary["CampaignTasks"]);
                    gridRow.ChartSeriesSettings = ConfigureSetting("CampaignTasks");
                    GridRows.Add(_rowManagerTemplateTaskPeriod.AddRow(gridRow));

                    gridRow = new TemplateTaskPeriodGridRow(_rowManagerTemplateTaskPeriod,
                                                            "TimeSpanLongHourMinuteSecondOnlyPositiveCellModel",
                                                            "AverageTaskTime", manager.WordDictionary["AverageTaskTime"]);
                    gridRow.ChartSeriesSettings = ConfigureSetting("AverageTaskTime");
                    GridRows.Add(_rowManagerTemplateTaskPeriod.AddRow(gridRow));

                    gridRow = new TemplateTaskPeriodGridRow(_rowManagerTemplateTaskPeriod, "PercentWithNegativeCell",
                                                            "CampaignTaskTime",
                                                            manager.WordDictionary["CampaignTaskTime"]);
                    gridRow.ChartSeriesSettings = ConfigureSetting("CampaignTaskTime");
                    GridRows.Add(_rowManagerTemplateTaskPeriod.AddRow(gridRow));

                    gridRow = new TemplateTaskPeriodGridRow(_rowManagerTemplateTaskPeriod,
                                                            "TimeSpanLongHourMinuteSecondOnlyPositiveCellModel",
                                                            "AverageAfterTaskTime",
                                                            manager.WordDictionary["AverageAfterTaskTime"]);
                    gridRow.ChartSeriesSettings = ConfigureSetting("AverageAfterTaskTime");
                    GridRows.Add(_rowManagerTemplateTaskPeriod.AddRow(gridRow));

                    gridRow = new TemplateTaskPeriodGridRow(_rowManagerTemplateTaskPeriod, "PercentWithNegativeCell",
                                                            "CampaignAfterTaskTime",
                                                            manager.WordDictionary["CampaignAfterTaskTime"]);
                    gridRow.ChartSeriesSettings = ConfigureSetting("CampaignAfterTaskTime");
                    GridRows.Add(_rowManagerTemplateTaskPeriod.AddRow(gridRow));

                    gridRow = new TemplateTaskPeriodGridRow(_rowManagerTemplateTaskPeriod, "NumericReadOnlyCell",
                                                            "TotalTasks", manager.WordDictionary["TotalTasks"]);
                    gridRow.ChartSeriesSettings = ConfigureSetting("TotalTasks");
                    GridRows.Add(_rowManagerTemplateTaskPeriod.AddRow(gridRow));

                    gridRow = new TemplateTaskPeriodGridRow(_rowManagerTemplateTaskPeriod,
                                                            "TimeSpanLongHourMinutesStaticCellModel",
                                                            "TotalAverageTaskTime", manager.WordDictionary["TotalAverageTaskTime"]);
                    gridRow.ChartSeriesSettings = ConfigureSetting("TotalAverageTaskTime");
                    GridRows.Add(_rowManagerTemplateTaskPeriod.AddRow(gridRow));

                    gridRow = new TemplateTaskPeriodGridRow(_rowManagerTemplateTaskPeriod,
                                                            "TimeSpanLongHourMinutesStaticCellModel",
                                                            "TotalAverageAfterTaskTime", manager.WordDictionary["TotalAverageAfterTaskTime"]);
                    gridRow.ChartSeriesSettings = ConfigureSetting("TotalAverageAfterTaskTime");
                    GridRows.Add(_rowManagerTemplateTaskPeriod.AddRow(gridRow));

                    gridRow = new TemplateTaskPeriodGridRow(_rowManagerTemplateTaskPeriod, "NumericReadOnlyCell",
                                                            "TotalStatisticCalculatedTasks",
                                                            manager.WordDictionary["TotalStatisticCalculatedTasks"]);
                    gridRow.ChartSeriesSettings = ConfigureSetting("TotalStatisticCalculatedTasks");
                    GridRows.Add(_rowManagerTemplateTaskPeriod.AddRow(gridRow));

                    gridRow = new TemplateTaskPeriodGridRow(_rowManagerTemplateTaskPeriod, "NumericReadOnlyCell",
                                                            "TotalStatisticAbandonedTasks",
                                                            manager.WordDictionary["TotalStatisticAbandonedTasks"]);
                    gridRow.ChartSeriesSettings = ConfigureSetting("TotalStatisticAbandonedTasks");
                    GridRows.Add(_rowManagerTemplateTaskPeriod.AddRow(gridRow));

                    gridRow = new TemplateTaskPeriodGridRow(_rowManagerTemplateTaskPeriod, "NumericReadOnlyCell",
                                                            "TotalStatisticAnsweredTasks",
                                                            manager.WordDictionary["TotalStatisticAnsweredTasks"]);
                    gridRow.ChartSeriesSettings = ConfigureSetting("TotalStatisticAnsweredTasks");
                    GridRows.Add(_rowManagerTemplateTaskPeriod.AddRow(gridRow));

                    gridRow = new TemplateTaskPeriodGridRow(_rowManagerTemplateTaskPeriod,
                                                            "TimeSpanLongHourMinutesStaticCellModel",
                                                            "TotalStatisticAverageTaskTime",
                                                            manager.WordDictionary["TotalStatisticAverageTaskTime"]);
                    gridRow.ChartSeriesSettings = ConfigureSetting("TotalStatisticAverageTaskTime");
                    GridRows.Add(_rowManagerTemplateTaskPeriod.AddRow(gridRow));

                    gridRow = new TemplateTaskPeriodGridRow(_rowManagerTemplateTaskPeriod,
                                                            "TimeSpanLongHourMinutesStaticCellModel",
                                                            "TotalStatisticAverageAfterTaskTime",
                                                            manager.WordDictionary["TotalStatisticAverageAfterTaskTime"]);
                    gridRow.ChartSeriesSettings = ConfigureSetting("TotalStatisticAverageAfterTaskTime");
                    GridRows.Add(_rowManagerTemplateTaskPeriod.AddRow(gridRow));
                }
                else
                {
                    InboundTelephonyRows();
                }
            }
            else
                InboundTelephonyRows();
        }

        private void InboundTelephonyRows()
        {
			var manager = new TextManager(_skillType);
            TemplateTaskPeriodGridRow gridRow = new TemplateTaskPeriodGridRow(_rowManagerTemplateTaskPeriod,
                                                                              "NumericWorkloadIntradayTaskLimitedCell",
																			  "Tasks", manager.WordDictionary["Tasks"],
                                                                              9);
            gridRow.ChartSeriesSettings = ConfigureSetting("Tasks");
            GridRows.Add(_rowManagerTemplateTaskPeriod.AddRow(gridRow));

            gridRow = new TemplateTaskPeriodGridRow(_rowManagerTemplateTaskPeriod, "PercentWithNegativeCell",
                                                    "CampaignTasks",
													manager.WordDictionary["CampaignTasks"]);
            gridRow.ChartSeriesSettings = ConfigureSetting("CampaignTasks");
            GridRows.Add(_rowManagerTemplateTaskPeriod.AddRow(gridRow));

            gridRow = new TemplateTaskPeriodGridRow(_rowManagerTemplateTaskPeriod, "PositiveTimeSpanTotalSecondsCell",
													"AverageTaskTime", manager.WordDictionary["AverageTaskTime"]);
            gridRow.ChartSeriesSettings = ConfigureSetting("AverageTaskTime");
            GridRows.Add(_rowManagerTemplateTaskPeriod.AddRow(gridRow));

            gridRow = new TemplateTaskPeriodGridRow(_rowManagerTemplateTaskPeriod, "PercentWithNegativeCell",
                                                    "CampaignTaskTime",
													manager.WordDictionary["CampaignTaskTime"]);
            gridRow.ChartSeriesSettings = ConfigureSetting("CampaignTaskTime");
            GridRows.Add(_rowManagerTemplateTaskPeriod.AddRow(gridRow));

            gridRow = new TemplateTaskPeriodGridRow(_rowManagerTemplateTaskPeriod,
                                                    "PositiveTimeSpanTotalSecondsCell",
                                                    "AverageAfterTaskTime",
													manager.WordDictionary["AverageAfterTaskTime"]);
            gridRow.ChartSeriesSettings = ConfigureSetting("AverageAfterTaskTime");
            GridRows.Add(_rowManagerTemplateTaskPeriod.AddRow(gridRow));

            gridRow = new TemplateTaskPeriodGridRow(_rowManagerTemplateTaskPeriod, "PercentWithNegativeCell",
                                                    "CampaignAfterTaskTime",
													manager.WordDictionary["CampaignAfterTaskTime"]);
            gridRow.ChartSeriesSettings = ConfigureSetting("CampaignAfterTaskTime");
            GridRows.Add(_rowManagerTemplateTaskPeriod.AddRow(gridRow));

            gridRow = new TemplateTaskPeriodGridRow(_rowManagerTemplateTaskPeriod, "NumericReadOnlyCell",
													"TotalTasks", manager.WordDictionary["TotalTasks"]);
            gridRow.ChartSeriesSettings = ConfigureSetting("TotalTasks");
            GridRows.Add(_rowManagerTemplateTaskPeriod.AddRow(gridRow));

            gridRow = new TemplateTaskPeriodGridRow(_rowManagerTemplateTaskPeriod,
                                                    "TimeSpanTotalSecondsReadOnlyCell",
													"TotalAverageTaskTime", manager.WordDictionary["TotalAverageTaskTime"]);
            gridRow.ChartSeriesSettings = ConfigureSetting("TotalAverageTaskTime");
            GridRows.Add(_rowManagerTemplateTaskPeriod.AddRow(gridRow));

            gridRow = new TemplateTaskPeriodGridRow(_rowManagerTemplateTaskPeriod,
                                                    "TimeSpanTotalSecondsReadOnlyCell",
													"TotalAverageAfterTaskTime", manager.WordDictionary["TotalAverageAfterTaskTime"]);
            gridRow.ChartSeriesSettings = ConfigureSetting("TotalAverageAfterTaskTime");
            GridRows.Add(_rowManagerTemplateTaskPeriod.AddRow(gridRow));

            gridRow = new TemplateTaskPeriodGridRow(_rowManagerTemplateTaskPeriod, "NumericReadOnlyCell",
                                                    "TotalStatisticCalculatedTasks",
													manager.WordDictionary["TotalStatisticCalculatedTasks"]);
            gridRow.ChartSeriesSettings = ConfigureSetting("TotalStatisticCalculatedTasks");
            GridRows.Add(_rowManagerTemplateTaskPeriod.AddRow(gridRow));

            gridRow = new TemplateTaskPeriodGridRow(_rowManagerTemplateTaskPeriod, "NumericReadOnlyCell",
                                                    "TotalStatisticAbandonedTasks",
													manager.WordDictionary["TotalStatisticAbandonedTasks"]);
            gridRow.ChartSeriesSettings = ConfigureSetting("TotalStatisticAbandonedTasks");
            GridRows.Add(_rowManagerTemplateTaskPeriod.AddRow(gridRow));

            gridRow = new TemplateTaskPeriodGridRow(_rowManagerTemplateTaskPeriod, "NumericReadOnlyCell",
                                                    "TotalStatisticAnsweredTasks",
													manager.WordDictionary["TotalStatisticAnsweredTasks"]);
            gridRow.ChartSeriesSettings = ConfigureSetting("TotalStatisticAnsweredTasks");
            GridRows.Add(_rowManagerTemplateTaskPeriod.AddRow(gridRow));

            gridRow = new TemplateTaskPeriodGridRow(_rowManagerTemplateTaskPeriod,
                                                    "TimeSpanTotalSecondsReadOnlyCell",
                                                    "TotalStatisticAverageTaskTime",
													manager.WordDictionary["TotalStatisticAverageTaskTime"]);
            gridRow.ChartSeriesSettings = ConfigureSetting("TotalStatisticAverageTaskTime");
            GridRows.Add(_rowManagerTemplateTaskPeriod.AddRow(gridRow));

            gridRow = new TemplateTaskPeriodGridRow(_rowManagerTemplateTaskPeriod,
                                                    "TimeSpanTotalSecondsReadOnlyCell",
                                                    "TotalStatisticAverageAfterTaskTime",
													manager.WordDictionary["TotalStatisticAverageAfterTaskTime"]);
            gridRow.ChartSeriesSettings = ConfigureSetting("TotalStatisticAverageAfterTaskTime");
            GridRows.Add(_rowManagerTemplateTaskPeriod.AddRow(gridRow));
        }

        protected override void OnShowContextMenu(Syncfusion.Windows.Forms.ShowContextMenuEventArgs e)
        {
            ContextMenu.MenuItems[3].Enabled = true;
            base.OnShowContextMenu(e);
        }

    	protected override void OnUpdatingDataPeriodList()
        {
            //TODO! Move template things to template grid class
            if (Owner != null)
            {
                TaskOwnerDay = TaskOwnerHelper.TaskOwnerDays
                    .FirstOrDefault(t => t.CurrentDate == Owner.CurrentDay);
            }
            if (TaskOwnerDay == null)
                return;

            IList<ITemplateTaskPeriod> templateTaskPeriods = new List<ITemplateTaskPeriod>();
            IWorkloadDayBase wlDay = TaskOwnerDay as IWorkloadDayBase;
            if (wlDay != null)
            {
                templateTaskPeriods = wlDay.SortedTaskPeriodList;
            }

            if (templateTaskPeriods.Count > 0)
            {
                DateTime firstTime = templateTaskPeriods[0].Period.StartDateTime;
                DateTime lastTime = templateTaskPeriods.Last().Period.EndDateTime;
                CreateIntervalList(firstTime, lastTime);
            }
            else
            {
                Intervals.Clear();
            }
            _rowManagerTemplateTaskPeriod.BaseDate = TaskOwnerDay.CurrentDate.Date;
            _rowManagerTemplateTaskPeriod.SetDataSource(templateTaskPeriods);
            if (TaskOwnerDay is IWorkloadDay)
            {
                GridRows[0] = new DateHeaderGridRow(DateHeaderType.Date, new List<DateOnly> { TaskOwnerDay.CurrentDate });
                GridRows[1] = new DateHeaderGridRow(DateHeaderType.WeekdayName, new List<DateOnly> { TaskOwnerDay.CurrentDate });
            }
        }

        protected override void OnSelectionChanged(GridSelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);
            if (Owner != null && e.Range.Top > 1)
            {
                GridRow gridRow = GridRows[e.Range.Top] as GridRow;
                if (gridRow != null)
                {
                    Owner.TriggerCellClicked(this, gridRow);
                }
            }
        }

		protected override void SaveAsTemplateOnClick(object sender, EventArgs e)
		{
			IWorkloadDay workloadDay = TaskOwnerDay as WorkloadDay;
			if (workloadDay == null)
				return;
			using(var editWorkloadDayTemplate = new EditWorkloadDayTemplate(workloadDay, workloadDay.OpenHourList, _satisticHelper))
				editWorkloadDayTemplate.ShowDialog(this);
		}

        protected override void TriggerGridRangePasted(GridRangeInfo gridRangeInfo)
        {
            base.TriggerGridRangePasted(gridRangeInfo);
            var workloadDay = TaskOwnerDay as WorkloadDay;
            if (workloadDay == null)
                return;
            workloadDay.Parents.OfType<ISkillDay>().ForEach(s => s.RecalculateStaff());
        }
    }
}
