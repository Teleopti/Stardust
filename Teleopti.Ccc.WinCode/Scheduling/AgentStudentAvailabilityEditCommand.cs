using System;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
	public class AgentStudentAvailabilityEditCommand : IAgentStudentAvailabilityCommand
	{
		private readonly IScheduleDay _scheduleDay;
		private readonly TimeSpan? _startTime;
		private readonly TimeSpan? _endTime;
		private readonly IAgentStudentAvailabilityDayCreator _studentAvailabilityDayCreator;
	    private readonly IScheduleDictionary _scheduleDictionary;

	    public AgentStudentAvailabilityEditCommand(IScheduleDay scheduleDay, TimeSpan? startTime, TimeSpan? endTime, IAgentStudentAvailabilityDayCreator studentAvailabilityDayCreator, IScheduleDictionary scheduleDictionary)
		{
			_scheduleDay = scheduleDay;
			_startTime = startTime;
			_endTime = endTime;
			_studentAvailabilityDayCreator = studentAvailabilityDayCreator;
		    _scheduleDictionary = scheduleDictionary;
		}

		public void Execute()
		{
			if (CanExecute())
			{
				var studentAvailabilityDay = _scheduleDay.PersistableScheduleDataCollection().OfType<IStudentAvailabilityDay>().First();

				studentAvailabilityDay.Change(new TimePeriod(_startTime.GetValueOrDefault(),_endTime.GetValueOrDefault()));

				_scheduleDictionary.Modify(ScheduleModifier.Scheduler, _scheduleDay, NewBusinessRuleCollection.Minimum(),
					new ResourceCalculationOnlyScheduleDayChangeCallback(),
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
