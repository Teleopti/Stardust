using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public interface IGroupMoveTimeOptimizer
    {
        IList<DateOnly> Execute();
        void LockDate(DateOnly dateOnly);
        IScheduleMatrixPro Matrix { get; }
        IPerson Person { get; }
        IOptimizationOverLimitByRestrictionDecider OptimizationOverLimitByRestrictionDecider { get; }
        bool IsMatrixForDateAndPerson(DateOnly dateOnly, IPerson person);
    }

    public class GroupMoveTimeOptimizer : IGroupMoveTimeOptimizer
    {
        private readonly IScheduleMatrixLockableBitArrayConverter _scheduleMatrixLockableBitArrayConverter;
        private readonly IMoveTimeDecisionMaker _moveTimeDecisionMaker;
        private readonly IScheduleResultDataExtractor _dataExtractor;
        private readonly IOptimizationOverLimitByRestrictionDecider _optimizationOverLimitByRestrictionDecider;
        private ILockableBitArray _lockableBitArray;

        public GroupMoveTimeOptimizer(IScheduleMatrixLockableBitArrayConverter scheduleMatrixLockableBitArrayConverter, 
                                        IMoveTimeDecisionMaker moveTimeDecisionMaker, 
                                        IScheduleResultDataExtractor dataExtractor,
                                        IOptimizationOverLimitByRestrictionDecider optimizationOverLimitByRestrictionDecider)

        {
            _scheduleMatrixLockableBitArrayConverter = scheduleMatrixLockableBitArrayConverter;
            _moveTimeDecisionMaker = moveTimeDecisionMaker;
            _dataExtractor = dataExtractor;
            _optimizationOverLimitByRestrictionDecider = optimizationOverLimitByRestrictionDecider;
        }

        public IList<DateOnly> Execute()
        {
            if (_lockableBitArray == null)
            {
                _lockableBitArray = _scheduleMatrixLockableBitArrayConverter.Convert(false, false);
            }

            var dates = _moveTimeDecisionMaker.Execute(_scheduleMatrixLockableBitArrayConverter, _dataExtractor);

            if (dates.Count != 2)
                return dates;
            //var result = _groupMoveTimeValidatorRunner.Run(Person,new List<DateOnly>{ dates[0]}, new List<DateOnly>{ dates[1]},true);
            //if (!result.Success)
            //{
            //    foreach(var date in dates )
            //    {
            //        LockDate(date);
            //    }
                
            //}
            
            //run executer 

            return dates;
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
                if (fullWeeksPeriodDay.Day == dateOnly)
                    _lockableBitArray.Lock(index, true);
                index++;
            }
        }

        public IScheduleMatrixPro Matrix
        {
            get
            {
                return _scheduleMatrixLockableBitArrayConverter.SourceMatrix;
            }
        }

        public IPerson Person
        {
            get { return _scheduleMatrixLockableBitArrayConverter.SourceMatrix.Person; }
        }

        public IOptimizationOverLimitByRestrictionDecider OptimizationOverLimitByRestrictionDecider
        {
            get { return _optimizationOverLimitByRestrictionDecider; }
        }

        public bool IsMatrixForDateAndPerson(DateOnly dateOnly, IPerson person)
        {
            var matrix = Matrix;
            if (matrix.Person == person && matrix.SchedulePeriod.DateOnlyPeriod.Contains(dateOnly))
                return true;
            return false;
        }
    }
}