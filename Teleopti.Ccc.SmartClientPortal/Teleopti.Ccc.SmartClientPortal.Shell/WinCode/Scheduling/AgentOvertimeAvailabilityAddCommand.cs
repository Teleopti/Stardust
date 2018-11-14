using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.Restriction;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling
{
    public class AgentOvertimeAvailabilityAddCommand : IAgentOvertimeAvailabilityCommand
	{
		private readonly IScheduleDay _scheduleDay;
		private readonly TimeSpan? _startTime;
		private readonly TimeSpan? _endTime;
		private readonly IOvertimeAvailabilityCreator _overtimeAvailabilityDayCreator;
        private readonly IScheduleDictionary _scheduleDictionary;
	    private readonly IScheduleDayChangeCallback _scheduleDayChangeCallback;

	    public AgentOvertimeAvailabilityAddCommand(IScheduleDay scheduleDay, 
													TimeSpan? startTime, 
													TimeSpan? endTime, 
													IOvertimeAvailabilityCreator overtimeAvailabilityDayCreator, 
													IScheduleDictionary scheduleDictionary,
													IScheduleDayChangeCallback scheduleDayChangeCallback)
		{
			_scheduleDay = scheduleDay;
			_startTime = startTime;
			_endTime = endTime;
			_overtimeAvailabilityDayCreator = overtimeAvailabilityDayCreator;
		    _scheduleDictionary = scheduleDictionary;
		    _scheduleDayChangeCallback = scheduleDayChangeCallback;
		}

		public void Execute()
		{
			if (!CanExecute()) return;
			var overtimeAvailabilityDay = _overtimeAvailabilityDayCreator.Create(_scheduleDay, _startTime, _endTime);
			if (overtimeAvailabilityDay != null)
			{
			    _scheduleDay.Add(overtimeAvailabilityDay);

                _scheduleDictionary.Modify(ScheduleModifier.Scheduler, _scheduleDay, NewBusinessRuleCollection.Minimum(), 
											_scheduleDayChangeCallback,
                                              new ScheduleTagSetter(KeepOriginalScheduleTag.Instance));
			}
		}

		public bool CanExecute()
		{
			foreach (var persistableScheduleData in _scheduleDay.PersistableScheduleDataCollection())
			{
				if (persistableScheduleData is IOvertimeAvailability) return false;
			}

			if (!_overtimeAvailabilityDayCreator.CanCreate(_startTime, _endTime, out _, out _))
				return false;

			return true;
		}
	}
}
