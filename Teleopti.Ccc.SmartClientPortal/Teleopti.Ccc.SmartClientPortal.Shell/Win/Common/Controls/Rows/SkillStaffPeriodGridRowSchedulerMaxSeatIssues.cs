using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Common.Rows;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Rows
{
	public class SkillStaffPeriodGridRowSchedulerMaxSeatIssues : SkillStaffPeriodGridRowScheduler
	{
		private readonly RowManagerScheduler<SkillStaffPeriodGridRowScheduler, ISkillStaffPeriod> _rowManager;

		public SkillStaffPeriodGridRowSchedulerMaxSeatIssues(RowManagerScheduler<SkillStaffPeriodGridRowScheduler, ISkillStaffPeriod> rowManager, string cellType, string displayMember, string rowHeaderText)
			: base(rowManager, cellType, displayMember, rowHeaderText)
        {
            _rowManager = rowManager;
        }

        public override void QueryCellInfo(CellInfo cellInfo)
        {
            //Let base first do its things
            base.QueryCellInfo(cellInfo);
            if (_rowManager.DataSource.Count == 0 || _rowManager.Intervals.Count == 0 || cellInfo.ColIndex == 0) return;
            drawMaxIssues(cellInfo);
        }

		private void drawMaxIssues(CellInfo cellInfo)
		{
			ISkillStaffPeriod skillStaffPeriod = GetObjectAtPositionForInterval(_rowManager, cellInfo.ColIndex, cellInfo.RowHeaderCount);

			if (skillStaffPeriod != null)
			{
				IntervalHasAboveMaxSeats aboveMaxAgents = new IntervalHasAboveMaxSeats();
				if (aboveMaxAgents.IsSatisfiedBy(skillStaffPeriod))
				{
					cellInfo.Style.Interior = ColorHelper.SeriousOverstaffingBrush;
					cellInfo.Style.CellTipText = Resources.MaxSeatLimitBroken;
				}
			}
		}
	}
}