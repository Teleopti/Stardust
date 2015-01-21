namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public interface IHangfireEventProcessor
	{
		void Process(string displayName, string eventType, string serializedEvent, string handlerType);
	}
}