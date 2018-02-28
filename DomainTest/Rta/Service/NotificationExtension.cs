using Newtonsoft.Json;
using Teleopti.Ccc.Domain.MessageBroker;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service
{
	public static class NotificationExtension
	{
		public static T DeserializeBindaryData<T>(this Message message)
		{
			return JsonConvert.DeserializeObject<T>(message.BinaryData);
		}
	}
}