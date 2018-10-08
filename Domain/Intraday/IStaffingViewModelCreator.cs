using System;
using Teleopti.Ccc.Domain.Intraday.ApplicationLayer.ViewModels;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Intraday
{
	public interface IStaffingViewModelCreator
	{
		IntradayStaffingViewModel Load_old(Guid[] skillIdList, DateOnly? dateOnly = null, bool useShrinkage = false);
		IntradayStaffingViewModel Load_old(Guid[] skillIdList, int dateOffset);
	}
}