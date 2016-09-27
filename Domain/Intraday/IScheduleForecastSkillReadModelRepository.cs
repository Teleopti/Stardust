using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Intraday
{
	public interface IScheduleForecastSkillReadModelRepository
	{
		void Persist(IEnumerable<ResourcesDataModel> items);
		IEnumerable<SkillStaffingInterval> GetBySkill(Guid skillId, DateTime startDateTime, DateTime endDateTime);
		IEnumerable<SkillStaffingInterval> GetBySkillArea(Guid skillAreaId, DateTime startDateTime, DateTime endDateTime);
	    DateTime GetLastCalculatedTime();
	}
}