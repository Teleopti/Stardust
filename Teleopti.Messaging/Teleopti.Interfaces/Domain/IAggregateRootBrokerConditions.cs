namespace Teleopti.Interfaces.Domain
{
	public interface IAggregateRootBrokerConditions : IAggregateRoot
	{
		bool SendChangeOverMessageBroker();
	}
}
