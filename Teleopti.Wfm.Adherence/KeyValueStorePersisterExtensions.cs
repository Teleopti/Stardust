using System;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Wfm.Adherence.Domain.Service;
using Teleopti.Wfm.Adherence.States;

namespace Teleopti.Wfm.Adherence
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
		
		public static void Update(this IKeyValueStorePersister instance, string key, int value)
		{
			instance.Update(key, value.ToString());
		}
		
		public static void Update(this IKeyValueStorePersister instance, string key, long value)
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

		public static int Get(this IKeyValueStorePersister instance, string key, int @default)
		{
			var value = instance.Get(key);
			if (string.IsNullOrEmpty(value))
				return @default;
			int result;
			return int.TryParse(value, out result) ? result : @default;
		}
		
		public static long Get(this IKeyValueStorePersister instance, string key, long @default)
		{
			var value = instance.Get(key);
			if (string.IsNullOrEmpty(value))
				return @default;
			long result;
			return long.TryParse(value, out result) ? result : @default;
		}
	}
}