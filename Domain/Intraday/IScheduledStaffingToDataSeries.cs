using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Intraday
{
	public interface IScheduledStaffingToDataSeries
	{
		double?[] DataSeries(IList<SkillStaffingIntervalLightModel> scheduledStaffing, DateTime[] timeSeries);
	}
}