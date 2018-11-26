using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.Staffing
{
	public interface IForecastedStaffingToDataSeries
	{
		double?[] DataSeries(IList<StaffingIntervalModel> forecastedStaffingPerSkill, DateTime[] timeSeries);
	}
}