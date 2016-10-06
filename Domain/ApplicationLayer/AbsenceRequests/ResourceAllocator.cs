using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests
{
	public class ResourceAllocator
	{
		private readonly IScheduleForecastSkillReadModelRepository _scheduleForecastSkillReadModelRepository;
		private readonly INow _now;

		public ResourceAllocator(IScheduleForecastSkillReadModelRepository scheduleForecastSkillReadModelRepository, INow now)
		{
			_scheduleForecastSkillReadModelRepository = scheduleForecastSkillReadModelRepository;
			_now = now;
		}

		public IEnumerable<StaffingIntervalChange> AllocateResource(IPersonRequest personRequest)
		{
			//Get skillStaffingIntervals 
			var cascadingPersonSkills = personRequest.Person.Period(new DateOnly(_now.UtcDateTime())).CascadingSkills();
			var lowestIndex = cascadingPersonSkills.Min(x => x.Skill.CascadingIndex);
			var skills = personRequest.Person.Period(new DateOnly(_now.UtcDateTime())).PersonSkillCollection.Select(x => x.Skill).Where(y => !y.IsCascading() || y.CascadingIndex == lowestIndex);
			var skillStaffingIntervals = new List<SkillStaffingInterval>();

			foreach (var skill in skills)
			{
				var intervals = _scheduleForecastSkillReadModelRepository.GetBySkill(skill.Id.GetValueOrDefault(), personRequest.Request.Period.StartDateTime, personRequest.Request.Period.EndDateTime).ToList();
				skillStaffingIntervals.AddRange(intervals);
			}

			//Calculate resources on shortest common periods
			var periods = getShortestCommonPeriods(skillStaffingIntervals, personRequest.Request.Period).ToList();

			var unmergedIntervalChanges = new List<StaffingIntervalChange>();
			foreach (var period in periods)
			{
				var staffingIntervalsInPeriod = skillStaffingIntervals.Where(x => period.Intersect(new DateTimePeriod(x.StartDateTime.Utc(), x.EndDateTime.Utc()))).ToList(); 
				var totaloverstaffing = staffingIntervalsInPeriod.Sum(interval => interval.StaffingLevel - interval.ForecastWithShrinkage);
				unmergedIntervalChanges.AddRange(staffingIntervalsInPeriod.Select(skillStaffingInterval => new StaffingIntervalChange()
																				  {
																					  StartDateTime = period.StartDateTime,
																					  EndDateTime = period.EndDateTime,
																					  SkillId = skillStaffingInterval.SkillId,
																					  StaffingLevel = -((skillStaffingInterval.StaffingLevel - skillStaffingInterval.ForecastWithShrinkage)/totaloverstaffing)*getStaffingFactor(period, personRequest.Request.Period)
																				  }));
			}

			//Merge intervals for skills with larger intervals than smallest common 
			var mergedIntervalChanges = mergeIntervalChanges(unmergedIntervalChanges, skillStaffingIntervals, periods.FirstOrDefault().EndDateTime.Subtract(periods.FirstOrDefault().StartDateTime));

			return mergedIntervalChanges;
		}

		private static double getStaffingFactor(DateTimePeriod period, DateTimePeriod requestPeriod)
		{
			var overlappingPeriod = period.Intersection(requestPeriod);
			double factor = 0;
			if (overlappingPeriod.HasValue)
			{
				var overlappingTimeSpan = overlappingPeriod.Value.EndDateTime.Subtract(overlappingPeriod.Value.StartDateTime);
				var periodTimeSpan = period.EndDateTime.Subtract(period.StartDateTime);
				factor = (double)overlappingTimeSpan.Ticks/periodTimeSpan.Ticks;
			}
		//	var factor = (double) requestPeriod.EndDateTime.Subtract(requestPeriod.StartDateTime).Ticks/period.EndDateTime.Subtract(period.StartDateTime).Ticks;
			return factor < 1 ? factor : 1;
		}


		private static IEnumerable<StaffingIntervalChange> mergeIntervalChanges(List<StaffingIntervalChange> unmergedIntervalChanges, IEnumerable<SkillStaffingInterval> staffingIntervals, TimeSpan shortestPeriod)
		{
			var mergedIntervalChanges = new List<StaffingIntervalChange>();
			foreach (var interval in staffingIntervals)
			{
				if (interval.GetTimeSpan() > shortestPeriod)
				{
					var period = new DateTimePeriod(interval.StartDateTime.Utc(), interval.EndDateTime.Utc());
					var intervalsInPeriod = unmergedIntervalChanges.Where(x => period.Intersect(new DateTimePeriod(x.StartDateTime.Utc(), x.EndDateTime.Utc())) && x.SkillId == interval.SkillId).ToList();
					var sumOverstaffed = intervalsInPeriod.Sum(i => i.StaffingLevel)/interval.divideBy(shortestPeriod);

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



		private static IEnumerable<DateTimePeriod> getShortestCommonPeriods(List<SkillStaffingInterval> skillStaffingIntervals, DateTimePeriod period)
		{
			var intervalWithShortestSkillInterval = skillStaffingIntervals.Aggregate((interval, nextInterval) => interval.GetTimeSpan() < nextInterval.GetTimeSpan() ? interval : nextInterval);
			var shortestTimeSpan = intervalWithShortestSkillInterval.GetTimeSpan();

			var ret = new List<DateTimePeriod>(){new DateTimePeriod(period.StartDateTime.Utc(), period.StartDateTime.Add(shortestTimeSpan).Utc())};
			
			while (ret.Last().EndDateTime < period.EndDateTime)
			{
				var startDateTime = ret.Last().EndDateTime;
				var endDateTime = startDateTime.Add(shortestTimeSpan);
				ret.Add(new DateTimePeriod(startDateTime.Utc(), endDateTime.Utc()));
			}
			return ret;
		}
	}
}