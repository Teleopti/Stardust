using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Common.Rows;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Common.Controls.Rows
{
    public class SkillDayGridRowMinMaxIssuesSummary : SkillDayGridRow
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public SkillDayGridRowMinMaxIssuesSummary(RowManagerScheduler<SkillDayGridRow, IDictionary<DateTime, IList<ISkillStaffPeriod>>> rowManager, string cellType, string displayMember, string rowHeaderText)
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
            if (skillStaffPeriods==null || !skillStaffPeriods.Any()) return;

            StringBuilder toolTip = new StringBuilder();
            
            if (aboveMaxAgents()) //Prio 2
            {
                cellInfo.Style.Interior = ColorHelper.OverstaffingBrush;
                toolTip.AppendLine(UserTexts.Resources.MaximumAgents);
            }
            if (belowMinAgents()) //Prio 1
            {
                cellInfo.Style.Interior = ColorHelper.SeriousOverstaffingBrush;
                toolTip.AppendLine(UserTexts.Resources.MinimumAgents);
            }
            cellInfo.Style.CellTipText = toolTip.ToString();
        }

        private bool aboveMaxAgents()
        {
            foreach (var skillStaffPeriod in SkillStaffPeriodList)
            {
                var aggregate = (IAggregateSkillStaffPeriod)skillStaffPeriod;
                if (aggregate.AggregatedMinMaxStaffAlarm == MinMaxStaffBroken.MaxStaffBroken)
                    return true;
            }
            return false;
        }

        private bool belowMinAgents()
        {
            foreach (var skillStaffPeriod in SkillStaffPeriodList)
            {
                var aggregate = (IAggregateSkillStaffPeriod)skillStaffPeriod;
                if (aggregate.AggregatedMinMaxStaffAlarm == MinMaxStaffBroken.MinStaffBroken)
                    return true;
            }
            return false;
        }
    }
}