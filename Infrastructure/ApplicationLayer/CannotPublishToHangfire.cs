namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class CannotPublishToHangfire : IHangfireEventClient
	{
		public void Enqueue(string displayName, string eventType, string serializedEvent, string handlerType)
		{
			throw new System.NotImplementedException();
		}
	}
}