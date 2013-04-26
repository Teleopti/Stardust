using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
	public interface IAgentPreferenceRemoveCommand : IExecutableCommand, ICanExecute
	{
		//
	}

	public class AgentPreferenceRemoveCommand : IAgentPreferenceRemoveCommand
	{
		private readonly IScheduleDay _scheduleDay;
 
		public AgentPreferenceRemoveCommand(IScheduleDay scheduleDay)
		{
			_scheduleDay = scheduleDay;

		}
		public void Execute()
		{
			if (CanExecute())
			{
				_scheduleDay.DeletePreferenceRestriction();
			}
		}

		public bool CanExecute()
		{
			foreach (var persistableScheduleData in _scheduleDay.PersistableScheduleDataCollection())
			{
				if (persistableScheduleData is IPreferenceDay) return true;
			}

			return false;
		}
	}
}
