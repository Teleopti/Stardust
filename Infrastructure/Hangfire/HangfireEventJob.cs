using System;
using System.Collections.Generic;
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
		public int RunInterval;
		
		public static string TenantPrefixForTenant(string tenant)
		{
			return tenant?.GetHashCode().ToString();
		}

		public static string TenantPrefixForRecurringId(string id)
		{
			return id.Substring(0, id.IndexOf(delimiter, StringComparison.Ordinal));
		}

		private int recurringId()
		{
			unchecked
			{
				var hash = Tenant?.GetHashCode() ?? "".GetHashCode();
				hash = (hash * 397) ^ HandlerTypeName.GetHashCode();
				hash = (hash * 397) ^ Event.GetType().Name.GetHashCode();
				return hash;
			}
		}

		public string RecurringId()
		{
			var tenantPrefix = TenantPrefixForTenant(Tenant);
			var tenant = Tenant ?? "";
			if (tenant.Length > 15)
				tenant = tenant.Substring(0, 15);
			var handler = HandlerTypeName ?? "";
			if (handler.Length > 15)
				handler = handler.Substring(0, 15);
			var @event = Event.GetType().Name;
			if (@event.Length > 15)
				@event = @event.Substring(0, 15);
			var id = recurringId();
			return $"{tenantPrefix}{delimiter}{tenant}:{handler}:{@event}:{id}";
		}
	}
}