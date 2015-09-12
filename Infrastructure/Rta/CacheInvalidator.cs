using System;
using System.Threading;
using MbCache.Core;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Common;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public class CacheInvalidator : ICacheInvalidator
	{
		private readonly IMbCacheFactory _cacheFactory;
		private readonly IDatabaseLoader _databaseLoader;
		private readonly IDataSourceScope _dataSource;
		private readonly RtaTenants _tenants;

		public CacheInvalidator(
			IMbCacheFactory cacheFactory,
			IDatabaseLoader databaseLoader,
			IDataSourceScope dataSource,
			RtaTenants tenants
			)
		{
			_cacheFactory = cacheFactory;
			_databaseLoader = databaseLoader;
			_dataSource = dataSource;
			_tenants = tenants;
		}

		public void InvalidateAllForCurrentTenant()
		{
			_cacheFactory.Invalidate<IDatabaseLoader>();
			InvalidateStateForCurrentTenant();
		}

		public void InvalidateStateForCurrentTenant()
		{
			_cacheFactory.Invalidate<AlarmMappingLoader>();
			_cacheFactory.Invalidate<StateMappingLoader>();
		}

		public void InvalidateSchedulesForCurrentTenant(Guid personId)
		{
			_cacheFactory.Invalidate(_databaseLoader, x => x.GetCurrentSchedule(personId), true);
		}

		public void InvalidateAllForAllTenants()
		{
			_tenants.ForAllTenants(t =>
			{
				using (_dataSource.OnThisThreadUse(t))
				{
					InvalidateAllForCurrentTenant();
					Thread.Sleep(1000);
				}
			});
		}
	}

}