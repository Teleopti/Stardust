using System;
using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class ExternalLogonMapper
	{
		private readonly IExternalLogonReader _externalLogonReader;
		private readonly IKeyValueStorePersister _keyValueStore;
		private string _version;
		private ILookup<key, Guid> _cache;

		public ExternalLogonMapper(IExternalLogonReader externalLogonReader, IKeyValueStorePersister keyValueStore)
		{
			_externalLogonReader = externalLogonReader;
			_keyValueStore = keyValueStore;
		}

		public IEnumerable<Guid> PersonIdsFor(int dataSourceId, string userCode)
		{
			return _cache[new key
			{
				dataSourceId = dataSourceId,
				userCode = userCode
			}];
		}

		public void Refresh()
		{
			var latestVersion = _keyValueStore.Get("ExternalLogonReadModelVersion");
			var refresh = latestVersion != _version || _version == null;
			if (!refresh)
				return;

			_cache = _externalLogonReader
				.Read()
				.ToLookup(x => new key
				{
					dataSourceId = x.DataSourceId.Value,
					userCode = x.UserCode
				}, x => x.PersonId);
			_version = latestVersion;
		}

		private class key
		{
			public int dataSourceId;
			public string userCode;

			#region
			protected bool Equals(key other)
			{
				return dataSourceId == other.dataSourceId 
					   && string.Equals(userCode, other.userCode);
			}

			public override bool Equals(object obj)
			{
				if (ReferenceEquals(null, obj)) return false;
				if (ReferenceEquals(this, obj)) return true;
				if (obj.GetType() != this.GetType()) return false;
				return Equals((key) obj);
			}

			public override int GetHashCode()
			{
				unchecked
				{
					return (dataSourceId*397) ^ (userCode != null ? userCode.GetHashCode() : 0);
				}
			}
			#endregion
		}
	}
}