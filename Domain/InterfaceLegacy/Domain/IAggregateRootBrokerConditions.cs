namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IAggregateRootBrokerConditions : IAggregateRoot
	{
		bool SendChangeOverMessageBroker();
	}
}
