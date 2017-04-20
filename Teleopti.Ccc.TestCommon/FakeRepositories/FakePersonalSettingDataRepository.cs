using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting;
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
			value.SetOwner(new PersonalSettingData(entityName,null));
			return value.BelongsTo;
		}

		public T FindValueByKeyAndOwnerPerson<T>(string key, IPerson ownerPerson, T defaultValue) where T : class, ISettingValue
		{
			var ignorePersonForNow = new Func<string, T, T>(FindValueByKey);
			return ignorePersonForNow(key, defaultValue);
		}
	}
}