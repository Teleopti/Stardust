using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Rows;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Rows
{
    public class SkillStaffPeriodGridRowSchedulerMinMaxIssuesSummary : SkillStaffPeriodGridRowScheduler
    {
        private readonly RowManagerScheduler<SkillStaffPeriodGridRowScheduler, ISkillStaffPeriod> _rowManager;

        public SkillStaffPeriodGridRowSchedulerMinMaxIssuesSummary(RowManagerScheduler<SkillStaffPeriodGridRowScheduler, ISkillStaffPeriod> rowManager, string cellType, string displayMember, string rowHeaderText)
            : base(rowManager, cellType, displayMember, rowHeaderText)
        {
            _rowManager = rowManager;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public override void QueryCellInfo(CellInfo cellInfo)
        {
            //Let base first do its things
            base.QueryCellInfo(cellInfo);
            if (_rowManager.DataSource.Count == 0 || _rowManager.Intervals.Count == 0 || cellInfo.ColIndex == 0) return;
            drawMinMaxIssues(cellInfo);
        }

        private void drawMinMaxIssues(CellInfo cellInfo)
        {
            ISkillStaffPeriod skillStaffPeriod = GetObjectAtPositionForInterval(_rowManager, cellInfo.ColIndex,
                                                                                cellInfo.RowHeaderCount);
            if (skillStaffPeriod == null)
                return;

            var aggregate = (IAggregateSkillStaffPeriod)skillStaffPeriod;
            if(aggregate.AggregatedMinMaxStaffAlarm == MinMaxStaffBroken.Ok)
                return;

            if(aggregate.AggregatedMinMaxStaffAlarm == MinMaxStaffBroken.BothBroken)
            {
                cellInfo.Style.Interior = ColorHelper.SeriousOverstaffingBrush;
                cellInfo.Style.CellTipText = UserTexts.Resources.MaximumAgents + Environment.NewLine + UserTexts.Resources.MinimumAgents;
            }

            if(aggregate.AggregatedMinMaxStaffAlarm == MinMaxStaffBroken.MinStaffBroken)
            {
                cellInfo.Style.Interior = ColorHelper.SeriousOverstaffingBrush;
                cellInfo.Style.CellTipText = UserTexts.Resources.MinimumAgents;
                return;
            }

            if (aggregate.AggregatedMinMaxStaffAlarm == MinMaxStaffBroken.MaxStaffBroken)
            {
                cellInfo.Style.Interior = ColorHelper.OverstaffingBrush;
                cellInfo.Style.CellTipText = UserTexts.Resources.MaximumAgents;
            }
            
        }
    }
}