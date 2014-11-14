using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Interfaces.Messages;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public interface IServiceBusEventPublisher : IEventPublisher
	{
		bool EnsureBus();
		void Publish(ILogOnInfo nonEvent);
	}
}