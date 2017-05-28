using System;
using System.Collections.Generic;
using System.Linq;
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
			return tenant;
		}

		public static string TenantPrefixForRecurringId(string id)
		{
			return id.Substring(0, id.IndexOf(delimiter, StringComparison.Ordinal));
		}

		public string RecurringId()
		{
			var hash = $"{Tenant}{delimiter}{HandlerTypeName}{delimiter}{Event.GetType().Name}".GetHashCode();
			var displayFriendlyId = $"{Tenant}{delimiter}{Type.GetType(HandlerTypeName).Name}{delimiter}{Event.GetType().Name}{delimiter}{hash}";

			var maxLength = 100 - "recurring-job:".Length;
			if (displayFriendlyId.Length > maxLength)
				displayFriendlyId = displayFriendlyId.Substring(displayFriendlyId.Length - maxLength, maxLength);

			return displayFriendlyId;
		}
	}
}