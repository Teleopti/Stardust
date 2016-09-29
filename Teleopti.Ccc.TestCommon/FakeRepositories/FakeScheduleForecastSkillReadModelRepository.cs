﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
    public class FakeScheduleForecastSkillReadModelRepository : IScheduleForecastSkillReadModelRepository
    {
	    public Dictionary<Guid, List<SkillStaffingInterval>> FakeStaffingList { get; set; }


		public void Persist(IEnumerable<ResourcesDataModel> items)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<SkillStaffingInterval> GetBySkill(Guid skillId, DateTime startDateTime, DateTime endDateTime)
        {
	        if (FakeStaffingList.ContainsKey(skillId))
	        {
		        var thatSkill = FakeStaffingList[skillId];
		        return thatSkill.Where(x => x.StartDateTime >= startDateTime && x.EndDateTime <= endDateTime);
	        }
			return new List<SkillStaffingInterval>();

        }

        public IEnumerable<SkillStaffingInterval> GetBySkillArea(Guid skillAreaId, DateTime startDateTime, DateTime endDateTime)
        {
            throw new NotImplementedException();
        }

        public DateTime GetLastCalculatedTime()
        {
            return LastCalculatedDate;
        }

        public void PersistChange(StaffingIntervalChange staffingIntervalChanges)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<StaffingIntervalChange> GetReadModelChanges(DateTimePeriod dateTimePeriod)
        {
            throw new NotImplementedException();
        }

        public DateTime LastCalculatedDate { get; set; }
    }
}