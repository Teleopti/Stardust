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
		private readonly IScheduleResultDataExtractor _dayIntraDayDeviationDataExtractor;
		private readonly IOptimizationOverLimitByRestrictionDecider _optimizationOverLimitByRestrictionDecider;
		private ILockableBitArray _lockableBitArray;
		private readonly IScheduleMatrixPro _matrix;

		public GroupIntradayOptimizer(IScheduleMatrixLockableBitArrayConverter scheduleMatrixLockableBitArrayConverter, 
			IIntradayDecisionMaker intradayDecisionMaker,
			IScheduleResultDataExtractor dayIntraDayDeviationDataExtractor,
			IOptimizationOverLimitByRestrictionDecider optimizationOverLimitByRestrictionDecider)
		{
			_scheduleMatrixLockableBitArrayConverter = scheduleMatrixLockableBitArrayConverter;
			_intradayDecisionMaker = intradayDecisionMaker;
			_dayIntraDayDeviationDataExtractor = dayIntraDayDeviationDataExtractor;
			_optimizationOverLimitByRestrictionDecider = optimizationOverLimitByRestrictionDecider;
			_matrix = _scheduleMatrixLockableBitArrayConverter.SourceMatrix;
		}

		public DateOnly? Execute()
		{
			if(_lockableBitArray == null)
				_lockableBitArray = _scheduleMatrixLockableBitArrayConverter.Convert(false, false);
			DateOnly? date = _intradayDecisionMaker.Execute(_lockableBitArray, _dayIntraDayDeviationDataExtractor,
				                                 _matrix);
			return date;
		}

		public void LockDate(DateOnly dateOnly)
		{
			if (_lockableBitArray == null)
				_lockableBitArray = _scheduleMatrixLockableBitArrayConverter.Convert(false, false);

			int index = 0;
			foreach (var fullWeeksPeriodDay in _matrix.FullWeeksPeriodDays)
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
			get { return _scheduleMatrixLockableBitArrayConverter.SourceMatrix; }
		}

		public IOptimizationOverLimitByRestrictionDecider OptimizationOverLimitByRestrictionDecider
		{
			get { return _optimizationOverLimitByRestrictionDecider; }
		}

		public bool IsMatrixForDateAndPerson(DateOnly dateOnly, IPerson person)
		{
			if (_matrix.Person == person && _matrix.SchedulePeriod.DateOnlyPeriod.Contains(dateOnly))
				return true;

			return false;
		}
	}
}