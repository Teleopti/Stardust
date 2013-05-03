using System;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public interface IDailyStaffingMetricsViewModelFactory
	{
		DailyStaffingMetricsViewModel CreateViewModel(Guid skillId, DateTime date);
	}
}