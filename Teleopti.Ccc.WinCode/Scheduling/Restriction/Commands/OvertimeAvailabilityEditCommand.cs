using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.Restriction.Commands
{
    public interface IOvertimeAvailabilityEditCommand : IExecutableCommand, ICanExecute
    {

    }

    public class OvertimeAvailabilityEditCommand : IOvertimeAvailabilityEditCommand
    {
        private readonly IScheduleDay _scheduleDay;
        private readonly TimeSpan? _startTime;
        private readonly TimeSpan? _endTime;
        private readonly IOvertimeAvailabilityCreator _overtimeAvailabilityCreator;

        public OvertimeAvailabilityEditCommand(IScheduleDay scheduleDay, TimeSpan? startTime, TimeSpan? endTime, IOvertimeAvailabilityCreator  overtimeAvailabilityCreator )
        {
            _scheduleDay = scheduleDay;
            _startTime = startTime;
            _endTime = endTime;
            _overtimeAvailabilityCreator = overtimeAvailabilityCreator;
        }

        public void Execute()
        {
            if (CanExecute())
            {
                _scheduleDay.DeleteStudentAvailabilityRestriction();
                var overtimeAvailability = _overtimeAvailabilityCreator.Create(_scheduleDay, _startTime, _endTime);
                if (overtimeAvailability != null)
                    _scheduleDay.Add(overtimeAvailability);
            }		
        }

        public bool CanExecute()
        {
            foreach (var persistableScheduleData in _scheduleDay.PersistableScheduleDataCollection())
            {
                if (persistableScheduleData is IOvertimeAvailability )
                {
                    bool startTimeError;
                    bool endTimeError;
                    if (!_overtimeAvailabilityCreator.CanCreate(_startTime, _endTime, out startTimeError, out endTimeError)) return false;

                    return true;
                }
            }

            return false;
        }
    }
}