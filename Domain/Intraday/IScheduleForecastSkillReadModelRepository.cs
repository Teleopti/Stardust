using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Intraday
{
	public interface IScheduleForecastSkillReadModelRepository
	{
		void Persist(IEnumerable<SkillStaffingInterval> items, DateTime timeWhenResourceCalcDataLoaded);
		IEnumerable<SkillStaffingInterval> GetBySkill(Guid skillId, DateTime startDateTime, DateTime endDateTime);
		IEnumerable<SkillStaffingInterval> GetBySkillArea(Guid skillAreaId, DateTime startDateTime, DateTime endDateTime);
	    DateTime GetLastCalculatedTime();
	    void PersistChange(StaffingIntervalChange staffingIntervalChanges);
	    IEnumerable<StaffingIntervalChange> GetReadModelChanges(DateTimePeriod dateTimePeriod);
	}
}