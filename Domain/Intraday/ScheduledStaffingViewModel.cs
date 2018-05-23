using System.Collections.Generic;
using Teleopti.Ccc.Domain.Staffing;

namespace Teleopti.Ccc.Domain.Intraday
{
	public class ScheduledStaffingViewModel
	{
		public StaffingDataSeries DataSeries { get; set; }

		public bool StaffingHasData { get; set; }

		public IEnumerable<SkillCombinationResourceBpoImportInfoModel> ImportBpoInfoList;
	}
}