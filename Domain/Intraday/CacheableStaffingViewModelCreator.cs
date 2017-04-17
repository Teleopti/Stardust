using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using log4net;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Intraday
{
	public class CacheableStaffingViewModelCreator : ICacheableStaffingViewModelCreator
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(CacheableStaffingViewModelCreator));
		private readonly IStaffingViewModelCreator _staffingViewModelCreator;
		private readonly IIntervalLengthFetcher _intervalLengthFetcher;
		private const double tolerance = 0.00001;

		public CacheableStaffingViewModelCreator(IStaffingViewModelCreator staffingViewModelCreator, IIntervalLengthFetcher intervalLengthFetcher)
		{
			_staffingViewModelCreator = staffingViewModelCreator;
			_intervalLengthFetcher = intervalLengthFetcher;
		}

		public IList<IntradayStaffingViewModel> Load(Guid skillId, DateOnlyPeriod dateOnlyPeriod, bool useShrinkage)
		{
			var intradayStaffingViewModels = new List<IntradayStaffingViewModel>();
			var days = dateOnlyPeriod.DayCollection();
			var notCachedDays = new List<DateOnly>();
			foreach (var day in days)
			{
				var cacheKey = getCacheKey(skillId, day, useShrinkage);
				var intradayStaffingViewModel = loadDataFromCache(cacheKey);
				if (intradayStaffingViewModel != null)
				{
					intradayStaffingViewModels.Add(intradayStaffingViewModel);
				}
				else
				{
					notCachedDays.Add(day);
				}
			}

			if (!notCachedDays.Any())
				return intradayStaffingViewModels;

			var loadedIntradayStaffingViewModels = _staffingViewModelCreator.Load(new[] { skillId }
			, new DateOnlyPeriod(notCachedDays.First(), notCachedDays.Last()), useShrinkage);

			foreach (var intradayStaffingViewModel in loadedIntradayStaffingViewModels)
			{
				logIntradayStaffingViewModel(skillId, useShrinkage, intradayStaffingViewModel);
				if (intradayStaffingViewModel.DataSeries == null)
					continue;

				intradayStaffingViewModels.Add(intradayStaffingViewModel);

				if (!existsStaffingData(intradayStaffingViewModel))
					continue;

				var cacheKey = getCacheKey(skillId, intradayStaffingViewModel.DataSeries.Date, useShrinkage);
				cacheData(cacheKey, intradayStaffingViewModel);
			}

			return intradayStaffingViewModels;
		}

		private static void cacheData(string cacheKey, IntradayStaffingViewModel intradyStaffingViewModel)
		{
			var cachePolicy = new CacheItemPolicy { SlidingExpiration = new TimeSpan(0, 10, 0) };
			MemoryCache.Default.Set(cacheKey, intradyStaffingViewModel, cachePolicy);
		}

		private static IntradayStaffingViewModel loadDataFromCache(string cacheKey)
		{
			var intradyStaffingViewModelCache = MemoryCache.Default.Get(cacheKey) as IntradayStaffingViewModel;
			return intradyStaffingViewModelCache;
		}

		private string getCacheKey(Guid skillId, DateOnly date, bool useShrinkage)
		{
			return $"{skillId}_{date.ToShortDateString()}_{useShrinkage}_{_intervalLengthFetcher.IntervalLength}";
		}

		private static bool existsStaffingData(IntradayStaffingViewModel intradyStaffingViewModel)
		{
			if (!intradyStaffingViewModel.StaffingHasData)
				return false;

			if (intradyStaffingViewModel.DataSeries.ScheduledStaffing == null
				|| intradyStaffingViewModel.DataSeries.ScheduledStaffing?.Length == 0)
				return false;

			if (intradyStaffingViewModel.DataSeries.ScheduledStaffing.All(s => Math.Abs(s.GetValueOrDefault()) < tolerance))
				return false;

			if (intradyStaffingViewModel.DataSeries.ForecastedStaffing.All(s => Math.Abs(s.GetValueOrDefault()) < tolerance))
				return false;

			return true;
		}

		private static void logIntradayStaffingViewModel(Guid skillId, bool useShrinkage,
			IntradayStaffingViewModel intradayStaffingViewModel)
		{
			if (intradayStaffingViewModel.DataSeries?.Time == null)
			{
				logger.Warn($"no data for {skillId}");
				return;
			}

			var stringBuilder = new StringBuilder();
			stringBuilder.AppendLine($"Skill:{skillId}, useShrinkage:{useShrinkage}, date:{intradayStaffingViewModel.DataSeries.Date}");
			for (int i = 0; i < intradayStaffingViewModel.DataSeries.Time.Length; i++)
			{
				stringBuilder.Append($"[{intradayStaffingViewModel.DataSeries.Time[i]},");
				stringBuilder.Append(i <= intradayStaffingViewModel.DataSeries.ScheduledStaffing?.Length - 1
					? $"{intradayStaffingViewModel.DataSeries.ScheduledStaffing[i] ?? double.NaN},"
					: "NotFound");
				stringBuilder.Append(i <= intradayStaffingViewModel.DataSeries.ForecastedStaffing?.Length - 1
					? $"{intradayStaffingViewModel.DataSeries.ForecastedStaffing[i] ?? double.NaN}"
					: "NotFound");
				stringBuilder.Append("],");
			}
			stringBuilder.AppendLine();
			logger.Warn(stringBuilder.ToString());
		}
	}

	public interface ICacheableStaffingViewModelCreator
	{
		IList<IntradayStaffingViewModel> Load(Guid skillId, DateOnlyPeriod dateOnlyPeriod, bool useShrinkage);
	}
}
