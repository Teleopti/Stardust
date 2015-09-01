using System;
using MbCache.Core;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public class CacheInvalidator : ICacheInvalidator
	{
		private readonly IMbCacheFactory _cacheFactory;
		private readonly IDatabaseLoader _databaseLoader;

		public CacheInvalidator(
			IMbCacheFactory cacheFactory,
			IDatabaseLoader databaseLoader
			)
		{
			_cacheFactory = cacheFactory;
			_databaseLoader = databaseLoader;
		}

		public void InvalidateAll()
		{
			_cacheFactory.Invalidate<IDatabaseLoader>();
			Invalidate();
		}

		public void Invalidate()
		{
			_cacheFactory.Invalidate<AlarmMappingLoader>();
			_cacheFactory.Invalidate<StateMappingLoader>();
		}

		public void InvalidateSchedules(Guid personId)
		{
			_cacheFactory.Invalidate(_databaseLoader, x => x.GetCurrentSchedule(personId), true);
		}
	}

}