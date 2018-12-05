using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common;

namespace Teleopti.Wfm.Adherence.States
{
	public class ExternalLogonMapper
	{
		private readonly IExternalLogonReader _externalLogonReader;
		private readonly IKeyValueStorePersister _keyValueStore;
		private readonly PerTenant<string> _version;
		private readonly PerTenant<ILookup<key, Guid>> _personIdForDataSourceAndUserCode;
		private readonly PerTenant<ILookup<string, Guid>> _personIdForUserCode;
		private readonly PerTenant<IDictionary<Guid, TimeZoneInfo>> _timeZoneForPersonId;

		public ExternalLogonMapper(IExternalLogonReader externalLogonReader, IKeyValueStorePersister keyValueStore, ICurrentDataSource dataSource)
		{
			_externalLogonReader = externalLogonReader;
			_keyValueStore = keyValueStore;
			_version = new PerTenant<string>(dataSource);
			_personIdForDataSourceAndUserCode = new PerTenant<ILookup<key, Guid>>(dataSource);
			_personIdForUserCode = new PerTenant<ILookup<string, Guid>>(dataSource);
			_timeZoneForPersonId = new PerTenant<IDictionary<Guid, TimeZoneInfo>>(dataSource);
		}

		public IEnumerable<Guid> PersonIdsFor(int dataSourceId, string userCode)
		{
			return _personIdForDataSourceAndUserCode.Value[new key
			{
				dataSourceId = dataSourceId,
				userCode = userCode
			}];
		}

		public IEnumerable<Guid> PersonIdsFor(string userCode)
		{
			return _personIdForUserCode.Value[userCode];
		}

		public TimeZoneInfo TimeZoneFor(Guid personId)
		{
			return _timeZoneForPersonId.Value[personId];
		}

		[ReadModelUnitOfWork]
		public virtual void Refresh()
		{
			var latestVersion = _keyValueStore.Get("ExternalLogonReadModelVersion");
			var refresh = latestVersion != _version.Value || _version.Value == null;
			if (!refresh)
				return;

			var data = _externalLogonReader.Read();
			_personIdForDataSourceAndUserCode.Set(
				data
					.ToLookup(x => new key
					{
						dataSourceId = x.DataSourceId.Value,
						userCode = x.UserCode
					}, x => x.PersonId)
			);
			_personIdForUserCode.Set(
				data.ToLookup(x => x.UserCode, x => x.PersonId)
			);
			_timeZoneForPersonId.Set(
				data.Select(x => new
					{
						x.PersonId,
						x.TimeZone
					})
					.Distinct()
					.ToDictionary(
						x => x.PersonId,
						x => allTimeZoneIds.Contains(x.TimeZone) ? TimeZoneInfo.FindSystemTimeZoneById(x.TimeZone) : TimeZoneInfo.Utc
					)
			);
			_version.Set(latestVersion);
		}

		private static readonly HashSet<string> allTimeZoneIds =
			new HashSet<string>(TimeZoneInfo.GetSystemTimeZones()
				.Select(tz => tz.Id));

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