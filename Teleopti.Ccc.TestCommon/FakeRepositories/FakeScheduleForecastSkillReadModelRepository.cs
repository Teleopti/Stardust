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
	    private List<SkillStaffingInterval> _fakeStaffingList  = new List<SkillStaffingInterval>();
		private List<CustomStaffingIntervalChange> _readModelChanges = new List<CustomStaffingIntervalChange>();
		public DateTime UtcNow = DateTime.UtcNow;

		public void Persist(IEnumerable<SkillStaffingInterval> items, DateTime timeWhenResourceCalcDataLoaded)
		{
			_fakeStaffingList.AddRange(items);

	        var filteredChanges = _readModelChanges.Where(x => x.InsertedOn >= timeWhenResourceCalcDataLoaded);
			if(filteredChanges.Any())
				_readModelChanges = filteredChanges.ToList();

        }

		public IEnumerable<SkillStaffingInterval> GetBySkill(Guid skillId, DateTime startDateTime, DateTime endDateTime)
        {
	       return _fakeStaffingList.Where(x => x.SkillId == skillId && 
					((x.StartDateTime < startDateTime && x.EndDateTime > startDateTime) || (x.StartDateTime >= startDateTime && x.StartDateTime < endDateTime) ));
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
            _readModelChanges.Add(new CustomStaffingIntervalChange() {InsertedOn = UtcNow,StaffingIntervalChange = staffingIntervalChanges});
        }

	    

	    public IEnumerable<StaffingIntervalChange> GetReadModelChanges(DateTimePeriod dateTimePeriod)
	    {
		    return
			    _readModelChanges.Where(
				    x =>
					    (x.StaffingIntervalChange.StartDateTime < dateTimePeriod.StartDateTime && x.StaffingIntervalChange.EndDateTime > dateTimePeriod.StartDateTime) ||
					    (x.StaffingIntervalChange.StartDateTime >= dateTimePeriod.StartDateTime&& dateTimePeriod.EndDateTime > x.StaffingIntervalChange.StartDateTime)).Select(y => y.StaffingIntervalChange);
	    }

        public IEnumerable<SkillStaffingInterval> ReadMergedStaffingAndChanges(Guid skillId, DateTimePeriod period)
        {
            var intervals = GetBySkill(skillId, period.StartDateTime, period.EndDateTime);
            var changes = GetReadModelChanges(period);
            foreach (var change in changes.Where(x => x.SkillId == skillId))
            {
                var staffingInterval = intervals.FirstOrDefault(x => x.StartDateTime == change.StartDateTime && x.EndDateTime == change.EndDateTime);
                if (staffingInterval != null)
                    staffingInterval.StaffingLevel += change.StaffingLevel;
            }
            return intervals;
        }

        public DateTime LastCalculatedDate { get; set; }
    }

	internal class CustomStaffingIntervalChange
	{
		public DateTime InsertedOn { get; set; }
		public StaffingIntervalChange StaffingIntervalChange { get; set; }
	}
}