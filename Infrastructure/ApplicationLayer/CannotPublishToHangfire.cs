namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class CannotPublishToHangfire : IHangfireEventClient
	{
		public void Enqueue(string displayName, string eventType, string serializedEvent)
		{
			throw new System.NotImplementedException();
		}
	}
}