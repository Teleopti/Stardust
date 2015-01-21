namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public interface IHangfireEventClient
	{
		void Enqueue(string displayName, string eventType, string serializedEvent, string handlerType);
	}
}