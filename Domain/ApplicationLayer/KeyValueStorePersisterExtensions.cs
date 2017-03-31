using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	public static class KeyValueStorePersisterExtensions
	{
		public static void Update(this IKeyValueStorePersister instance, string key, bool value)
		{
			instance.Update(key, value.ToString());
		}

		public static bool Get(this IKeyValueStorePersister instance, string key, bool @default)
		{
			var value = instance.Get(key);
			if (string.IsNullOrEmpty(value))
				return @default;
			bool result;
			return bool.TryParse(value, out result) ? result : @default;
		}

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