using Newtonsoft.Json;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service
{
	public static class NotificationExtension
	{
		public static T DeserializeBindaryData<T>(this Interfaces.MessageBroker.Message message)
		{
			return JsonConvert.DeserializeObject<T>(message.BinaryData);
		}
	}
}