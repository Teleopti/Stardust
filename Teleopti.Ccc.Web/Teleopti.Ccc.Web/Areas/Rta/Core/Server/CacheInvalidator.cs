using System;
using MbCache.Core;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Infrastructure.Rta;

namespace Teleopti.Ccc.Web.Areas.Rta.Core.Server
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

		public void Invalidate()
		{
			_cacheFactory.Invalidate<AlarmMappingLoader>();
			_cacheFactory.Invalidate<StateMappingLoader>();
		}

		public void InvalidateSchedules(Guid personId)
		{
			// TODO 
			if (_databaseReader is DatabaseReader)
				_cacheFactory.Invalidate(_databaseReader, x => x.GetCurrentSchedule(personId), true);
		}
	}
}