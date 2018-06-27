using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Intraday.Domain;
using Teleopti.Ccc.Domain.Staffing;

namespace Teleopti.Ccc.Domain.Intraday
{
	public interface IForecastedStaffingToDataSeries
	{
		double?[] DataSeries(IList<StaffingIntervalModel> forecastedStaffingPerSkill, DateTime[] timeSeries);
	}
}