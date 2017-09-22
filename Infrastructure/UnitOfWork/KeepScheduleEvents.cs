using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class KeepScheduleEvents : IClearScheduleEvents
	{
		public void Execute(IDifferenceCollection<IPersistableScheduleData> scheduleDifference)
		{
		}
	}
}