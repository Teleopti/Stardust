using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public interface IScheduleRestrictionExtractor
	{
		IEffectiveRestriction Extract(IList<DateOnly> dateOnlyList, IList<IScheduleMatrixPro> matrixList, ISchedulingOptions schedulingOptions, TimeZoneInfo timeZone);
	}

	public class ScheduleRestrictionExtractor : IScheduleRestrictionExtractor
	{
		private readonly IScheduleDayEquator _scheduleDayEquator;

		public ScheduleRestrictionExtractor(IScheduleDayEquator scheduleDayEquator)
		{
			_scheduleDayEquator = scheduleDayEquator;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods",
			MessageId = "1")]
		public IEffectiveRestriction Extract(IList<DateOnly> dateOnlyList, IList<IScheduleMatrixPro> matrixList,
		                                     ISchedulingOptions schedulingOptions, TimeZoneInfo timeZone)
		{
			IEffectiveRestriction restriction = new EffectiveRestriction(new StartTimeLimitation(),
			                                                             new EndTimeLimitation(),
			                                                             new WorkTimeLimitation(), null, null, null,
			                                                             new List<IActivityRestriction>());
			if (dateOnlyList == null)
				return restriction;

			if (schedulingOptions.UseLevellingSameShift)
			{
				var sameShiftRestriction = extractSameShift(dateOnlyList, matrixList);
				if (sameShiftRestriction == null) return null;
				restriction = restriction.Combine(sameShiftRestriction);
			}
			if (schedulingOptions.UseLevellingSameStartTime)
			{
				var sameStartRestriction = extractSameStartTime(dateOnlyList, matrixList, timeZone);
				if (sameStartRestriction == null) return null;
				restriction = restriction.Combine(sameStartRestriction);
			}
			if (schedulingOptions.UseLevellingSameEndTime)
			{
				var sameEndRestriction = extractSameEndTime(dateOnlyList, matrixList, timeZone);
				if (sameEndRestriction == null) return null;
				restriction = restriction.Combine(sameEndRestriction);
			}
			if (schedulingOptions.UseLevellingSameShiftCategory)
			{
				var sameShiftCategory = extractSameShiftCategory(dateOnlyList, matrixList);
				if (sameShiftCategory == null) return null;
				restriction = restriction.Combine(sameShiftCategory);
			}
			return restriction;
		}

		private IEffectiveRestriction extractSameShift(IList<DateOnly> dateOnlyList, IEnumerable<IScheduleMatrixPro> matrixList)
		{
			var restriction = new EffectiveRestriction(new StartTimeLimitation(),
													   new EndTimeLimitation(),
													   new WorkTimeLimitation(), null, null, null,
													   new List<IActivityRestriction>());
			foreach (var matrix in matrixList)
			{
				foreach (var dateOnly in dateOnlyList)
				{
					var schedule = matrix.GetScheduleDayByKey(dateOnly);
					if (schedule == null)
						continue;

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
							if (!_scheduleDayEquator.MainShiftBasicEquals(mainShift, restriction.CommonMainShift))
								return null;
						}
					}
				}
			}
			return restriction;
		}
		
		private IEffectiveRestriction extractSameShiftCategory(IList<DateOnly> dateOnlyList, IEnumerable<IScheduleMatrixPro> matrixList)
		{
			var restriction = new EffectiveRestriction(new StartTimeLimitation(),
													   new EndTimeLimitation(),
													   new WorkTimeLimitation(), null, null, null,
													   new List<IActivityRestriction>());
			foreach (var matrix in matrixList)
			{
				foreach (var dateOnly in dateOnlyList)
				{
					var schedule = matrix.GetScheduleDayByKey(dateOnly);
					if (schedule == null)
						continue;

					var schedulePart = schedule.DaySchedulePart();
					if (schedulePart.SignificantPart() == SchedulePartView.MainShift)
					{
						if (restriction.ShiftCategory == null)
						{
							var assignment = schedulePart.AssignmentHighZOrder();
							if (assignment == null) continue;
							var mainShift = assignment.MainShift;
							if (mainShift == null) continue;
							restriction.ShiftCategory = mainShift.ShiftCategory;
						}
						else
						{
							var assignment = schedulePart.AssignmentHighZOrder();
							if (assignment == null) continue;
							var mainShift = assignment.MainShift;
							if (mainShift == null) continue;
							if (restriction.ShiftCategory != mainShift.ShiftCategory)
								return null;
						}
					}
				}
			}
			return restriction;
		}

		private static IEffectiveRestriction extractSameStartTime(IList<DateOnly> dateOnlyList, IEnumerable<IScheduleMatrixPro> matrixList, TimeZoneInfo timeZone)
		{
			var startTimeLimitation = new StartTimeLimitation();
			foreach (var matrix in matrixList)
			{
				foreach (var dateOnly in dateOnlyList)
				{
					var schedule = matrix.GetScheduleDayByKey(dateOnly);
					if (schedule == null)
						continue;

					var period = schedule.DaySchedulePart().ProjectionService().CreateProjection().Period();
					if (period == null) continue;
					if (startTimeLimitation.StartTime == null && startTimeLimitation.EndTime == null)
					{
						var timePeriod = period.Value.TimePeriod(timeZone);
						startTimeLimitation = new StartTimeLimitation(timePeriod.StartTime, timePeriod.StartTime);
					}
					else
					{
						var timePeriod = period.Value.TimePeriod(timeZone);
						if (startTimeLimitation.StartTime != timePeriod.StartTime || startTimeLimitation.EndTime != timePeriod.StartTime)
							return null;
					}
				}
			}
			var restriction = new EffectiveRestriction(startTimeLimitation,
			                                           new EndTimeLimitation(),
			                                           new WorkTimeLimitation(), null, null, null,
			                                           new List<IActivityRestriction>());
			return restriction;
		}

		private static IEffectiveRestriction extractSameEndTime(IList<DateOnly> dateOnlyList, IEnumerable<IScheduleMatrixPro> matrixList, TimeZoneInfo timeZone)
		{
			var endTimeLimitation = new EndTimeLimitation();
			foreach (var matrix in matrixList)
			{
				foreach (var dateOnly in dateOnlyList)
				{
					var schedule = matrix.GetScheduleDayByKey(dateOnly);
					if (schedule == null)
						continue;

					var period = schedule.DaySchedulePart().ProjectionService().CreateProjection().Period();
					if (period == null) continue;
					if (endTimeLimitation.StartTime == null && endTimeLimitation.EndTime == null)
					{
						var timePeriod = period.Value.TimePeriod(timeZone);
						endTimeLimitation = new EndTimeLimitation(timePeriod.EndTime, timePeriod.EndTime);
					}
					else
					{
						var timePeriod = period.Value.TimePeriod(timeZone);
						if (endTimeLimitation.StartTime != timePeriod.EndTime || endTimeLimitation.EndTime != timePeriod.EndTime)
							return null;
					}
				}
			}
			var restriction = new EffectiveRestriction(new StartTimeLimitation(),
													   endTimeLimitation,
			                                           new WorkTimeLimitation(), null, null, null,
			                                           new List<IActivityRestriction>());
			return restriction;
		}
	}
}
