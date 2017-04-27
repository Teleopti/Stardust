using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Staffing
{
	public class CompareProjection
	{
		private readonly IIntervalLengthFetcher _intervalLengthFetcher;

		public CompareProjection(IIntervalLengthFetcher intervalLengthFetcher)
		{
			_intervalLengthFetcher = intervalLengthFetcher;
		}

		public IEnumerable<ActivityResourceInterval> Compare(IScheduleDay before, IScheduleDay after)
		{
			var resourceChanges = new List<ActivityResourceInterval>();
			var resolution = _intervalLengthFetcher.IntervalLength;
			resourceChanges.AddRange(remove(before, resolution));
			return resourceChanges;
		}

		private IEnumerable<ActivityResourceInterval> remove(IScheduleDay before, int resolution)
		{
			var activityResouceIntervals = new List<ActivityResourceInterval>();
			var projection = before.ProjectionService().CreateProjection();

			var layers = projection.ToResourceLayers(resolution).ToList();
			foreach (var layer in layers)
			{
				activityResouceIntervals.Add(new ActivityResourceInterval
				{
					Activity = layer.PayloadId,
					Interval = layer.Period,
					Resource = -layer.Resource
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
