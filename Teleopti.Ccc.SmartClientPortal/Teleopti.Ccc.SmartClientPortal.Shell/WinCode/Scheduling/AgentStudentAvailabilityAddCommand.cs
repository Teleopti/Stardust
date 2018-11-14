using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling
{
    public class AgentStudentAvailabilityAddCommand : IAgentStudentAvailabilityCommand
	{
		private readonly IScheduleDay _scheduleDay;
		private readonly TimeSpan? _startTime;
		private readonly TimeSpan? _endTime;
		private readonly IAgentStudentAvailabilityDayCreator _studentAvailabilityDayCreator;
        private readonly IScheduleDictionary _scheduleDictionary;
	    private readonly IScheduleDayChangeCallback _scheduleDayChangeCallback;

	    public AgentStudentAvailabilityAddCommand(IScheduleDay scheduleDay, 
												TimeSpan? startTime, 
												TimeSpan? endTime, 
												IAgentStudentAvailabilityDayCreator studentAvailabilityDayCreator, 
												IScheduleDictionary scheduleDictionary,
												IScheduleDayChangeCallback scheduleDayChangeCallback)
		{
			_scheduleDay = scheduleDay;
			_startTime = startTime;
			_endTime = endTime;
			_studentAvailabilityDayCreator = studentAvailabilityDayCreator;
		    _scheduleDictionary = scheduleDictionary;
		    _scheduleDayChangeCallback = scheduleDayChangeCallback;
		}

		public void Execute()
		{
			if (CanExecute())
			{
				var studentAvailabilityDay = _studentAvailabilityDayCreator.Create(_scheduleDay, _startTime, _endTime);

				if (studentAvailabilityDay != null)
				{
				    _scheduleDay.Add(studentAvailabilityDay);

                    _scheduleDictionary.Modify(ScheduleModifier.Scheduler, _scheduleDay, NewBusinessRuleCollection.Minimum(), _scheduleDayChangeCallback,
                                                  new ScheduleTagSetter(KeepOriginalScheduleTag.Instance));
				}
			}
		}

		public bool CanExecute()
		{
			if (!_scheduleDay.FullAccess) return false;

			foreach (var persistableScheduleData in _scheduleDay.PersistableScheduleDataCollection())
			{
				if (persistableScheduleData is IStudentAvailabilityDay) return false;
			}

			if (!_studentAvailabilityDayCreator.CanCreate(_startTime, _endTime, out _, out _)) return false;

			return true;
		}
	}
}
