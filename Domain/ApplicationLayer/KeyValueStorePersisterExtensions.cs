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

		public static int Get(this IKeyValueStorePersister instance, string key, int @default)
		{
			var value = instance.Get(key);
			if (string.IsNullOrEmpty(value))
				return @default;
			int result;
			return int.TryParse(value, out result) ? result : @default;
		}
	}
}