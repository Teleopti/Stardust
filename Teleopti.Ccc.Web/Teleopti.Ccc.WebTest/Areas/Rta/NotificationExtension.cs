using Newtonsoft.Json;
using Teleopti.Interfaces.MessageBroker;

namespace Teleopti.Ccc.WebTest.Areas.Rta
{
	public static class NotificationExtension
	{
		public static T DeserializeBindaryData<T>(this Notification notification)
		{
			return JsonConvert.DeserializeObject<T>(notification.BinaryData);
		}
	}
}