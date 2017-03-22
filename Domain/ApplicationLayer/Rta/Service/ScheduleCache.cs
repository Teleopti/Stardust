using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Common;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class ScheduleCache
	{
		private readonly IScheduleReader _reader;
		private readonly PerTenant<string> _version;
		private readonly PerTenant<IDictionary<Guid, IEnumerable<ScheduledActivity>>> _dictionary;

		public ScheduleCache(IScheduleReader reader, ICurrentDataSource dataSource)
		{
			_reader = reader;
			_version = new PerTenant<string>(dataSource);
			_dictionary = new PerTenant<IDictionary<Guid, IEnumerable<ScheduledActivity>>>(dataSource);
		}

		public IEnumerable<ScheduledActivity> Read(Guid personId)
		{
			IEnumerable<ScheduledActivity> result;
			_dictionary.Value.TryGetValue(personId, out result);
			return result ?? Enumerable.Empty<ScheduledActivity>();
		}

		[ReadModelUnitOfWork]
		protected virtual IEnumerable<ScheduledActivity> Read()
		{
			return _reader.Read();
		}

		public void Refresh(string latestVersion)
		{
			var refresh = latestVersion != _version.Value || _version.Value == null;
			if (!refresh)
				return;

			var activities = Read();

			_dictionary.Set(
				activities
					.GroupBy(x => x.PersonId)
					.ToDictionary(x => x.Key, x => x.ToArray() as IEnumerable<ScheduledActivity>)
			);
			_version.Set(latestVersion);
		}
	}
}