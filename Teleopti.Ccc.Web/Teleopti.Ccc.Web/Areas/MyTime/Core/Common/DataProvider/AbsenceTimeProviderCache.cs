using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Caching;
using System.Web.Configuration;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider
{
	public class AbsenceTimeProviderCache : IAbsenceTimeProviderCache
	{
		private String _cacheKey;
		private int _cacheExpiry;
		private bool _cacheEnabled;


		public void Setup(IScenario scenario, DateOnlyPeriod period, IBudgetGroup budgetGroup)
		{
			_cacheKey = GetCacheKey(period, budgetGroup, scenario);

			getCacheExpiryValue();
		}

		private void getCacheExpiryValue()
		{
			int expiryValue;
			_cacheEnabled = int.TryParse(GetConfigValue(), out expiryValue) && expiryValue != 0;

			if (_cacheEnabled)
			{
				_cacheExpiry = expiryValue;
			}
		}

		public virtual string GetConfigValue()
		{
			return WebConfigurationManager.AppSettings["BudgetGroupAbsenceTimeCacheExpiryInSeconds"];
		}

		public IEnumerable<PayloadWorkTime> Get()
		{
			if (!_cacheEnabled)
			{
				return null;
			}

			return HttpRuntime.Cache.Get(_cacheKey) as IEnumerable<PayloadWorkTime>;
		}

		public void Add(IEnumerable<PayloadWorkTime> absenceTime)
		{
			if (!_cacheEnabled)
			{
				return;
			}

			if (HttpRuntime.Cache.Get(_cacheKey) == null)
			{
				HttpRuntime.Cache.Add(_cacheKey, absenceTime, null, DateTime.UtcNow.AddSeconds(_cacheExpiry), Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
			}
		}

		public static String GetCacheKey(DateOnlyPeriod period, IBudgetGroup budgetGroup, IScenario scenario)
		{
			return period.DateString + budgetGroup.Id + scenario.Id;
		}


	}
}