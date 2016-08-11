using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.Intraday
{
	public interface IForecastedStaffingLoader
	{
		IList<StaffingIntervalModel> Load(Guid[] skillIdList);
	}
}