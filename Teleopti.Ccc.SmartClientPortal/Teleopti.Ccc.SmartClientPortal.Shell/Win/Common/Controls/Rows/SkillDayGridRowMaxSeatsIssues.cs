using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Common.Rows;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Common.Controls.Rows
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
            if (skillStaffPeriods==null || skillStaffPeriods.Count() == 0) return;

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