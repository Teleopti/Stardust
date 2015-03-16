using Newtonsoft.Json;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service
{
	public static class NotificationExtension
	{
		public static T DeserializeBindaryData<T>(this Interfaces.MessageBroker.Notification notification)
		{
			return JsonConvert.DeserializeObject<T>(notification.BinaryData);
		}
	}
}