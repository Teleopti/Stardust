using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
    public class FakeScheduleForecastSkillReadModelRepository : IScheduleForecastSkillReadModelRepository
    {
	    private readonly List<SkillStaffingInterval> _fakeStaffingList  = new List<SkillStaffingInterval>();
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
            var skillStaffingIntervals = GetBySkill(skillId, period.StartDateTime, period.EndDateTime).ToList();
            var mergedStaffingIntervals = new List<SkillStaffingInterval>();
            var intervalChanges = GetReadModelChanges(period).Where(x => x.SkillId == skillId).ToList();
            if (intervalChanges.Any())
            {
                skillStaffingIntervals.ForEach(interval =>
                {
                    var changes =
                        intervalChanges.Where(x => x.StartDateTime == interval.StartDateTime && x.EndDateTime == interval.EndDateTime).ToList();
                    if (changes.Any())
                    {
                        interval.StaffingLevel += changes.Sum((x => x.StaffingLevel));
                    }
                    mergedStaffingIntervals.Add(interval);
                });
            }
            else
            {
                mergedStaffingIntervals = skillStaffingIntervals.ToList();
            }
            return mergedStaffingIntervals;
        }

        public DateTime LastCalculatedDate { get; set; }
    }

	internal class CustomStaffingIntervalChange
	{
		public DateTime InsertedOn { get; set; }
		public StaffingIntervalChange StaffingIntervalChange { get; set; }
	}
}