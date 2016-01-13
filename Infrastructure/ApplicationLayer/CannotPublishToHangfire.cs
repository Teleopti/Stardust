namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class CannotPublishToHangfire : IHangfireEventClient
	{
		public void Enqueue(string displayName, string tenant, string eventType, string serializedEvent, string handlerType)
		{
			throw new System.NotImplementedException();
		}

		public void AddOrUpdateRecurring(string displayName, string id, string tenant, string eventType, string serializedEvent, string handlerType)
		{
			throw new System.NotImplementedException();
		}
	}
}