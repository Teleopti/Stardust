using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Staffing
{
	public class CompareProjection
	{
		private readonly IIntervalLengthFetcher _intervalLengthFetcher;
		private readonly IActivityRepository _activityRepository;
		private readonly IDisableDeletedFilter _disableDeletedFilter;
		private readonly IPersonSkillProvider _personSkillProvider;

		public CompareProjection(IIntervalLengthFetcher intervalLengthFetcher, IActivityRepository activityRepository, IDisableDeletedFilter disableDeletedFilter, IPersonSkillProvider personSkillProvider)
		{
			_intervalLengthFetcher = intervalLengthFetcher;
			_activityRepository = activityRepository;
			_disableDeletedFilter = disableDeletedFilter;
			_personSkillProvider = personSkillProvider;
		}

		public IEnumerable<SkillCombinationResource> Compare(IScheduleDay scheduleDayBefore, IScheduleDay scheduleDayAfter)
		{
			if (!scheduleDayBefore.Person.Equals(scheduleDayAfter.Person))
				throw new Exception("ScheduleDays must be for the same person!");
			if (scheduleDayBefore.Period != scheduleDayAfter.Period)
				throw new Exception("ScheduleDays must be for the same date!");

			IEnumerable<IActivity> allActivities;
			using (_disableDeletedFilter.Disable())
			{
				allActivities = _activityRepository.LoadAll();
			}
			var personSkills = _personSkillProvider.SkillsOnPersonDate(scheduleDayBefore.Person, scheduleDayAfter.DateOnlyAsPeriod.DateOnly);

			var resolution = _intervalLengthFetcher.IntervalLength;
			var beforeIntervals = getActivityIntervals(scheduleDayBefore, resolution).ToList();
			var afterIntervals = getActivityIntervals(scheduleDayAfter, resolution).ToList();

			var allIntervals = new List<ActivityResourceInterval>();
			allIntervals.AddRange(beforeIntervals);
			allIntervals.AddRange(afterIntervals);

			var totalStart = allIntervals.Min(x => x.Interval.StartDateTime);
			var totaltEnd = allIntervals.Max(x => x.Interval.EndDateTime);

			var output = new List<ActivityResourceInterval>();
			var intervalStart = totalStart;
			while (intervalStart < totaltEnd)
			{
				var before = beforeIntervals.Where(x => x.Interval.StartDateTime == intervalStart).ToList();
				var after = afterIntervals.Where(x => x.Interval.StartDateTime == intervalStart).ToList();
				intervalStart = intervalStart.AddMinutes(resolution);

				var intervalOutput = new List<ActivityResourceInterval>();
				foreach (var activityResourceInterval in before)
				{
					var activity = allActivities.First(x => x.Id == activityResourceInterval.Activity);
					if (activity.RequiresSkill && personSkills.Skills.Select(x => x.Activity).Contains(activity))
						intervalOutput.Add(new ActivityResourceInterval
						{
							Interval = activityResourceInterval.Interval,
							Activity = activityResourceInterval.Activity,
							Resource = -activityResourceInterval.Resource //negative
						});
				}

				foreach (var activityResourceInterval in after)
				{
					var activity = allActivities.First(x => x.Id == activityResourceInterval.Activity);
					if (activity.RequiresSkill && personSkills.Skills.Select(x => x.Activity).Contains(activity))
						intervalOutput.Add(new ActivityResourceInterval
						{
							Interval = activityResourceInterval.Interval,
							Activity = activityResourceInterval.Activity,
							Resource = activityResourceInterval.Resource 
						});
				}

				var intervalOutputsSum = intervalOutput
					.GroupBy(p => new {p.Interval, p.Activity})
					.Select(g => new ActivityResourceInterval {Interval = g.Key.Interval, Activity = g.Key.Activity, Resource = g.Sum(p => p.Resource)});

				output.AddRange(intervalOutputsSum.Where(x => Math.Abs(x.Resource) >= 0.001));
			}

			return output.Select(activityResourceInterval => new SkillCombinationResource
			{
				StartDateTime = activityResourceInterval.Interval.StartDateTime,
				EndDateTime = activityResourceInterval.Interval.EndDateTime,
				Resource = activityResourceInterval.Resource,
				SkillCombination = personSkills.Skills.Where(x => x.Activity.Id.GetValueOrDefault() == activityResourceInterval.Activity).Select(x => x.Id.GetValueOrDefault())
			}).ToList();
		}

		private IEnumerable<ActivityResourceInterval> getActivityIntervals(IScheduleDay scheduleDay, int resolution)
		{
			var activityResouceIntervals = new List<ActivityResourceInterval>();
			var projection = scheduleDay.ProjectionService().CreateProjection();

			var layers = projection.ToResourceLayers(resolution).ToList();
			foreach (var layer in layers)
			{
				activityResouceIntervals.Add(new ActivityResourceInterval
				{
					Activity = layer.PayloadId,
					Interval = layer.Period,
					Resource = layer.Resource
				});
			}
			return activityResouceIntervals;
		}

		
	}

	public class ActivityResourceInterval
	{
		public Guid Activity;
		public DateTimePeriod Interval;
		public double Resource;
	}
}
