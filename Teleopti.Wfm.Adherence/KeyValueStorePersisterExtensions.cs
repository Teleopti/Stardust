using System;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Service;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	public static class KeyValueStorePersisterExtensions
	{
		public static void Update(this IKeyValueStorePersister instance, string key, CurrentScheduleReadModelVersion value)
		{
			instance.Update(key, value.ToString());
		}

		public static CurrentScheduleReadModelVersion Get(this IKeyValueStorePersister instance, string key, Func<CurrentScheduleReadModelVersion> @default)
		{
			var value = instance.Get(key);
			if (string.IsNullOrEmpty(value))
				return @default?.Invoke();
			return CurrentScheduleReadModelVersion.Parse(value) ?? @default?.Invoke();
		}
	}
}