namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public interface IHangfireEventClient
	{
		void Enqueue(string displayName, string tenant, string eventType, string serializedEvent, string handlerType);
		void AddOrUpdate(string displayName, string id, string tenant, string eventType, string serializedEvent, string handlerType);
	}
}