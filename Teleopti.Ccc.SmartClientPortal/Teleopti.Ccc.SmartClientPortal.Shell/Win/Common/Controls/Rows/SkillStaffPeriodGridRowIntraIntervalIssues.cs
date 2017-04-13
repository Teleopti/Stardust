using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Common.Rows;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Rows
{
	public class SkillStaffPeriodGridRowIntraIntervalIssues : SkillStaffPeriodGridRowScheduler
	{
		private readonly RowManagerScheduler<SkillStaffPeriodGridRowScheduler, ISkillStaffPeriod> _rowManager;

		public SkillStaffPeriodGridRowIntraIntervalIssues(RowManagerScheduler<SkillStaffPeriodGridRowScheduler, ISkillStaffPeriod> rowManager, string cellType, string displayMember, string rowHeaderText): base(rowManager, cellType, displayMember, rowHeaderText)
        {
            _rowManager = rowManager;
        }

        public override void QueryCellInfo(CellInfo cellInfo)
        {
            base.QueryCellInfo(cellInfo);
            if (_rowManager.DataSource.Count == 0 || _rowManager.Intervals.Count == 0 || cellInfo.ColIndex == 0) return;
			drawIntraIntervalIssues(cellInfo);
        }

        private void drawIntraIntervalIssues(CellInfo cellInfo)
        {
            var skillStaffPeriod = GetObjectAtPositionForInterval(_rowManager, cellInfo.ColIndex,cellInfo.RowHeaderCount);
	        if (skillStaffPeriod == null) return;

	        if (skillStaffPeriod.HasIntraIntervalIssue)
	        {
		        cellInfo.Style.Interior = ColorHelper.IntervalIssueBrush;
		        cellInfo.Style.CellTipText = UserTexts.Resources.IntraIntervalIssue;
	        }
        }
	}
}
