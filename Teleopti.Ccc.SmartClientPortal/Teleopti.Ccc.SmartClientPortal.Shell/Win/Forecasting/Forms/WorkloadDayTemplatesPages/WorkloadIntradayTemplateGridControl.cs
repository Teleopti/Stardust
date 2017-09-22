using System;
using System.Collections.Generic;
using System.Linq;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Rows;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Rows;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Forecasting;
using Teleopti.Ccc.WinCode.Common.Chart;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms.WorkloadDayTemplatesPages
{
    public class WorkloadIntradayTemplateGridControl : WorkloadIntradayGridControl
    {
        private readonly ISkillType _skillType;

        public WorkloadIntradayTemplateGridControl(ITaskOwner taskOwner, TaskOwnerHelper taskOwnerHelper, TimeZoneInfo timeZone, int resolution, AbstractDetailView owner, ISkillType skillType, IStatisticHelper statisticsHelper)
            : base(taskOwner, taskOwnerHelper, timeZone, resolution, owner, new ChartSettings(),skillType,statisticsHelper)
        {
            _skillType = skillType;
        }

        /// <summary>
        /// Initializes the grid, this grid "way"
        /// </summary>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-05-20
        /// </remarks>
        protected override void InitializeGrid()
        {
            base.InitializeGrid();
            Cols.Size[0] = ColorHelper.GridHeaderColumnWidth();
            if (TaskOwnerDay != null)
            {
                Rows.HeaderCount = 0;
                Rows.SetFrozenCount(0, false);
            }
        }

        protected override void CreateGridRows()
        {
            base.CreateGridRows();
            GridRows.Clear();
            RowManagerTemplateTaskPeriod.Rows.Clear();
            TextManager manager = new TextManager(_skillType);
			if (_skillType.ForecastSource != ForecastSource.InboundTelephony && _skillType.ForecastSource != ForecastSource.Chat)
            {
                GridRows.Add(new IntervalHeaderGridRow(Intervals));
                GridRows.Add(RowManagerTemplateTaskPeriod.AddRow(new TemplateTaskPeriodGridRow(RowManagerTemplateTaskPeriod, "NumericCell",
                    "Tasks", manager.WordDictionary["Tasks"])));

                GridRows.Add(RowManagerTemplateTaskPeriod.AddRow(new TemplateTaskPeriodGridRow(RowManagerTemplateTaskPeriod, "TimeSpanLongHourMinuteSecondOnlyPositiveCellModel",
                    "AverageTaskTime", manager.WordDictionary["AverageTaskTime"])));

                GridRows.Add(RowManagerTemplateTaskPeriod.AddRow(new TemplateTaskPeriodGridRow(RowManagerTemplateTaskPeriod, "TimeSpanLongHourMinuteSecondOnlyPositiveCellModel",
                    "AverageAfterTaskTime", manager.WordDictionary["AverageAfterTaskTime"])));
            }
            else
            {
                GridRows.Add(new IntervalHeaderGridRow(Intervals));
                GridRows.Add(RowManagerTemplateTaskPeriod.AddRow(new TemplateTaskPeriodGridRow(RowManagerTemplateTaskPeriod, "NumericCell",
					"Tasks", manager.WordDictionary["Tasks"])));

                GridRows.Add(RowManagerTemplateTaskPeriod.AddRow(new TemplateTaskPeriodGridRow(RowManagerTemplateTaskPeriod, "TimeSpanTotalSecondsCell",
					"AverageTaskTime", manager.WordDictionary["AverageTaskTime"])));

                GridRows.Add(RowManagerTemplateTaskPeriod.AddRow(new TemplateTaskPeriodGridRow(RowManagerTemplateTaskPeriod, "TimeSpanTotalSecondsCell",
					"AverageAfterTaskTime", manager.WordDictionary["AverageAfterTaskTime"])));
            }

        }

        protected override void OnSaveCellInfo(GridSaveCellInfoEventArgs e)
        {
            base.OnSaveCellInfo(e);

            if (e.RowIndex > 0)
            {
                TriggerModifyRows((TaskPeriodType)(e.RowIndex - 1));
            }
        }

		protected override void OnShowContextMenu(Syncfusion.Windows.Forms.ShowContextMenuEventArgs e)
        {
            base.OnShowContextMenu(e);
            ContextMenu.MenuItems[3].Enabled = false;
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
            RowManagerTemplateTaskPeriod.BaseDate = TaskOwnerDay.CurrentDate.Date;
            RowManagerTemplateTaskPeriod.SetDataSource(templateTaskPeriods);
        }

        /// <summary>
        /// Updates the grid row to chart.
        /// </summary>
        /// <param name="taskPeriodType">Type of the task period.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-30
        /// </remarks>
        public void UpdateGridRowToChart(TaskPeriodType taskPeriodType)
        {
            var row = (int)taskPeriodType + 1;
            var range = GridRangeInfo.Cells(row, Cols.HeaderCount + 1, row, ColCount);
			RefreshGridNoResize();
            TriggerDataToChart(range);
        }
    }
}
