﻿using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
	public interface IAgentStudentAvailabilityAddCommand : IExecutableCommand, ICanExecute
	{
		//	
	}

	public class AgentStudentAvailabilityAddCommand : IAgentStudentAvailabilityAddCommand
	{
		private readonly IScheduleDay _scheduleDay;
		private readonly TimeSpan? _startTime;
		private readonly TimeSpan? _endTime;
		private readonly bool _endNextDay;
		private readonly IAgentStudentAvailabilityDayCreator _studentAvailabilityDayCreator;

		public AgentStudentAvailabilityAddCommand(IScheduleDay scheduleDay, TimeSpan? startTime, TimeSpan? endTime, bool endNextDay, IAgentStudentAvailabilityDayCreator studentAvailabilityDayCreator)
		{
			_scheduleDay = scheduleDay;
			_startTime = startTime;
			_endTime = endTime;
			_endNextDay = endNextDay;
			_studentAvailabilityDayCreator = studentAvailabilityDayCreator;
		}

		public void Execute()
		{
			if (CanExecute())
			{
				var studentAvailabilityDay = _studentAvailabilityDayCreator.Create(_scheduleDay, _startTime, _endTime, _endNextDay);
				if(studentAvailabilityDay != null)
					_scheduleDay.Add(studentAvailabilityDay);
			}
		}

		public bool CanExecute()
		{
			foreach (var persistableScheduleData in _scheduleDay.PersistableScheduleDataCollection())
			{
				if (persistableScheduleData is IStudentAvailabilityDay) return false;
			}

			if (!_studentAvailabilityDayCreator.CanCreate(_startTime, _endTime, _endNextDay)) return false;

			return true;
		}
	}
}
