using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class KeepScheduleEvents : IClearScheduleEvents
	{
		public void Execute(IDifferenceCollection<IPersistableScheduleData> scheduleDifference)
		{
		}
	}
}