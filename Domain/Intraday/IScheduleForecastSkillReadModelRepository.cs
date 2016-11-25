﻿using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Intraday
{
	public interface IScheduleForecastSkillReadModelRepository
	{
		void Persist(IEnumerable<SkillStaffingInterval> items, DateTime timeWhenResourceCalcDataLoaded);
		IEnumerable<SkillStaffingInterval> GetBySkill(Guid skillId, DateTime startDateTime, DateTime endDateTime);
		IDictionary<Guid, DateTime> GetLastCalculatedTime();
	    void PersistChange(StaffingIntervalChange staffingIntervalChanges);
	    IEnumerable<StaffingIntervalChange> GetReadModelChanges(DateTimePeriod dateTimePeriod);
	    IEnumerable<SkillStaffingInterval> ReadMergedStaffingAndChanges(Guid skillId, DateTimePeriod period);
	    IEnumerable<SkillStaffingInterval> ReadMergedStaffingAndChanges(Guid[] ids, DateTimePeriod period);
		IEnumerable<SkillStaffingInterval> GetBySkills(Guid[] guids, DateTime dateTime, DateTime endDateTime);
		void UpdateInsertedDateTime(Guid eventLogOnBusinessUnitId);
	}
}