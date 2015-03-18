using System;
using MbCache.Core;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public class CacheInvalidator : ICacheInvalidator
	{
		private readonly IMbCacheFactory _cacheFactory;
		private readonly IDatabaseReader _databaseReader;

		public CacheInvalidator(
			IMbCacheFactory cacheFactory,
			IDatabaseReader databaseReader
			)
		{
			_cacheFactory = cacheFactory;
			_databaseReader = databaseReader;
		}

		public void InvalidateAll()
		{
			_cacheFactory.Invalidate<IDatabaseReader>();
			_cacheFactory.Invalidate<IPersonOrganizationProvider>();
			Invalidate();
		}

		public void Invalidate()
		{
			_cacheFactory.Invalidate<AlarmMappingLoader>();
			_cacheFactory.Invalidate<StateMappingLoader>();
		}

		public void InvalidateSchedules(Guid personId, string tenant)
		{
			// if (_databaseReader is DatabaseReader) is to make testing with caching turned on work.
			// when the caching is moved to an rta internal layer this wont be needed any more.
			// now the _databaseReader is a RtaFakeDatabase when testing and thats not cached.
			// yeah right, like this will make sense when I read it next time....
			if (_databaseReader is DatabaseReader)
				_cacheFactory.Invalidate(_databaseReader, x => x.GetCurrentSchedule(personId, tenant), true);
		}
	}
}