using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Rows;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Rows
{
	public class SkillDayGridRowMaxSeatsIssues : SkillDayGridRow
	{
		private readonly DayHasAboveMaxSeats _aboveMaxSeats;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public SkillDayGridRowMaxSeatsIssues(RowManagerScheduler<SkillDayGridRow, IDictionary<DateTime, IList<ISkillStaffPeriod>>> rowManager, string cellType, string displayMember, string rowHeaderText)
            : base(rowManager, cellType, displayMember, rowHeaderText)
        {
			_aboveMaxSeats = new DayHasAboveMaxSeats();
        }

        public override void QueryCellInfo(CellInfo cellInfo)
        {
            base.QueryCellInfo(cellInfo);
            drawMaxIssues(cellInfo);
        }

        private void drawMaxIssues(CellInfo cellInfo)
        {
            var skillStaffPeriods = SkillStaffPeriodList;
            if (skillStaffPeriods==null || !skillStaffPeriods.Any()) return;

            StringBuilder toolTip = new StringBuilder();

			if (_aboveMaxSeats.IsSatisfiedBy(skillStaffPeriods))
            {
                cellInfo.Style.Interior = ColorHelper.SeriousOverstaffingBrush;
				toolTip.AppendLine(Resources.MaxSeatLimitBroken);
            }

            cellInfo.Style.CellTipText = toolTip.ToString();
        }
	}
}