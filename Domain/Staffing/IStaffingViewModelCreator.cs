using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Staffing
{
	public interface IStaffingViewModelCreator
	{
		ScheduledStaffingViewModel Load(Guid[] skillIds, DateOnly? dateInLocalTime = null,
			bool useShrinkage = false);
	}
}