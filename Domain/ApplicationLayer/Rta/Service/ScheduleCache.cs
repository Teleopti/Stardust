using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class ScheduleCache
	{
		private readonly IScheduleReader _reader;
		private readonly INow _now;
		private readonly PerTenant<string> _version;
		private readonly PerTenant<DateTime?> _updatedAt;
		private readonly PerTenant<IDictionary<Guid, IEnumerable<ScheduledActivity>>> _dictionary;

		public ScheduleCache(IScheduleReader reader, INow now, ICurrentDataSource dataSource)
		{
			_reader = reader;
			_now = now;
			_version = new PerTenant<string>(dataSource);
			_updatedAt = new PerTenant<DateTime?>(dataSource);
			_dictionary = new PerTenant<IDictionary<Guid, IEnumerable<ScheduledActivity>>>(dataSource);
		}

		public IEnumerable<ScheduledActivity> Read(Guid personId)
		{
			_dictionary.Value.TryGetValue(personId, out IEnumerable<ScheduledActivity> result);
			return result ?? Enumerable.Empty<ScheduledActivity>();
		}

		[ReadModelUnitOfWork]
		protected virtual IEnumerable<CurrentSchedule> Read(DateTime? lastUpdate)
		{
			return _reader.Read(lastUpdate);
		}

		public void Refresh(string latestVersion)
		{
			var refresh = latestVersion != _version.Value || _version.Value == null;
			if (!refresh)
				return;

			var updatedActivities = Read(_updatedAt.Value)
				.ToDictionary(x => x.PersonId, x => x.Schedule);

			_dictionary.Set(
				merge(_dictionary.Value, updatedActivities)
			);
			_updatedAt.Set(_now.UtcDateTime());
			_version.Set(latestVersion);
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