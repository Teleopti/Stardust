using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public class OptimizationOverLimitByMovedDaysDecider
    {
        private readonly IScheduleMatrixOriginalStateContainer _originalStateContainer;
        private readonly int _moveMaxDaysOff;
        private readonly int _moveMaxWorkShift;
        private ILogWriter _logWriter;

        public OptimizationOverLimitByMovedDaysDecider(
            IScheduleMatrixOriginalStateContainer originalStateContainer,
            int moveMaxDaysOff,
            int moveMaxWorkShift)
        {
            _originalStateContainer = originalStateContainer;
            _moveMaxDaysOff = moveMaxDaysOff;
            _moveMaxWorkShift = moveMaxWorkShift;
            _logWriter = new LogWriter<IOptimizationOverLimitDecider>();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), 
        System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "workshift"), 
        System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Interfaces.Domain.ILogWriter.LogInfo(System.String)")]
        public bool OverLimit()
        {
            if (_moveMaxDaysOff > -1)
            {
                if (_originalStateContainer.CountChangedDayOffs() > _moveMaxDaysOff)
                {
                    string name = _originalStateContainer.ScheduleMatrix.Person.Name.ToString();
                    _logWriter.LogInfo("The maximum " + _moveMaxDaysOff + " day off have already been moved for " + name);
                    return true;
                }
            }
            if (_moveMaxWorkShift > -1)
            {
                int changedWorkShifts = _originalStateContainer.CountChangedWorkShifts();
                if (changedWorkShifts > _moveMaxWorkShift)
                {
                    string name = _originalStateContainer.ScheduleMatrix.Person.Name.ToString();
                    _logWriter.LogInfo("The maximum " + _moveMaxWorkShift + " workshift have already been moved for " + name);
                    return true;
                }
            }
            return false;
        }
    }
}
