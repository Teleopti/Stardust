using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Common.Rows;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Common.Controls.Rows
{
    /// <summary>
    /// Row class for Staffing issues
    /// </summary>
    /// <remarks>
    /// Created by: peterwe
    /// Created date: 9/24/2008
    /// </remarks>
    public class SkillStaffPeriodGridRowSchedulerStaffingIssues : SkillStaffPeriodGridRowScheduler
    {
        private readonly ISkill _skill;
        private readonly RowManager<SkillStaffPeriodGridRowScheduler, ISkillStaffPeriod> _rowManager;

        public SkillStaffPeriodGridRowSchedulerStaffingIssues(RowManager<SkillStaffPeriodGridRowScheduler, ISkillStaffPeriod> rowManager, string cellType, string displayMember, string rowHeaderText, ISkill skill) 
            : base(rowManager, cellType, displayMember, rowHeaderText)
        {
            _rowManager = rowManager;
            _skill = skill; //need skill to calculate staffingIssues
        }

        public override void QueryCellInfo(CellInfo cellInfo)
        {
            //Let base do it's stuff first
            base.QueryCellInfo(cellInfo);
            if (_rowManager.DataSource.Count == 0 || _rowManager.Intervals.Count == 0 || cellInfo.ColIndex == 0) return;
            drawStaffingIssues(cellInfo);
        }

        private void drawStaffingIssues(CellInfo cellInfo)
        {
            if (_skill == null)
                return;

            ISkillStaffPeriod skillStaffPeriod = GetObjectAtPositionForInterval(_rowManager, cellInfo.ColIndex,
                                                                                cellInfo.RowHeaderCount);
            if (skillStaffPeriod == null)
                return;

            IntervalHasOverstaffing overstaffing = new IntervalHasOverstaffing(_skill);
            if (overstaffing.IsSatisfiedBy(skillStaffPeriod))
            {
                cellInfo.Style.Interior = ColorHelper.OverstaffingBrush;
                cellInfo.Style.CellTipText = UserTexts.Resources.Overstaffing;
            }

            IntervalHasUnderstaffing understaffing = new IntervalHasUnderstaffing(_skill);
            if (understaffing.IsSatisfiedBy(skillStaffPeriod))
            {
                cellInfo.Style.Interior = ColorHelper.UnderstaffingBrush;
                cellInfo.Style.CellTipText = UserTexts.Resources.Understaffing;
            }
            IntervalHasSeriousUnderstaffing seriousUnderstaffing = new IntervalHasSeriousUnderstaffing(_skill);
            if (seriousUnderstaffing.IsSatisfiedBy(skillStaffPeriod))
            {
                cellInfo.Style.Interior = ColorHelper.SeriousOverstaffingBrush;
                cellInfo.Style.CellTipText = UserTexts.Resources.SeriousUnderstaffing;
            }

        }
    }
}