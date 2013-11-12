using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
	public class AgentOvertimeAvailabilityRemoveCommand : IAgentOvertimeAvailabilityCommand
	{
		private readonly IScheduleDay _scheduleDay;
	    private readonly IScheduleDictionary _scheduleDictionary;

	    public AgentOvertimeAvailabilityRemoveCommand(IScheduleDay scheduleDay, IScheduleDictionary scheduleDictionary)
		{
		    _scheduleDay = scheduleDay;
		    _scheduleDictionary = scheduleDictionary;
		}

	    public void Execute()
		{
			if (CanExecute())
			{
				_scheduleDay.DeleteOvertimeAvailability();

                _scheduleDictionary.Modify(ScheduleModifier.Scheduler, _scheduleDay, NewBusinessRuleCollection.Minimum(), new ResourceCalculationOnlyScheduleDayChangeCallback(),
                                              new ScheduleTagSetter(KeepOriginalScheduleTag.Instance));
			}
		}

		public bool CanExecute()
		{
			foreach (var persistableScheduleData in _scheduleDay.PersistableScheduleDataCollection())
			{
				if (persistableScheduleData is IOvertimeAvailability) return true;
			}

			return false;
		}
	}
}
