using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IOptimizationLimits
	{
		bool HasOverLimitExceeded(OverLimitResults lastOverLimitCounts, IScheduleMatrixPro matrix);
		OverLimitResults OverLimitsCounts(IScheduleMatrixPro matrix);
		bool MoveMaxDaysOverLimit();
		bool ValidateMinWorkTimePerWeek(IScheduleMatrixPro scheduleMatrixPro);
	}

	public class OptimizationLimits : IOptimizationLimits
	{
		private readonly IOptimizationOverLimitByRestrictionDecider _overLimitByRestrictionDecider;

		public OptimizationLimits(IOptimizationOverLimitByRestrictionDecider overLimitByRestrictionDecider)
		{
			_overLimitByRestrictionDecider = overLimitByRestrictionDecider;
		}

		public OverLimitResults OverLimitsCounts(IScheduleMatrixPro matrix)
		{
			return _overLimitByRestrictionDecider.OverLimitsCounts(matrix);
		}

		public bool MoveMaxDaysOverLimit()
		{
			return _overLimitByRestrictionDecider.MoveMaxDaysOverLimit();
		}

		public bool HasOverLimitExceeded(OverLimitResults lastOverLimitCounts, IScheduleMatrixPro matrix)
		{
			return _overLimitByRestrictionDecider.HasOverLimitIncreased(lastOverLimitCounts, matrix);
		}

		public bool ValidateMinWorkTimePerWeek(IScheduleMatrixPro scheduleMatrixPro)
		{
			var minWorkTimeIsFulfilled = true;
			var dictionary = new Dictionary<IPerson, IScheduleRange> { { scheduleMatrixPro.Person, scheduleMatrixPro.ActiveScheduleRange } };
			var minWeekWorkTime = new MinWeekWorkTimeRule(new WeeksFromScheduleDaysExtractor());
			foreach (var scheduleDayPro in scheduleMatrixPro.EffectivePeriodDays)
			{
				var scheduleDays = new List<IScheduleDay> { scheduleDayPro.DaySchedulePart() };
				minWorkTimeIsFulfilled = minWeekWorkTime.Validate(dictionary, scheduleDays).IsEmpty();
				if (!minWorkTimeIsFulfilled) break;
			}	
			
			return minWorkTimeIsFulfilled;
		}
	}
}
