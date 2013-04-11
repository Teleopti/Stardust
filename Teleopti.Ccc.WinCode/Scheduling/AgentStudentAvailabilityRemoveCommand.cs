using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
	public interface IAgentStudentAvailabilityRemoveCommand : IExecutableCommand, ICanExecute
	{
		//	
	}

	public class AgentStudentAvailabilityRemoveCommand : IAgentStudentAvailabilityRemoveCommand
	{
		private readonly IScheduleDay _scheduleDay;

		public AgentStudentAvailabilityRemoveCommand(IScheduleDay scheduleDay)
		{
			_scheduleDay = scheduleDay;
		}

		public void Execute()
		{
			if (CanExecute())
			{
				_scheduleDay.DeleteStudentAvailabilityRestriction();
			}
		}

		public bool CanExecute()
		{
			foreach (var persistableScheduleData in _scheduleDay.PersistableScheduleDataCollection())
			{
				if (persistableScheduleData is IStudentAvailabilityDay) return true;
			}

			return false;
		}
	}
}
