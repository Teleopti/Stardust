using System;
using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class ScheduleCache
	{
		private readonly IScheduleReader _reader;
		private readonly IKeyValueStorePersister _keyValues;
		private string _version;
		private IDictionary<Guid, IEnumerable<ScheduledActivity>> _dictionary = new Dictionary<Guid, IEnumerable<ScheduledActivity>>();

		public ScheduleCache(IScheduleReader reader, IKeyValueStorePersister keyValues)
		{
			_reader = reader;
			_keyValues = keyValues;
		}

		public IEnumerable<ScheduledActivity> Read(Guid personId)
		{
			IEnumerable<ScheduledActivity> result;
			_dictionary.TryGetValue(personId, out result);
			return result ?? Enumerable.Empty<ScheduledActivity>();
		}

		public void Refresh()
		{
			var version = _keyValues.Get("CurrentScheduleReadModelVersion");

			var refresh = version != _version || _version == null;
			if (!refresh)
				return;

			var activities = _reader.Read();

			_dictionary = activities
				.GroupBy(x => x.PersonId)
				.ToDictionary(x => x.Key, x => x.ToArray() as IEnumerable<ScheduledActivity>);
			_version = version;
		}
	}
}