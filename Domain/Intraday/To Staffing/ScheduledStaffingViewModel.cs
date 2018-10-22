using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.Staffing
{
	public class ScheduledStaffingViewModel
	{
		public StaffingDataSeries DataSeries { get; set; }

		public bool StaffingHasData { get; set; }

		public IEnumerable<SkillCombinationResourceBpoImportInfoModel> ImportBpoInfoList;
	}
}