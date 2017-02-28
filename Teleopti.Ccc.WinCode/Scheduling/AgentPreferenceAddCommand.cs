using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
    public class AgentPreferenceAddCommand : IAgentPreferenceCommand
	{
		private readonly IScheduleDay _scheduleDay;
		private readonly IAgentPreferenceDayCreator _agentPreferenceDayCreator;
	    private readonly IScheduleDictionary _scheduleDictionary;
	    private readonly IScheduleDayChangeCallback _scheduleDayChangeCallback;
	    private readonly IAgentPreferenceData _data;


		public AgentPreferenceAddCommand(IScheduleDay scheduleDay, IAgentPreferenceData data, IAgentPreferenceDayCreator agentPreferenceDayCreator, IScheduleDictionary scheduleDictionary, IScheduleDayChangeCallback scheduleDayChangeCallback)
		{
			_scheduleDay = scheduleDay;
			_agentPreferenceDayCreator = agentPreferenceDayCreator;
		    _scheduleDictionary = scheduleDictionary;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
			_data = data;
		}

		public void Execute()
		{
			if (CanExecute())
			{
				var preferenceDay = _agentPreferenceDayCreator.Create(_scheduleDay, _data);
				if (preferenceDay != null)
				{
				    _scheduleDay.Add(preferenceDay);

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
				if (persistableScheduleData is IPreferenceDay) return false;
			}

			var result = _agentPreferenceDayCreator.CanCreate(_data);
			return result.Result;
		}
	}
}
