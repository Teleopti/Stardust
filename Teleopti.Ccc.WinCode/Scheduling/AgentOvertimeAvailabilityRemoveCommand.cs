using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
	public interface IAgentOvertimeAvailabilityRemoveCommand : IExecutableCommand, ICanExecute
	{
	}

	public class AgentOvertimeAvailabilityRemoveCommand : IAgentOvertimeAvailabilityRemoveCommand
	{
		private readonly IScheduleDay _scheduleDay;

		public AgentOvertimeAvailabilityRemoveCommand(IScheduleDay scheduleDay)
		{
			_scheduleDay = scheduleDay;
		}

		public void Execute()
		{
			if (CanExecute())
			{
				_scheduleDay.DeleteOvertimeAvailability();
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
