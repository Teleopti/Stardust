using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakePersonalSettingDataRepository : IPersonalSettingDataRepository
	{
		private readonly IDictionary<string, object> storage = new Dictionary<string, object>();

		public T FindValueByKey<T>(string key, T defaultValue) where T : class, ISettingValue
		{
			object value;
			if (storage.TryGetValue(key, out value))
				return (T)value ?? defaultValue;

			return defaultValue;
		}

		public ISettingData PersistSettingValue(ISettingValue value)
		{
			storage[value.BelongsTo.Key] = value;
			return value.BelongsTo;
		}

		public ISettingData PersistSettingValue(string entityName, ISettingValue value)
		{
			storage[entityName] = value;
			return value.BelongsTo;
		}

		public T FindValueByKeyAndOwnerPerson<T>(string key, IPerson ownerPerson, T defaultValue) where T : class, ISettingValue
		{
			throw new System.NotImplementedException();
		}
	}
}