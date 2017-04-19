using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Intraday
{
	public interface IStaffingViewModelCreator
	{
		IntradayStaffingViewModel Load(Guid[] skillIdList, DateOnly? dateOnly = null, bool useShrinkage = false);
		IEnumerable<IntradayStaffingViewModel> Load(Guid[] skillIdList, DateOnlyPeriod dateOnlyPeriod, bool useShrinkage = false);
	}
}