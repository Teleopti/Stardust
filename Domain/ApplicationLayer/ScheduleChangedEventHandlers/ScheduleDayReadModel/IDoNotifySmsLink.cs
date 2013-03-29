using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBus.Denormalizer
{
	public interface IDoNotifySmsLink
	{
		void NotifySmsLink(ScheduleDayReadModel readModel, DateOnly date, IPerson person);
	}
}