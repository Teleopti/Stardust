using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Rows;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Rows
{
    public class SkillFullPeriodGridRowMinMaxIssues: SkillFullPeriodGridRow
    {
        private readonly DayHasAboveMaxAgents aboveMaxAgents;
        private readonly DayHasBelowMinAgents belowMinAgents;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public SkillFullPeriodGridRowMinMaxIssues(RowManagerScheduler<SkillFullPeriodGridRow, IDictionary<DateOnlyPeriod, IList<ISkillStaffPeriod>>> rowManager, string cellType, string displayMember, string rowHeaderText)
            : base(rowManager, cellType, displayMember, rowHeaderText)
        {
            aboveMaxAgents = new DayHasAboveMaxAgents();
            belowMinAgents = new DayHasBelowMinAgents();
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
            
            if (aboveMaxAgents.IsSatisfiedBy(skillStaffPeriods)) //Prio 2
            {
                cellInfo.Style.Interior = ColorHelper.OverstaffingBrush;
                toolTip.AppendLine(UserTexts.Resources.MaximumAgents);
            }
            if (belowMinAgents.IsSatisfiedBy(skillStaffPeriods)) //Prio 1
            {
                cellInfo.Style.Interior = ColorHelper.SeriousOverstaffingBrush;
                toolTip.AppendLine(UserTexts.Resources.MinimumAgents);
            }
            cellInfo.Style.CellTipText = toolTip.ToString();
        }
    }
}
