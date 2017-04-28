using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Hangfire
{
	public class HangfireEventJob
	{
		private const string delimiter = ":::";

		public string DisplayName;
		public string Tenant;
		public string QueueName;
		public int Attempts;
		public int AllowFailures;
		public IEvent Event;
		public IEnumerable<IEvent> Package;
		public string HandlerTypeName;
		
		public string EventTypeName()
		{
			var eventType = Event.GetType();
			return $"{eventType.FullName}, {eventType.Assembly.GetName().Name}";
		}

		public static string TenantHashForRecurringId(string id)
		{
			return id.Substring(0, id.IndexOf(delimiter, StringComparison.Ordinal));
		}

		public string RecurringId()
		{
			var hashedHandlerAndEvent = $"{HandlerTypeName}{delimiter}{Event.GetType().Name}".GenerateGuid().ToString("N");
			var hashedTenant = Tenant?.GenerateGuid().ToString("N") ?? "";
			return $"{hashedTenant}{delimiter}{hashedHandlerAndEvent}";
		}
	}
}