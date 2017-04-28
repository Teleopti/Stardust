using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Staffing
{
	public class CompareProjection
	{
		private readonly IIntervalLengthFetcher _intervalLengthFetcher;
		private readonly IActivityRepository _activityRepository;
		private readonly IDisableDeletedFilter _disableDeletedFilter;

		public CompareProjection(IIntervalLengthFetcher intervalLengthFetcher, IActivityRepository activityRepository, IDisableDeletedFilter disableDeletedFilter)
		{
			_intervalLengthFetcher = intervalLengthFetcher;
			_activityRepository = activityRepository;
			_disableDeletedFilter = disableDeletedFilter;
		}

		public IEnumerable<ActivityResourceInterval> Compare(IScheduleDay scheduleDayBefore, IScheduleDay scheduleDayAfter)
		{
			IEnumerable<IActivity> allActivities;
			using (_disableDeletedFilter.Disable())
			{
				allActivities = _activityRepository.LoadAll();
			}

			var resolution = _intervalLengthFetcher.IntervalLength;
			var beforeIntervals = getActivityIntervals(scheduleDayBefore, resolution);
			var afterIntervals = getActivityIntervals(scheduleDayAfter, resolution);

			var allIntervals = new List<ActivityResourceInterval>();
			allIntervals.AddRange(beforeIntervals);
			allIntervals.AddRange(afterIntervals);

			var totalStart = allIntervals.Min(x => x.Interval.StartDateTime);
			var totaltEnd = allIntervals.Max(x => x.Interval.EndDateTime);

			var output = new List<ActivityResourceInterval>();
			var intervalStart = totalStart;
			while (intervalStart < totaltEnd)
			{
				var before = beforeIntervals.FirstOrDefault(x => x.Interval.StartDateTime == intervalStart);
				var after = afterIntervals.FirstOrDefault(x => x.Interval.StartDateTime == intervalStart);
				intervalStart = intervalStart.AddMinutes(resolution);

				if (before != null && after != null)
				{
					if (before.Activity == after.Activity && Math.Abs(before.Resource - after.Resource) <= 0.01)
						continue;


					if (before.Activity == after.Activity && allActivities.First(x => x.Id == after.Activity).RequiresSkill)
					{
						output.Add(new ActivityResourceInterval
						{
							Interval = before.Interval,
							Activity = before.Activity,
							Resource = after.Resource - before.Resource
						});
						continue;
					}

					//activity is different
					if (allActivities.First(x => x.Id == before.Activity).RequiresSkill)
						output.Add(new ActivityResourceInterval
						{
							Interval = before.Interval,
							Activity = before.Activity,
							Resource = -before.Resource
						});
					if (allActivities.First(x => x.Id == after.Activity).RequiresSkill)
						output.Add(new ActivityResourceInterval
						{
							Interval = after.Interval,
							Activity = after.Activity,
							Resource = after.Resource
						});

				}
			}
			return output;
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
