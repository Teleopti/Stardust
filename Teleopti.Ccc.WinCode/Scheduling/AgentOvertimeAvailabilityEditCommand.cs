using System;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.WinCode.Scheduling.Restriction;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
	public interface IAgentOvertimeAvailabilityEditCommand : IExecutableCommand, ICanExecute
	{
	}

	public class AgentOvertimeAvailabilityEditCommand : IAgentOvertimeAvailabilityEditCommand
	{
		private readonly IScheduleDay _scheduleDay;
		private readonly TimeSpan? _startTime;
		private readonly TimeSpan? _endTime;
		private readonly IOvertimeAvailabilityCreator _overtimeAvailabilityDayCreator;

		public AgentOvertimeAvailabilityEditCommand(IScheduleDay scheduleDay, TimeSpan? startTime, TimeSpan? endTime, IOvertimeAvailabilityCreator overtimeAvailabilityDayCreator)
		{
			_scheduleDay = scheduleDay;
			_startTime = startTime;
			_endTime = endTime;
			_overtimeAvailabilityDayCreator = overtimeAvailabilityDayCreator;
		}

		public void Execute()
		{
			if (CanExecute())
			{
				_scheduleDay.DeleteOvertimeAvailability();
				var overtimeAvailabilityDay = _overtimeAvailabilityDayCreator.Create(_scheduleDay, _startTime, _endTime);
				if (overtimeAvailabilityDay != null)
					_scheduleDay.Add(overtimeAvailabilityDay);
			}		
		}

		public bool CanExecute()
		{
			foreach (var persistableScheduleData in _scheduleDay.PersistableScheduleDataCollection())
			{
				if (persistableScheduleData is IOvertimeAvailability)
				{
					bool startTimeError;
					bool endTimeError;
					if (!_overtimeAvailabilityDayCreator.CanCreate(_startTime, _endTime, out startTimeError, out endTimeError)) return false;

					return true;
				}
			}

			return false;
		}
	}
}
