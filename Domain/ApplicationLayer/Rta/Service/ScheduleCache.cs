using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class ScheduleCache
	{
		private static readonly object lockObject = new object();
		private readonly IScheduleReader _reader;
		private readonly PerTenant<string> _version;
		private readonly PerTenant<int?> _latestUpdate;
		private readonly PerTenant<IDictionary<Guid, IEnumerable<ScheduledActivity>>> _dictionary;

		public ScheduleCache(IScheduleReader reader, ICurrentDataSource dataSource)
		{
			_reader = reader;
			_version = new PerTenant<string>(dataSource);
			_latestUpdate = new PerTenant<int?>(dataSource);
			_dictionary = new PerTenant<IDictionary<Guid, IEnumerable<ScheduledActivity>>>(dataSource);
		}

		public IEnumerable<ScheduledActivity> Read(Guid personId)
		{
			IEnumerable<ScheduledActivity> result;
			_dictionary.Value.TryGetValue(personId, out result);
			return result.EmptyIfNull();
		}

		[ReadModelUnitOfWork]
		protected virtual IEnumerable<CurrentSchedule> Read(int? lastUpdate)
		{
			return _reader.Read(lastUpdate);
		}

		public void Refresh(string latestVersion)
		{
			lock (lockObject)
			{
				if (!shouldRefresh(latestVersion))
					return;

				var updatedActivities = Read(_latestUpdate.Value);

				_dictionary.Set(
					merge(_dictionary.Value, updatedActivities.ToDictionary(x => x.PersonId, x => x.Schedule))
				);
				_latestUpdate.Set(updatedActivities.IsEmpty() ? 0 : updatedActivities.Max(x => x.LastUpdate));
				_version.Set(latestVersion);
			}
		}

		public void ClearCache()
		{
			_version?.Clear();
			_latestUpdate?.Clear();
			_dictionary?.Clear();
		}

		private bool shouldRefresh(string latestVersion)
		{
			return latestVersion != _version.Value || _version.Value == null;
		}

		private static IDictionary<Guid, IEnumerable<ScheduledActivity>> merge(params IDictionary<Guid, IEnumerable<ScheduledActivity>>[] dictionaries)
		{
			var result = new Dictionary<Guid, IEnumerable<ScheduledActivity>>();
			foreach (var dict in dictionaries)
			{
				if (dict == null)
					continue;
				foreach (var x in dict)
					result[x.Key] = x.Value;
			}

			return result;
		}
	}
}