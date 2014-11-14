using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Interfaces.Messages;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public interface IServiceBusEventPopulatingPublisher : IEventPopulatingPublisher
	{
		bool EnsureBus();
		void Publish(ILogOnInfo message);
	}
}