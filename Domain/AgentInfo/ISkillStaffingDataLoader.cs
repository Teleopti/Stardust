using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo
{
	public interface ISkillStaffingDataLoader
	{
		IList<SkillStaffingData> Load(IList<ISkill> skills, DateOnlyPeriod period, Func<DateOnly, bool> dateFilter = null);
	}
}