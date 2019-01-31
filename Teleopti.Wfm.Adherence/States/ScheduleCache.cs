using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;

namespace Teleopti.Wfm.Adherence.States
{
	public class ScheduleCache
	{
		private static readonly object lockObject = new object();
		private readonly IScheduleReader _reader;
		private readonly PerTenant<CurrentScheduleReadModelVersion> _version;
		private readonly PerTenant<IDictionary<Guid, IEnumerable<ScheduledActivity>>> _cache;

		public ScheduleCache(IScheduleReader reader, ICurrentDataSource dataSource)
		{
			_reader = reader;
			_version = new PerTenant<CurrentScheduleReadModelVersion>(dataSource);
			_cache = new PerTenant<IDictionary<Guid, IEnumerable<ScheduledActivity>>>(dataSource);
		}

		public IEnumerable<ScheduledActivity> Read(Guid personId)
		{
			IEnumerable<ScheduledActivity> result = null;
			_cache.Value?.TryGetValue(personId, out result);
			return result.EmptyIfNull();
		}

		[ReadModelUnitOfWork]
		protected virtual IEnumerable<CurrentSchedule> Read(int fromRevision)
		{
			return _reader.Read(fromRevision);
		}

		[ReadModelUnitOfWork]
		protected virtual IEnumerable<CurrentSchedule> Read()
		{
			return _reader.Read();
		}

		public void Refresh(CurrentScheduleReadModelVersion latestVersion)
		{
			if (latestVersion == null)
				return;

			if (latestVersion.Equals(_version.Value))
				return;
			
			lock (lockObject)
			{
				if (latestVersion.Equals(_version.Value))
					return;

				IEnumerable<CurrentSchedule> updates;
				if (_version.Value == null || _version.Value.Version() != latestVersion.Version())
					updates = Read();
				else
					updates = Read(_version.Value.Revision());
				var schedulePerPerson = updates.GroupBy(x => x.PersonId).ToDictionary(x => x.Key, x => x.Last().Schedule);
				_cache.Set(mergeDictionaries(_cache.Value, schedulePerPerson));

				_version.Set(latestVersion);
			}
		}

		private static IDictionary<Guid, IEnumerable<ScheduledActivity>> mergeDictionaries(params IDictionary<Guid, IEnumerable<ScheduledActivity>>[] dictionaries)
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

	public class CurrentScheduleReadModelVersion
	{
		private readonly Guid _cacheVersion;
		private readonly int _scheduleRevision;

		public static CurrentScheduleReadModelVersion Parse(string str)
		{
			var splat = str?.Split(":");
			if (splat == null || splat.Length != 2)
				return null;

			Guid g;
			int i;
			if (Guid.TryParse(splat[0], out g) && int.TryParse(splat[1], out i))
				return new CurrentScheduleReadModelVersion(g, i);

			return null;
		}

		public static CurrentScheduleReadModelVersion Generate()
		{
			return new CurrentScheduleReadModelVersion(Guid.NewGuid(), 0);
		}

		private CurrentScheduleReadModelVersion(Guid cacheVersion, int scheduleRevision)
		{
			_cacheVersion = cacheVersion;
			_scheduleRevision = scheduleRevision;
		}

		public int Revision() => _scheduleRevision;
		public Guid Version() => _cacheVersion;

		public CurrentScheduleReadModelVersion NextRevision() => new CurrentScheduleReadModelVersion(_cacheVersion, _scheduleRevision + 1);

		protected bool Equals(CurrentScheduleReadModelVersion other)
		{
			return _cacheVersion.Equals(other._cacheVersion) && _scheduleRevision == other._scheduleRevision;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != GetType()) return false;
			return Equals((CurrentScheduleReadModelVersion) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (_cacheVersion.GetHashCode() * 397) ^ _scheduleRevision;
			}
		}

		public override string ToString() => $"{_cacheVersion}:{_scheduleRevision}";
	}
}