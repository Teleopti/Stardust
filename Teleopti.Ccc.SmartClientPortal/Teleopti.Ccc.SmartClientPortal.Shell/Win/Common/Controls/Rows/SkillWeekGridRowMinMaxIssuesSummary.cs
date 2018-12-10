using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Rows;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Rows
{
    public class SkillWeekGridRowMinMaxIssuesSummary : SkillWeekGridRow
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public SkillWeekGridRowMinMaxIssuesSummary(RowManagerScheduler<SkillWeekGridRow, IDictionary<DateOnlyPeriod, IList<ISkillStaffPeriod>>> rowManager, string cellType, string displayMember, string rowHeaderText)
            : base(rowManager, cellType, displayMember, rowHeaderText)
        {

        }

        public override void QueryCellInfo(CellInfo cellInfo)
        {
            base.QueryCellInfo(cellInfo);
            DrawMinMaxIssues(cellInfo);
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		protected void DrawMinMaxIssues(CellInfo cellInfo)
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