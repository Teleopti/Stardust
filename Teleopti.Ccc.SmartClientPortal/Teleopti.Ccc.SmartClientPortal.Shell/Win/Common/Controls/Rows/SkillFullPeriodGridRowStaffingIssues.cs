using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Rows;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Rows
{
    public class SkillFullPeriodGridRowStaffingIssues:SkillFullPeriodGridRow
    {
        private readonly DayHasOverstaffing overstaffing;
        private readonly DayHasUnderstaffing understaffing;
        private readonly DayHasSeriousUnderstaffing seriousUnderstaffing;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public SkillFullPeriodGridRowStaffingIssues(RowManagerScheduler<SkillFullPeriodGridRow, IDictionary<DateOnlyPeriod, IList<ISkillStaffPeriod>>> rowManager, string cellType, string displayMember, string rowHeaderText, ISkill skill)
            : base(rowManager, cellType, displayMember, rowHeaderText)
        {
            overstaffing = new DayHasOverstaffing(skill);
            understaffing = new DayHasUnderstaffing(skill);
            seriousUnderstaffing = new DayHasSeriousUnderstaffing(skill);
        }

        public override void QueryCellInfo(CellInfo cellInfo)
        {
            base.QueryCellInfo(cellInfo);
            drawStaffingIssues(cellInfo);
        }

        private void drawStaffingIssues(CellInfo cellInfo)
        {
            var skillStaffPeriods = SkillStaffPeriodList;
            if (skillStaffPeriods == null || !skillStaffPeriods.Any()) return;

            StringBuilder toolTip = new StringBuilder();
            if (overstaffing.IsSatisfiedBy(skillStaffPeriods))
            {
                //Prio 3
                cellInfo.Style.Interior = ColorHelper.OverstaffingBrush;
                toolTip.AppendLine(UserTexts.Resources.Overstaffing);
            }

            if (seriousUnderstaffing.IsSatisfiedBy(skillStaffPeriods))
            {
                //Prio 1
                cellInfo.Style.Interior = ColorHelper.SeriousOverstaffingBrush;
                toolTip.AppendLine(UserTexts.Resources.SeriousUnderstaffing);
            }
            else
            {
                if (understaffing.IsSatisfiedBy(skillStaffPeriods))
                {
                    //Prio 2
                    cellInfo.Style.Interior = ColorHelper.UnderstaffingBrush;
                    toolTip.AppendLine(UserTexts.Resources.Understaffing);
                }
            }

            cellInfo.Style.CellTipText = toolTip.ToString();
        }
    }
}
