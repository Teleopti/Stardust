using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Rows;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Rows
{
    public class SkillWeekGridRowStaffingIssuesSummary : SkillWeekGridRow
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public SkillWeekGridRowStaffingIssuesSummary(RowManagerScheduler<SkillWeekGridRow, IDictionary<DateOnlyPeriod, IList<ISkillStaffPeriod>>> rowManager, string cellType, string displayMember, string rowHeaderText)
            : base(rowManager, cellType, displayMember, rowHeaderText)
        {

        }

        public override void QueryCellInfo(CellInfo cellInfo)
        {
            base.QueryCellInfo(cellInfo);
            drawMinMaxIssues(cellInfo);
        }

        private void drawMinMaxIssues(CellInfo cellInfo)
        {
            var skillStaffPeriods = SkillStaffPeriodList;
            if (skillStaffPeriods == null || !skillStaffPeriods.Any()) return;

            StringBuilder toolTip = new StringBuilder();

            if (overStaffing()) //Prio 3
            {
                cellInfo.Style.Interior = ColorHelper.OverstaffingBrush;
                toolTip.AppendLine(UserTexts.Resources.Overstaffing);
            }
            if (underStaffing()) //Prio 2
            {
                cellInfo.Style.Interior = ColorHelper.UnderstaffingBrush;
                toolTip.AppendLine(UserTexts.Resources.Understaffing);
            }
            if (criticalUnderStaffing()) //Prio 1
            {
                cellInfo.Style.Interior = ColorHelper.SeriousOverstaffingBrush;
                toolTip.AppendLine(UserTexts.Resources.SeriousUnderstaffing);
            }
            cellInfo.Style.CellTipText = toolTip.ToString();
        }

        private bool overStaffing()
        {
            foreach (var skillStaffPeriod in SkillStaffPeriodList)
            {
                var aggregate = (IAggregateSkillStaffPeriod)skillStaffPeriod;
                if (aggregate.AggregatedStaffingThreshold == StaffingThreshold.Overstaffing)
                    return true;
            }
            return false;
        }

        private bool underStaffing()
        {
            foreach (var skillStaffPeriod in SkillStaffPeriodList)
            {
                var aggregate = (IAggregateSkillStaffPeriod)skillStaffPeriod;
                if (aggregate.AggregatedStaffingThreshold == StaffingThreshold.Understaffing)
                    return true;
            }
            return false;
        }

        private bool criticalUnderStaffing()
        {
            foreach (var skillStaffPeriod in SkillStaffPeriodList)
            {
                var aggregate = (IAggregateSkillStaffPeriod)skillStaffPeriod;
                if (aggregate.AggregatedStaffingThreshold == StaffingThreshold.CriticalUnderstaffing)
                    return true;
            }
            return false;
        }
    }
}