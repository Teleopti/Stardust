using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Common.Rows;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Common.Controls.Rows
{
	public class SkillDayGridIntervalIssues : SkillDayGridRow
	{
		public SkillDayGridIntervalIssues(RowManagerScheduler<SkillDayGridRow, IDictionary<DateTime, IList<ISkillStaffPeriod>>> rowManager, string cellType, string displayMember, string rowHeaderText): base(rowManager, cellType, displayMember, rowHeaderText)
        {
          
        }

        public override void QueryCellInfo(CellInfo cellInfo)
        {
			base.QueryCellInfo(cellInfo);
			if(cellInfo.ColIndex > 0)
				drawIntervalIssues(cellInfo);
        }

        private void drawIntervalIssues(CellInfo cellInfo)
        {
            var skillStaffPeriods = SkillStaffPeriodList;
            if (skillStaffPeriods == null || skillStaffPeriods.Count() == 0) return;

	        var min = double.MaxValue;
	        var hasIssue = false;
	        foreach (var skillStaffPeriod in skillStaffPeriods)
	        {
		        if (skillStaffPeriod.IntraIntervalValue < min)
		        {
			        min = skillStaffPeriod.IntraIntervalValue;
				}

		        if (skillStaffPeriod.HasIntraIntervalIssue)
		        {
			        hasIssue = true;
			        
		        }
	        }
			if(hasIssue)
				cellInfo.Style.Interior = ColorHelper.IntervalIssueBrush;

	        cellInfo.Style.CellValue = min;
        }
	}
}
