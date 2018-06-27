using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Staffing
{
	public interface IScheduledStaffingToDataSeries
	{
		double?[] DataSeries(IList<SkillStaffingIntervalLightModel> scheduledStaffing, DateTime[] timeSeries);
	}
}