using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBus.Denormalizer
{
	public interface IScheduleChangedNotification
	{
		void Notify(IScenario scenario, IPerson person, DateOnly date);
	}
}