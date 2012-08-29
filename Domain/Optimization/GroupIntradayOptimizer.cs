using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IGroupIntradayOptimizer
	{
		DateOnly? Execute();
		void LockDate(DateOnly dateOnly);
		IPerson Person { get; }
		bool IsMatrixForDateAndPerson(DateOnly dateOnly, IPerson person);
		IScheduleMatrixPro Matrix { get; }
		IOptimizationOverLimitByRestrictionDecider OptimizationOverLimitByRestrictionDecider { get; }
	}

	public class GroupIntradayOptimizer : IGroupIntradayOptimizer
	{
		private readonly IScheduleMatrixLockableBitArrayConverter _scheduleMatrixLockableBitArrayConverter;
		private readonly IIntradayDecisionMaker _intradayDecisionMaker;
		private readonly IScheduleResultDataExtractor _dayIntradayDeviationDataExtractor;
		private readonly IOptimizationOverLimitByRestrictionDecider _optimizationOverLimitByRestrictionDecider;
		private ILockableBitArray _lockableBitArray;

		public GroupIntradayOptimizer(IScheduleMatrixLockableBitArrayConverter scheduleMatrixLockableBitArrayConverter, 
			IIntradayDecisionMaker intradayDecisionMaker,
			IScheduleResultDataExtractor dayIntradayDeviationDataExtractor,
			IOptimizationOverLimitByRestrictionDecider optimizationOverLimitByRestrictionDecider)
		{
			_scheduleMatrixLockableBitArrayConverter = scheduleMatrixLockableBitArrayConverter;
			_intradayDecisionMaker = intradayDecisionMaker;
			_dayIntradayDeviationDataExtractor = dayIntradayDeviationDataExtractor;
			_optimizationOverLimitByRestrictionDecider = optimizationOverLimitByRestrictionDecider;
		}

		public DateOnly? Execute()
		{
			if(_lockableBitArray == null)
			{
				_lockableBitArray = _scheduleMatrixLockableBitArrayConverter.Convert(false, false);
			}

			DateOnly? date = _intradayDecisionMaker.Execute(_lockableBitArray, _dayIntradayDeviationDataExtractor,
												 _scheduleMatrixLockableBitArrayConverter.SourceMatrix);
			return date;
		}

		public void LockDate(DateOnly dateOnly)
		{
			if (_lockableBitArray == null)
			{
				_lockableBitArray = _scheduleMatrixLockableBitArrayConverter.Convert(false, false);
			}

			int index = 0;
			foreach (var fullWeeksPeriodDay in _scheduleMatrixLockableBitArrayConverter.SourceMatrix.FullWeeksPeriodDays)
			{
				if(fullWeeksPeriodDay.Day == dateOnly)
					_lockableBitArray.Lock(index, true);
				index++;
			}
		}

		public IPerson Person
		{
			get { return _scheduleMatrixLockableBitArrayConverter.SourceMatrix.Person; }
		}

		public IScheduleMatrixPro Matrix
		{
			get
			{
				return _scheduleMatrixLockableBitArrayConverter.SourceMatrix;
			}
		}

		public IOptimizationOverLimitByRestrictionDecider OptimizationOverLimitByRestrictionDecider
		{
			get { return _optimizationOverLimitByRestrictionDecider; }
		}

		public bool IsMatrixForDateAndPerson(DateOnly dateOnly, IPerson person)
		{
			IScheduleMatrixPro matrix = Matrix;
			if (matrix.Person == person && matrix.SchedulePeriod.DateOnlyPeriod.Contains(dateOnly))
				return true;

			return false;
		}
	}
}