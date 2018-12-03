using System;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling
{
	public class AgentStudentAvailabilityEditCommand : IAgentStudentAvailabilityCommand
	{
		private readonly IScheduleDay _scheduleDay;
		private readonly TimeSpan? _startTime;
		private readonly TimeSpan? _endTime;
		private readonly IAgentStudentAvailabilityDayCreator _studentAvailabilityDayCreator;
	    private readonly IScheduleDictionary _scheduleDictionary;
		private readonly IScheduleDayChangeCallback _scheduleDayChangeCallback;

		public AgentStudentAvailabilityEditCommand(IScheduleDay scheduleDay, 
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
				var studentAvailabilityDay = _scheduleDay.PersistableScheduleDataCollection().OfType<IStudentAvailabilityDay>().First();

				studentAvailabilityDay.Change(new TimePeriod(_startTime.GetValueOrDefault(),_endTime.GetValueOrDefault()));

				_scheduleDictionary.Modify(ScheduleModifier.Scheduler, _scheduleDay, NewBusinessRuleCollection.Minimum(),
					_scheduleDayChangeCallback,
					new ScheduleTagSetter(KeepOriginalScheduleTag.Instance));
			}
		}

		public bool CanExecute()
		{
			if (_scheduleDay.PersistableScheduleDataCollection().OfType<IStudentAvailabilityDay>().Any())
			{
				bool startTimeError;
				bool endTimeError;
				if (!_studentAvailabilityDayCreator.CanCreate(_startTime, _endTime, out startTimeError, out endTimeError))
					return false;

				return true;
			}

			return false;
		}
	}
}
