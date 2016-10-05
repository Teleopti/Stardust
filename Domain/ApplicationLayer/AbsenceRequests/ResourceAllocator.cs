using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests
{
	public class ResourceAllocator
	{
		private readonly IScheduleForecastSkillReadModelRepository _scheduleForecastSkillReadModelRepository;

		public ResourceAllocator(IScheduleForecastSkillReadModelRepository scheduleForecastSkillReadModelRepository)
		{
			_scheduleForecastSkillReadModelRepository = scheduleForecastSkillReadModelRepository;
		}

		public IEnumerable<StaffingIntervalChange> AllocateResource(IPersonRequest personRequest)
		{
			//Get skillStaffingIntervals 
			var cascadingPersonSkills = personRequest.Person.Period(DateOnly.Today).CascadingSkills();
			var lowestIndex = cascadingPersonSkills.Min(x => x.Skill.CascadingIndex);
			var skills = personRequest.Person.Period(DateOnly.Today).PersonSkillCollection.Select(x => x.Skill).Where(y => !y.IsCascading() || y.CascadingIndex == lowestIndex);
			var skillStaffingIntervals = new List<SkillStaffingInterval>();

			foreach (var skill in skills)
			{
				var intervals = _scheduleForecastSkillReadModelRepository.GetBySkill(skill.Id.GetValueOrDefault(), personRequest.Request.Period.StartDateTime, personRequest.Request.Period.EndDateTime).ToList();
				skillStaffingIntervals.AddRange(intervals);
			}

			//Calculate resources on shortest common periods
			var periods = getShortestCommonPeriods(skillStaffingIntervals, personRequest.Request.Period.StartDateTime, personRequest.Request.Period.EndDateTime).ToList();

			var unmergedIntervalChanges = new List<StaffingIntervalChange>();
			foreach (var period in periods)
			{
				var staffingIntervalsInPeriod = skillStaffingIntervals.Where(x => period.Overlaps(new DateTimePeriod(x.StartDateTime, x.EndDateTime))).ToList(); 
				var totaloverstaffing = staffingIntervalsInPeriod.Sum(interval => interval.StaffingLevel - interval.ForecastWithShrinkage);
				unmergedIntervalChanges.AddRange(staffingIntervalsInPeriod.Select(skillStaffingInterval => new StaffingIntervalChange()
																				  {
																					  StartDateTime = period.StartDateTime,
																					  EndDateTime = period.EndDateTime,
																					  SkillId = skillStaffingInterval.SkillId,
																					  StaffingLevel = -(skillStaffingInterval.StaffingLevel - skillStaffingInterval.ForecastWithShrinkage)/totaloverstaffing
																				  }));
			}

			//Merge intervals for skills with larger intervals than smallest common 
			var mergedIntervalChanges = mergeIntervalChanges(unmergedIntervalChanges, skillStaffingIntervals, periods.FirstOrDefault().EndDateTime.Subtract(periods.FirstOrDefault().StartDateTime));

			return mergedIntervalChanges;
		}




		private static IEnumerable<StaffingIntervalChange> mergeIntervalChanges(List<StaffingIntervalChange> unmergedIntervalChanges, IEnumerable<SkillStaffingInterval> staffingIntervals, TimeSpan shortestPeriod)
		{
			var mergedIntervalChanges = new List<StaffingIntervalChange>();
			foreach (var interval in staffingIntervals)
			{
				if (interval.GetTimeSpan() > shortestPeriod)
				{
					var period = new DateTimePeriod(interval.StartDateTime, interval.EndDateTime);
					var intervalsInPeriod = unmergedIntervalChanges.Where(x => (period.Overlaps(new DateTimePeriod(x.StartDateTime, x.EndDateTime)) && x.SkillId == interval.SkillId));
					var sumOverstaffed = intervalsInPeriod.Sum(i => i.StaffingLevel)*intervalsInPeriod.Count()/interval.GetTimeSpanFactor(shortestPeriod);

					var staffingIntervalChange = new StaffingIntervalChange()
					{
						StartDateTime = interval.StartDateTime,
						EndDateTime = interval.EndDateTime,
						SkillId = interval.SkillId,
						StaffingLevel = sumOverstaffed
					};

					mergedIntervalChanges.Add(staffingIntervalChange);
				}
				else
				{
					mergedIntervalChanges.Add(unmergedIntervalChanges.FirstOrDefault(x => x.SkillId == interval.SkillId && x.StartDateTime == interval.StartDateTime));
				}
			}
			return mergedIntervalChanges;
		}



		private static IEnumerable<DateTimePeriod> getShortestCommonPeriods(List<SkillStaffingInterval> skillStaffingIntervals, DateTime StartDateTime, DateTime EndDateTime)
		{
			var intervalWithShortestSkillInterval = skillStaffingIntervals.Aggregate((interval, nextInterval) => interval.GetTimeSpan() < nextInterval.GetTimeSpan() ? interval : nextInterval);
			var shortestTimeSpan = intervalWithShortestSkillInterval.GetTimeSpan();

			var ret = new List<DateTimePeriod>(){new DateTimePeriod(StartDateTime, StartDateTime.Add(shortestTimeSpan))};
			
			while (ret.Last().EndDateTime < EndDateTime)
			{
				var startDateTime = ret.Last().EndDateTime;
				var endDateTime = startDateTime.Add(shortestTimeSpan);
				ret.Add(new DateTimePeriod(startDateTime, endDateTime));
			}
			return ret;
		}
	}
}