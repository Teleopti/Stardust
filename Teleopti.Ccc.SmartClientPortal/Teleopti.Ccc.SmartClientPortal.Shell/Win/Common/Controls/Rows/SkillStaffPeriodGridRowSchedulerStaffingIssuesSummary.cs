using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Rows;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Rows
{
    public class SkillStaffPeriodGridRowSchedulerStaffingIssuesSummary : SkillStaffPeriodGridRowScheduler
    {

        private readonly RowManager<SkillStaffPeriodGridRowScheduler, ISkillStaffPeriod> _rowManager;

        public SkillStaffPeriodGridRowSchedulerStaffingIssuesSummary(RowManager<SkillStaffPeriodGridRowScheduler, ISkillStaffPeriod> rowManager, string cellType, string displayMember, string rowHeaderText) 
            : base(rowManager, cellType, displayMember, rowHeaderText)
        {
            _rowManager = rowManager;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public override void QueryCellInfo(CellInfo cellInfo)
        {
            //Let base do it's stuff first
            base.QueryCellInfo(cellInfo);
            if (_rowManager.DataSource.Count == 0 || _rowManager.Intervals.Count == 0 || cellInfo.ColIndex == 0) return;
            drawStaffingIssues(cellInfo);
        }

        private void drawStaffingIssues(CellInfo cellInfo)
        {

            ISkillStaffPeriod skillStaffPeriod = GetObjectAtPositionForInterval(_rowManager, cellInfo.ColIndex,
                                                                                cellInfo.RowHeaderCount);
            if (skillStaffPeriod == null)
                return;

            IAggregateSkillStaffPeriod aggregate = (IAggregateSkillStaffPeriod) skillStaffPeriod;
            if (aggregate.AggregatedStaffingThreshold == StaffingThreshold.Ok)
                return;

            if (aggregate.AggregatedStaffingThreshold == StaffingThreshold.Overstaffing)
            {
                cellInfo.Style.Interior = ColorHelper.OverstaffingBrush;
                cellInfo.Style.CellTipText = UserTexts.Resources.Overstaffing;
            }

            if (aggregate.AggregatedStaffingThreshold == StaffingThreshold.Understaffing)
            {
                cellInfo.Style.Interior = ColorHelper.UnderstaffingBrush;
                cellInfo.Style.CellTipText = UserTexts.Resources.Understaffing;
            }

            if (aggregate.AggregatedStaffingThreshold == StaffingThreshold.CriticalUnderstaffing)
            {
                cellInfo.Style.Interior = ColorHelper.SeriousOverstaffingBrush;
                cellInfo.Style.CellTipText = UserTexts.Resources.SeriousUnderstaffing;
            }
        }
    }
}