using System.Collections.Generic;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.WorkShiftCalculation
{
	public interface IScheduleRestrictionExtractor
	{
		IEffectiveRestriction Extract(IList<DateOnly> dateOnlyList, IList<IScheduleMatrixPro> matrixList, ISchedulingOptions schedulingOptions);
	}

	public class ScheduleRestrictionExtractor : IScheduleRestrictionExtractor
	{
		private readonly IScheduleDayEquator _scheduleDayEquator;

		public ScheduleRestrictionExtractor(IScheduleDayEquator scheduleDayEquator)
		{
			_scheduleDayEquator = scheduleDayEquator;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		public IEffectiveRestriction Extract(IList<DateOnly> dateOnlyList, IList<IScheduleMatrixPro> matrixList, ISchedulingOptions schedulingOptions)
		{
			var restriction = new EffectiveRestriction(new StartTimeLimitation(),
														   new EndTimeLimitation(),
														   new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());
			if (dateOnlyList == null) return restriction;

			foreach (var matrix in matrixList)
			{
				foreach (var dateOnly in dateOnlyList)
				{
					var schedule = matrix.GetScheduleDayByKey(dateOnly);
					var schedulePart = schedule.DaySchedulePart();
					if (schedulePart.SignificantPart() == SchedulePartView.MainShift)
					{
						if (restriction.CommonMainShift == null)
						{
							var assignment = schedulePart.AssignmentHighZOrder();
							if (assignment == null) continue;
							var mainShift = assignment.MainShift;
							if (mainShift == null) continue;
							restriction.CommonMainShift = mainShift;
						}
						else
						{
							var assignment = schedulePart.AssignmentHighZOrder();
							if (assignment == null) continue;
							var mainShift = assignment.MainShift;
							if (mainShift == null) continue;
							if (!_scheduleDayEquator.MainShiftEquals(mainShift, restriction.CommonMainShift))
								return null;
						}
					}
				}
			}

			return restriction;
		}
	}
}
