using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Intraday
{
	public interface IStaffingViewModelCreator
	{
		IntradayStaffingViewModel Load(Guid[] skillIdList, DateOnly? dateOnly = null, bool useShrinkage = false);
		IntradayStaffingViewModel Load(Guid[] skillIdList, int dateOffset);
		IEnumerable<IntradayStaffingViewModel> Load(Guid[] skillIdList, DateOnlyPeriod dateOnlyPeriod, bool useShrinkage = false);
	}
}