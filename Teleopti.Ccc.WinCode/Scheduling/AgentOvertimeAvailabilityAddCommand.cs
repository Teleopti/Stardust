using System;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.WinCode.Scheduling.Restriction;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
	public interface IAgentOvertimeAvailabilityAddCommand : IExecutableCommand, ICanExecute
	{
		void Initialize();
	}

	public class AgentOvertimeAvailabilityAddCommand : IAgentOvertimeAvailabilityAddCommand
	{
		private readonly IScheduleDay _scheduleDay;
		private readonly TimeSpan? _startTime;
		private readonly TimeSpan? _endTime;
		private readonly IOvertimeAvailabilityCreator _overtimeAvailabilityDayCreator;
		private TimePeriod? _existingShiftTimePeriod;

		public AgentOvertimeAvailabilityAddCommand(IScheduleDay scheduleDay, TimeSpan? startTime, TimeSpan? endTime, IOvertimeAvailabilityCreator overtimeAvailabilityDayCreator)
		{
			_scheduleDay = scheduleDay;
			_startTime = startTime;
			_endTime = endTime;
			_overtimeAvailabilityDayCreator = overtimeAvailabilityDayCreator;
		}

		public void Initialize()
		{
			var shiftTimePeriod = _scheduleDay.ProjectionService().CreateProjection().Period();
			if (shiftTimePeriod != null)
				_existingShiftTimePeriod = shiftTimePeriod.Value.TimePeriod(TeleoptiPrincipal.Current.Regional.TimeZone);
		}

		public void Execute()
		{
			if (!CanExecute()) return;

			if (_existingShiftTimePeriod != null)
			{
				var overtimeAvailabilities = _overtimeAvailabilityDayCreator.Create(_scheduleDay, _startTime, _endTime, _existingShiftTimePeriod.Value.StartTime, _existingShiftTimePeriod.Value.EndTime);
				if (overtimeAvailabilities != null)
					overtimeAvailabilities.ForEach(_scheduleDay.Add);
			}
			else
			{
				var overtimeAvailabilityDay = _overtimeAvailabilityDayCreator.Create(_scheduleDay, _startTime, _endTime);
				if (overtimeAvailabilityDay != null)
					_scheduleDay.Add(overtimeAvailabilityDay);
			}
		}

		public bool CanExecute()
		{
			bool startTimeError;
			bool endTimeError;

			if (_existingShiftTimePeriod != null)
			{
				if (!_overtimeAvailabilityDayCreator.CanCreate(_startTime, _endTime, _existingShiftTimePeriod.Value.StartTime,
					                                           _existingShiftTimePeriod.Value.EndTime, out startTimeError,
					                                           out endTimeError)) return false;
			}
			else
			{
				foreach (var persistableScheduleData in _scheduleDay.PersistableScheduleDataCollection())
				{
					if (persistableScheduleData is IOvertimeAvailability) return false;
				}
				
				if (!_overtimeAvailabilityDayCreator.CanCreate(_startTime, _endTime, out startTimeError, out endTimeError)) return false;
			}
			
			return true;
		}
	}
}
