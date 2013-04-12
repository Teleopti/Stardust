using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
	public interface IAgentPreferenceEditCommand : IExecutableCommand, ICanExecute
	{
		//	
	}
	public class AgentPreferenceEditCommand : IAgentPreferenceEditCommand
	{
		private readonly IScheduleDay _scheduleDay;
		private readonly IAgentPreferenceDayCreator _agentPreferenceDayCreator;
		private readonly IAgentPreferenceData _data;

		public AgentPreferenceEditCommand(IScheduleDay scheduleDay, IAgentPreferenceData data, IAgentPreferenceDayCreator agentPreferenceDayCreator)
		{
			_scheduleDay = scheduleDay;
			_agentPreferenceDayCreator = agentPreferenceDayCreator;
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
