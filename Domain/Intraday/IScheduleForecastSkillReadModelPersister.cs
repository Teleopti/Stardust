using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Intraday
{
	public interface IScheduleForecastSkillReadModelPersister
	{
		void Persist(IEnumerable<ResourcesDataModel> items, DateOnly date);
		IEnumerable<SkillStaffingInterval> GetBySkill(Guid skillId, DateOnly dateOnly);
	}
}