using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling
{
	public class AgentPreferenceEditCommand : IAgentPreferenceCommand
	{
		private readonly IScheduleDay _scheduleDay;
		private readonly IAgentPreferenceDayCreator _agentPreferenceDayCreator;
	    private readonly IScheduleDictionary _scheduleDictionary;
		private readonly IScheduleDayChangeCallback _scheduleDayChangeCallback;
		private readonly IAgentPreferenceData _data;

		public AgentPreferenceEditCommand(IScheduleDay scheduleDay, IAgentPreferenceData data, IAgentPreferenceDayCreator agentPreferenceDayCreator, IScheduleDictionary scheduleDictionary, IScheduleDayChangeCallback scheduleDayChangeCallback)
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
					_scheduleDay.DeletePreferenceRestriction();
					_scheduleDay.Add(preferenceDay);

				    _scheduleDictionary.Modify(ScheduleModifier.Scheduler, _scheduleDay, NewBusinessRuleCollection.Minimum(), _scheduleDayChangeCallback,
				                                  new ScheduleTagSetter(KeepOriginalScheduleTag.Instance));
				}
			}
		}

		public bool CanExecute()
		{
			foreach (var persistableScheduleData in _scheduleDay.PersistableScheduleDataCollection())
			{
				if (persistableScheduleData is IPreferenceDay)
				{
					var result = _agentPreferenceDayCreator.CanCreate(_data);
					return result.Result;
				}
			}

			return false;
		}
	}
}
