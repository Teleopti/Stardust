using System;
using MbCache.Core;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;

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

		public void InvalidateSchedules(Guid personId)
		{
			_cacheFactory.Invalidate(_databaseReader, x => x.GetCurrentSchedule(personId), true);
		}
	}
}