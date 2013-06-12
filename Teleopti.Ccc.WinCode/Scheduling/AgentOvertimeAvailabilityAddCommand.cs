using System;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.WinCode.Scheduling.Restriction;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
	public interface IAgentOvertimeAvailabilityAddCommand : IExecutableCommand, ICanExecute
	{
	}

	public class AgentOvertimeAvailabilityAddCommand : IAgentOvertimeAvailabilityAddCommand
	{
		private readonly IScheduleDay _scheduleDay;
		private readonly TimeSpan? _startTime;
		private readonly TimeSpan? _endTime;
		private readonly IOvertimeAvailabilityCreator _overtimeAvailabilityDayCreator;

		public AgentOvertimeAvailabilityAddCommand(IScheduleDay scheduleDay, TimeSpan? startTime, TimeSpan? endTime, IOvertimeAvailabilityCreator overtimeAvailabilityDayCreator)
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
				var overtimeAvailabilityDay = _overtimeAvailabilityDayCreator.Create(_scheduleDay, _startTime, _endTime);
				if(overtimeAvailabilityDay != null)
					_scheduleDay.Add(overtimeAvailabilityDay);
			}
		}

		public bool CanExecute()
		{
			if (_scheduleDay.SignificantPart() == SchedulePartView.MainShift)
			{
				if (_scheduleDay.PersistableScheduleDataCollection().OfType<IOvertimeAvailability>().Count() == 2) return false;
			}
			else
			{
				foreach (var persistableScheduleData in _scheduleDay.PersistableScheduleDataCollection())
				{
					if (persistableScheduleData is IOvertimeAvailability) return false;
				}
			}
			
			bool startTimeError;
			bool endTimeError;
			if (!_overtimeAvailabilityDayCreator.CanCreate(_startTime, _endTime, out startTimeError, out endTimeError)) return false;

			return true;
		}
	}
}
