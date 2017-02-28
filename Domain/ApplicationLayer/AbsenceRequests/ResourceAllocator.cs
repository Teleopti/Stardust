using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
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

		public IEnumerable<StaffingIntervalChange> AllocateResource(IPersonRequest personRequest, DateTime startDate)
		{
			//Get skillStaffingIntervals 
			var personPeriod = personRequest.Person.Period(new DateOnly(startDate));
			var cascadingPersonSkills = personPeriod.CascadingSkills();
			var lowestIndex = cascadingPersonSkills.Min(x => x.Skill.CascadingIndex);
			var skills = personPeriod.PersonSkillCollection.Select(x => x.Skill).Where(y => !y.IsCascading() || y.CascadingIndex == lowestIndex);
			var skillStaffingIntervals = new List<SkillStaffingInterval>();

			foreach (var skill in skills)
			{
				var intervals = _scheduleForecastSkillReadModelRepository.ReadMergedStaffingAndChanges(skill.Id.GetValueOrDefault(), personRequest.Request.Period).ToList();
				skillStaffingIntervals.AddRange(intervals);
			}

			//Calculate resources on shortest common periods
			var periods = getShortestCommonPeriods(skillStaffingIntervals, personRequest.Request.Period).ToList();

			var unmergedIntervalChanges = new List<StaffingIntervalChange>();
			foreach (var period in periods)
			{
				var staffingIntervalsInPeriod = skillStaffingIntervals.Where(x => period.Intersect(new DateTimePeriod(x.StartDateTime.Utc(), x.EndDateTime.Utc()))).ToList(); 
				var totaloverstaffing = staffingIntervalsInPeriod.Sum(interval => interval.StaffingLevel - interval.Forecast);
				unmergedIntervalChanges.AddRange(staffingIntervalsInPeriod.Select(skillStaffingInterval => new StaffingIntervalChange()
																				  {
																					  StartDateTime = period.StartDateTime,
																					  EndDateTime = period.EndDateTime,
																					  SkillId = skillStaffingInterval.SkillId,
																					  StaffingLevel = -((skillStaffingInterval.StaffingLevel - skillStaffingInterval.Forecast)/totaloverstaffing)*getStaffingFactor(period, personRequest.Request.Period)
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

			if (unmergedIntervalChanges.Any())
			{
				var lookup = unmergedIntervalChanges.ToLookup(i => i.SkillId);
				foreach (var interval in staffingIntervals)
				{
					if (interval.GetTimeSpan() > shortestPeriod)
					{
						var period = new DateTimePeriod(interval.StartDateTime.Utc(), interval.EndDateTime.Utc());
						var intervalsInPeriod = lookup[interval.SkillId].Where(x => period.Intersect(new DateTimePeriod(x.StartDateTime.Utc(), x.EndDateTime.Utc()))).ToArray();
						var sumOverstaffed = intervalsInPeriod.Sum(i => i.StaffingLevel) / interval.DivideBy(shortestPeriod);

						var staffingIntervalChange = new StaffingIntervalChange
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
						var change = lookup[interval.SkillId].FirstOrDefault(x => x.StartDateTime == interval.StartDateTime);
						if (change != null)
						{
							mergedIntervalChanges.Add(change);
						}
					}
				}
			}
			return mergedIntervalChanges;
		}



		private static IEnumerable<DateTimePeriod> getShortestCommonPeriods(List<SkillStaffingInterval> skillStaffingIntervals, DateTimePeriod period)
		{
			if (!skillStaffingIntervals.Any())
				return new List<DateTimePeriod>();

			var start = skillStaffingIntervals.Min(x => x.StartDateTime);

			var intervalWithShortestSkillInterval = skillStaffingIntervals.Aggregate((interval, nextInterval) => interval.GetTimeSpan() < nextInterval.GetTimeSpan() ? interval : nextInterval);
			var shortestTimeSpan = intervalWithShortestSkillInterval.GetTimeSpan();

			var ret = new List<DateTimePeriod>(){new DateTimePeriod(start.Utc(), start.Add(shortestTimeSpan).Utc())};
			
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