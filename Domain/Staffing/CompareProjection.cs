using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;

namespace Teleopti.Ccc.Domain.Staffing
{
	public class CompareProjection
	{
		private readonly ISkillRepository _skillRepository;
		private readonly IDisableDeletedFilter _disableDeletedFilter;
		private readonly IPersonSkillProvider _personSkillProvider;

		public CompareProjection(IDisableDeletedFilter disableDeletedFilter, IPersonSkillProvider personSkillProvider,
			ISkillRepository skillRepository)
		{
			_disableDeletedFilter = disableDeletedFilter;
			_personSkillProvider = personSkillProvider;
			_skillRepository = skillRepository;
		}

		public IEnumerable<SkillCombinationResource> Compare(IScheduleDay scheduleDayBefore, IScheduleDay scheduleDayAfter)
		{
			if (!scheduleDayBefore.Person.Equals(scheduleDayAfter.Person))
				throw new Exception("ScheduleDays must be for the same person!");
			if (scheduleDayBefore.Period != scheduleDayAfter.Period)
				throw new Exception("ScheduleDays must be for the same date!");
			var resolution = 15;
			IEnumerable<ISkill> allSkills;
			using (_disableDeletedFilter.Disable())
			{
				allSkills = _skillRepository.LoadAll();
			}
			var personSkills = _personSkillProvider.SkillsOnPersonDate(scheduleDayBefore.Person, scheduleDayAfter.DateOnlyAsPeriod.DateOnly);
			var allNotDeletedSkill = allSkills.Where(x => !((IDeleteTag) x).IsDeleted).ToArray();

			if(allNotDeletedSkill.Any())
				resolution = allNotDeletedSkill.Select(x => x.DefaultResolution).Min();

			var beforeIntervals = getActivityIntervals(scheduleDayBefore, resolution).ToArray();
			var afterIntervals = getActivityIntervals(scheduleDayAfter, resolution).ToArray();
			
			var allIntervals = beforeIntervals.Concat(afterIntervals).ToArray();

			if (!allIntervals.Any()) return new List<SkillCombinationResource>();

			var output = new List<ActivityResourceInterval>();
			var totalStart = allIntervals.Min(x => x.Interval.StartDateTime);
			var totaltEnd = allIntervals.Max(x => x.Interval.EndDateTime);
			
			var intervalStart = totalStart;
			while (intervalStart < totaltEnd)
			{
				var before = beforeIntervals.Where(x => x.Interval.StartDateTime == intervalStart).ToList();
				var after = afterIntervals.Where(x => x.Interval.StartDateTime == intervalStart).ToList();
				intervalStart = intervalStart.AddMinutes(resolution);

				var intervalOutput = new List<ActivityResourceInterval>();
				foreach (var activityResourceInterval in before)
				{
					var aSkill = allNotDeletedSkill.FirstOrDefault(x => x.Activity.Id == activityResourceInterval.Activity);
					if(aSkill == null)
						continue;
					var skillResolution = aSkill.DefaultResolution;
					var diff = skillResolution / resolution;
					var minute =  TimeHelper.FitToDefaultResolutionRoundDown(
						TimeSpan.FromMinutes(activityResourceInterval.Interval.StartDateTime.Minute), skillResolution).Minutes;
					var splittedStart = activityResourceInterval.Interval.StartDateTime;
					var startOnInterval = new DateTime(splittedStart.Year, splittedStart.Month, splittedStart.Day, splittedStart.Hour,
						minute, 0, DateTimeKind.Utc);
					var interval = new DateTimePeriod(startOnInterval, startOnInterval.AddMinutes(skillResolution));
					if (aSkill.Activity.RequiresSkill && personSkills.Skills.Select(x => x.Activity).Contains(aSkill.Activity))
						intervalOutput.Add(new ActivityResourceInterval
						{
							Interval = interval,
							Activity = activityResourceInterval.Activity,
							Resource = -(activityResourceInterval.Resource / diff) //negative
						});
				}

				foreach (var activityResourceInterval in after)
				{
					var aSkill = allNotDeletedSkill.FirstOrDefault(x => x.Activity.Id == activityResourceInterval.Activity);
					if (aSkill == null)
						continue;
					var skillResolution = aSkill.DefaultResolution;
					var diff = skillResolution / resolution;
					var minute = TimeHelper.FitToDefaultResolutionRoundDown(
						TimeSpan.FromMinutes(activityResourceInterval.Interval.StartDateTime.Minute), skillResolution).Minutes;
					var splittedStart = activityResourceInterval.Interval.StartDateTime;
					var startOnInterval = new DateTime(splittedStart.Year, splittedStart.Month, splittedStart.Day, splittedStart.Hour,
						minute, 0, DateTimeKind.Utc);
					var interval = new DateTimePeriod(startOnInterval, startOnInterval.AddMinutes(skillResolution));
					if (aSkill.Activity.RequiresSkill && personSkills.Skills.Select(x => x.Activity).Contains(aSkill.Activity))
						intervalOutput.Add(new ActivityResourceInterval
						{
							Interval = interval,
							Activity = activityResourceInterval.Activity,
							Resource = activityResourceInterval.Resource/ diff
						});
				}

				var intervalOutputsSum = intervalOutput
					.GroupBy(p => new {p.Interval, p.Activity})
					.Select(g => new ActivityResourceInterval {Interval = g.Key.Interval, Activity = g.Key.Activity, Resource = g.Sum(p => p.Resource)});

				output.AddRange(intervalOutputsSum.Where(x => Math.Abs(x.Resource) >= 0.001));
			}

			var outputSum = output
				.GroupBy(p => new { p.Interval, p.Activity })
				.Select(g => new ActivityResourceInterval { Interval = g.Key.Interval, Activity = g.Key.Activity, Resource = g.Sum(p => p.Resource) });

			return outputSum.Select(activityResourceInterval => new SkillCombinationResource
			{
				StartDateTime = activityResourceInterval.Interval.StartDateTime,
				EndDateTime = activityResourceInterval.Interval.EndDateTime,
				Resource = activityResourceInterval.Resource,
				SkillCombination = personSkills.Skills.Where(x => x.Activity.Id == activityResourceInterval.Activity).Select(x => x.Id.GetValueOrDefault()).ToHashSet()
			}).ToArray();
			//Test from Denver
		}

		private IEnumerable<ActivityResourceInterval> getActivityIntervals(IScheduleDay scheduleDay, int resolution)
		{
			var projection = scheduleDay.ProjectionService().CreateProjection();

			return projection.ToResourceLayers(resolution, scheduleDay.TimeZone).Select(layer => new ActivityResourceInterval
			{
				Activity = layer.PayloadId,
				Interval = layer.Period,
				Resource = layer.Resource
			}).ToArray();
		}
	}

	public class ActivityResourceInterval
	{
		public Guid Activity;
		public DateTimePeriod Interval;
		public double Resource;
	}
}
