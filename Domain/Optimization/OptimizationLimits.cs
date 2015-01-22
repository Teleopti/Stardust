using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IOptimizationLimits
	{
		bool HasOverLimitIncreased(OverLimitResults lastOverLimitCounts, IScheduleMatrixPro matrix);
		OverLimitResults OverLimitsCounts(IScheduleMatrixPro matrix);
		bool MoveMaxDaysOverLimit();
		bool ValidateMinWorkTimePerWeek(IScheduleMatrixPro scheduleMatrixPro);
	}

	public class OptimizationLimits : IOptimizationLimits
	{
		private readonly IOptimizationOverLimitByRestrictionDecider _overLimitByRestrictionDecider;
		private readonly INewBusinessRule _minWeekWorkTimeRule;

		public OptimizationLimits(IOptimizationOverLimitByRestrictionDecider overLimitByRestrictionDecider, INewBusinessRule minWeekWorkTimeRule)
		{
			_overLimitByRestrictionDecider = overLimitByRestrictionDecider;
			_minWeekWorkTimeRule = minWeekWorkTimeRule;
		}

		public OverLimitResults OverLimitsCounts(IScheduleMatrixPro matrix)
		{
			return _overLimitByRestrictionDecider.OverLimitsCounts(matrix);
		}

		public bool MoveMaxDaysOverLimit()
		{
			return _overLimitByRestrictionDecider.MoveMaxDaysOverLimit();
		}

		public bool HasOverLimitIncreased(OverLimitResults lastOverLimitCounts, IScheduleMatrixPro matrix)
		{
			return _overLimitByRestrictionDecider.HasOverLimitIncreased(lastOverLimitCounts, matrix);
		}

		public bool ValidateMinWorkTimePerWeek(IScheduleMatrixPro scheduleMatrixPro)
		{
			var minWorkTimeIsFulfilled = true;
			var dictionary = new Dictionary<IPerson, IScheduleRange> { { scheduleMatrixPro.Person, scheduleMatrixPro.ActiveScheduleRange } };

			foreach (var scheduleDayPro in scheduleMatrixPro.EffectivePeriodDays)
			{
				var scheduleDays = new List<IScheduleDay> { scheduleDayPro.DaySchedulePart() };
				minWorkTimeIsFulfilled = _minWeekWorkTimeRule.Validate(dictionary, scheduleDays).IsEmpty();
				if (!minWorkTimeIsFulfilled) break;
			}	
			
			return minWorkTimeIsFulfilled;
		}
	}
}
