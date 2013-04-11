using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
	public interface IAgentStudentAvailabilityEditCommand : IExecutableCommand, ICanExecute
	{
		//	
	}

	public class AgentStudentAvailabilityEditCommand : IAgentStudentAvailabilityEditCommand
	{
		private readonly IScheduleDay _scheduleDay;
		private readonly TimeSpan? _startTime;
		private readonly TimeSpan? _endTime;
		private readonly IAgentStudentAvailabilityDayCreator _studentAvailabilityDayCreator;

		public AgentStudentAvailabilityEditCommand(IScheduleDay scheduleDay, TimeSpan? startTime, TimeSpan? endTime, IAgentStudentAvailabilityDayCreator studentAvailabilityDayCreator)
		{
			_scheduleDay = scheduleDay;
			_startTime = startTime;
			_endTime = endTime;
			_studentAvailabilityDayCreator = studentAvailabilityDayCreator;
		}

		public void Execute()
		{
			if (CanExecute())
			{
				_scheduleDay.DeleteStudentAvailabilityRestriction();
				var studentAvailabilityDay = _studentAvailabilityDayCreator.Create(_scheduleDay, _startTime, _endTime);
				if (studentAvailabilityDay != null)
					_scheduleDay.Add(studentAvailabilityDay);
			}		
		}

		public bool CanExecute()
		{
			foreach (var persistableScheduleData in _scheduleDay.PersistableScheduleDataCollection())
			{
				if (persistableScheduleData is IStudentAvailabilityDay)
				{
					bool startTimeError;
					bool endTimeError;
					if (!_studentAvailabilityDayCreator.CanCreate(_startTime, _endTime, out startTimeError, out endTimeError)) return false;

					return true;
				}
			}

			return false;
		}
	}
}
