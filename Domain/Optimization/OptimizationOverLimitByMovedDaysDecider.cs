using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public class OptimizationOverLimitByMovedDaysDecider : IOptimizationOverLimitDecider
    {
        private readonly IScheduleMatrixOriginalStateContainer _originalStateContainer;
        private readonly int _moveMaxDaysOff;
        private readonly int _moveMaxWorkShift;

        public OptimizationOverLimitByMovedDaysDecider(
            IScheduleMatrixOriginalStateContainer originalStateContainer,
            int moveMaxDaysOff,
            int moveMaxWorkShift)
        {
            _originalStateContainer = originalStateContainer;
            _moveMaxDaysOff = moveMaxDaysOff;
            _moveMaxWorkShift = moveMaxWorkShift;
        }

        public bool OverLimit(ILogWriter logWriter)
        {
            if (_moveMaxDaysOff > -1)
            {
                if (_originalStateContainer.CountChangedDayOffs() > _moveMaxDaysOff)
                {
                    string name = _originalStateContainer.ScheduleMatrix.Person.Name.ToString();
                    logWriter.LogInfo("The maximum " + _moveMaxDaysOff + " day off have already been moved for " + name);
                    return true;
                }
            }
            if (_moveMaxWorkShift > -1)
            {
                int changedWorkShifts = _originalStateContainer.CountChangedWorkShifts();
                if (changedWorkShifts > _moveMaxWorkShift)
                {
                    string name = _originalStateContainer.ScheduleMatrix.Person.Name.ToString();
                    logWriter.LogInfo("The maximum " + _moveMaxWorkShift + " workshift have already been moved for " + name);
                    return true;
                }
            }
            return false;
        }
    }
}
