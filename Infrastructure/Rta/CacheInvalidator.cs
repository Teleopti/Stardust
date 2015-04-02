using System;
using MbCache.Core;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public class CacheInvalidator : ICacheInvalidator
	{
		private readonly IMbCacheFactory _cacheFactory;
		private readonly IScheduleLoader _scheduleLoader;

		public CacheInvalidator(
			IMbCacheFactory cacheFactory,
			IScheduleLoader scheduleLoader
			)
		{
			_cacheFactory = cacheFactory;
			_scheduleLoader = scheduleLoader;
		}

		public void InvalidateAll()
		{
			_cacheFactory.Invalidate<IDatabaseReader>();
			_cacheFactory.Invalidate<IScheduleLoader>();
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
			_cacheFactory.Invalidate(_scheduleLoader, x => x.GetCurrentSchedule(personId), true);
		}
	}

}