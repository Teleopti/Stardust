using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Intraday
{
	public interface IForecastedStaffingProvider
	{
		IList<StaffingIntervalModel> StaffingPerSkill(IDictionary<ISkill, IEnumerable<ISkillDay>> skillDays, int minutesPerInterval, DateOnly? dateOnly, bool useShrinkage);
	}
}