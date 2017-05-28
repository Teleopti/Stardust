using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

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
		public IEvent Event; // this means its a single event job to a single event handler method, but we'r keeping the name to not break compatibility
		public IEnumerable<IEvent> Package;
		public string HandlerTypeName;

		public static string TenantPrefixForTenant(string tenant)
		{
			return tenant.GenerateGuid().ToString("N");
		}

		public static string TenantPrefixForRecurringId(string id)
		{
			return id.Substring(0, id.IndexOf(delimiter, StringComparison.Ordinal));
		}

		public string RecurringId()
		{
			var id = $"{Tenant}{delimiter}{HandlerTypeName}{delimiter}{Event.GetType().Name}";

			var hashedHandlerAndEvent = $"{HandlerTypeName}{delimiter}{Event.GetType().Name}".GenerateGuid().ToString("N");
			var hashedTenant = Tenant?.GenerateGuid().ToString("N") ?? "";
			return $"{hashedTenant}{delimiter}{hashedHandlerAndEvent}";
		}
	}
}