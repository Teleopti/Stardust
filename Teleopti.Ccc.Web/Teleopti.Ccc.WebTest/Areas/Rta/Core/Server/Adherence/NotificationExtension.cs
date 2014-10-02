﻿using Newtonsoft.Json;
using Teleopti.Interfaces.MessageBroker;

namespace Teleopti.Ccc.WebTest.Areas.Rta.Core.Server.Adherence
{
	public static class NotificationExtension
	{
		public static T GetOriginal<T>(this Notification notification)
		{
			return JsonConvert.DeserializeObject<T>(notification.BinaryData);
		}
	}
}