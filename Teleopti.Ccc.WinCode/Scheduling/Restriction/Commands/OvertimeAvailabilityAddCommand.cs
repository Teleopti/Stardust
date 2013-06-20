using System;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.Restriction.Commands
{
    public interface IOvertimeAvailabilityAddCommand : IExecutableCommand, ICanExecute
    {
       
    }

    public class OvertimeAvailabilityAddCommand : IOvertimeAvailabilityAddCommand
    {
        private readonly IScheduleDay _scheduleDay;
        private readonly TimeSpan? _startTime;
        private readonly TimeSpan? _endTime;
        private readonly IOvertimeAvailabilityCreator  _overtimeAvailabilityCreator ;

        public OvertimeAvailabilityAddCommand(IScheduleDay scheduleDay, TimeSpan? startTime, TimeSpan? endTime, IOvertimeAvailabilityCreator  overtimeAvailabilityCreator )
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
                var studentAvailabilityDay = _overtimeAvailabilityCreator.Create(_scheduleDay, _startTime, _endTime);
                if(studentAvailabilityDay != null)
                    _scheduleDay.Add(studentAvailabilityDay);
            }
        }

        public bool CanExecute()
        {
            foreach (var persistableScheduleData in _scheduleDay.PersistableScheduleDataCollection())
            {
                if (persistableScheduleData is IOvertimeAvailability) return false;
            }

            bool startTimeError;
            bool endTimeError;
            if (!_overtimeAvailabilityCreator.CanCreate(_startTime, _endTime, out startTimeError, out endTimeError)) return false;

            return true;
        }
    }
}