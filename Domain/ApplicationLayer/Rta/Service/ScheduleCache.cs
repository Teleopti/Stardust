using System;
using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class ScheduleCache
	{
		private readonly IScheduleReader _reader;
		private string _version;
		private IDictionary<Guid, IEnumerable<ScheduledActivity>> _dictionary = new Dictionary<Guid, IEnumerable<ScheduledActivity>>();

		public ScheduleCache(IScheduleReader reader)
		{
			_reader = reader;
		}

		public IEnumerable<ScheduledActivity> Read(Guid personId)
		{
			IEnumerable<ScheduledActivity> result;
			_dictionary.TryGetValue(personId, out result);
			return result ?? Enumerable.Empty<ScheduledActivity>();
		}

		public void Refresh(string latestVersion)
		{
			var refresh = latestVersion != _version || _version == null;
			if (!refresh)
				return;

			var activities = _reader.Read();

			_dictionary = activities
				.GroupBy(x => x.PersonId)
				.ToDictionary(x => x.Key, x => x.ToArray() as IEnumerable<ScheduledActivity>);
			_version = latestVersion;
		}
	}
}