using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Common;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class ExternalLogonMapper
	{
		private readonly IExternalLogonReader _externalLogonReader;
		private readonly IKeyValueStorePersister _keyValueStore;
		private readonly PerTenant<string> _version;
		private readonly PerTenant<ILookup<key, Guid>> _cache1;
		private readonly PerTenant<ILookup<string, Guid>> _cache2;

		public ExternalLogonMapper(IExternalLogonReader externalLogonReader, IKeyValueStorePersister keyValueStore, ICurrentDataSource dataSource)
		{
			_externalLogonReader = externalLogonReader;
			_keyValueStore = keyValueStore;
			_version = new PerTenant<string>(dataSource);
			_cache1 = new PerTenant<ILookup<key, Guid>>(dataSource);
			_cache2 = new PerTenant<ILookup<string, Guid>>(dataSource);
		}

		public IEnumerable<Guid> PersonIdsFor(int dataSourceId, string userCode)
		{
			return _cache1.Value[new key
			{
				dataSourceId = dataSourceId,
				userCode = userCode
			}];
		}

		public IEnumerable<Guid> PersonIdsFor(string userCode)
		{
			return _cache2.Value[userCode];
		}

		[ReadModelUnitOfWork]
		public virtual void Refresh()
		{
			var latestVersion = _keyValueStore.Get("ExternalLogonReadModelVersion");
			var refresh = latestVersion != _version.Value || _version.Value == null;
			if (!refresh)
				return;

			var data = _externalLogonReader.Read();
			_cache1.Set(
				data
					.ToLookup(x => new key
					{
						dataSourceId = x.DataSourceId.Value,
						userCode = x.UserCode
					}, x => x.PersonId)
			);
			_cache2.Set(
				data.ToLookup(x => x.UserCode, x => x.PersonId)
			);
			_version.Set(latestVersion);
		}

		private class key
		{
			public int dataSourceId;
			public string userCode;

			#region

			protected bool Equals(key other)
			{
				return dataSourceId == other.dataSourceId
					   && string.Equals(userCode, other.userCode, StringComparison.OrdinalIgnoreCase);
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
					return (dataSourceId * 397) ^ (userCode != null ? userCode.ToUpper().GetHashCode() : 0);
				}
			}

			#endregion
		}
	}
}