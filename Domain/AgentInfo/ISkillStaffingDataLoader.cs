using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo
{
	public interface ISkillStaffingDataLoader
	{
		IList<SkillStaffingData> Load(IList<ISkill> skills, DateOnlyPeriod period, bool useShrinkage, Func<DateOnly, bool> dateFilter = null);
	}
}