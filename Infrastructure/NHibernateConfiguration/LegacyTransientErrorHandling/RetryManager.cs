using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration.LegacyTransientErrorHandling
{
	[RemoveMeWithToggle(Toggles.Tech_Moving_ResilientConnectionLogic)]
	public class RetryManager
	{
		private static RetryManager defaultRetryManager;
		private IDictionary<string, RetryStrategy> retryStrategies;
		private string defaultRetryStrategyName;
		private IDictionary<string, string> defaultRetryStrategyNamesMap;
		private IDictionary<string, RetryStrategy> defaultRetryStrategiesMap;
		private RetryStrategy defaultStrategy;

		public static void SetDefault(RetryManager retryManager, bool throwIfSet = true)
		{
			if (RetryManager.defaultRetryManager != null && throwIfSet && retryManager != RetryManager.defaultRetryManager)
				throw new InvalidOperationException("ExceptionRetryManagerAlreadySet");
			RetryManager.defaultRetryManager = retryManager;
		}

		public static RetryManager Instance
		{
			get
			{
				RetryManager defaultRetryManager = RetryManager.defaultRetryManager;
				if (defaultRetryManager == null)
					throw new InvalidOperationException("ExceptionRetryManagerNotSet");
				return defaultRetryManager;
			}
		}

		public RetryManager(IEnumerable<RetryStrategy> retryStrategies)
			: this(retryStrategies, (string)null, (IDictionary<string, string>)null)
		{
		}

		public RetryManager(IEnumerable<RetryStrategy> retryStrategies, string defaultRetryStrategyName)
			: this(retryStrategies, defaultRetryStrategyName, (IDictionary<string, string>)null)
		{
		}

		public RetryManager(IEnumerable<RetryStrategy> retryStrategies, string defaultRetryStrategyName, IDictionary<string, string> defaultRetryStrategyNamesMap)
		{
			this.retryStrategies = (IDictionary<string, RetryStrategy>)retryStrategies.ToDictionary<RetryStrategy, string>((Func<RetryStrategy, string>)(p => p.Name));
			this.defaultRetryStrategyNamesMap = defaultRetryStrategyNamesMap;
			this.DefaultRetryStrategyName = defaultRetryStrategyName;
			this.defaultRetryStrategiesMap = (IDictionary<string, RetryStrategy>)new Dictionary<string, RetryStrategy>();
			if (this.defaultRetryStrategyNamesMap == null)
				return;
			foreach (KeyValuePair<string, string> keyValuePair in this.defaultRetryStrategyNamesMap.Where<KeyValuePair<string, string>>((Func<KeyValuePair<string, string>, bool>)(x => !string.IsNullOrWhiteSpace(x.Value))))
			{
				RetryStrategy retryStrategy;
				if (!this.retryStrategies.TryGetValue(keyValuePair.Value, out retryStrategy))
					throw new ArgumentOutOfRangeException(nameof(defaultRetryStrategyNamesMap), string.Format((IFormatProvider)CultureInfo.CurrentCulture, "DefaultRetryStrategyMappingNotFound", new object[2]
					{
						(object) keyValuePair.Key,
						(object) keyValuePair.Value
					}));
				this.defaultRetryStrategiesMap.Add(keyValuePair.Key, retryStrategy);
			}
		}

		public string DefaultRetryStrategyName
		{
			get
			{
				return this.defaultRetryStrategyName;
			}
			set
			{
				if (!string.IsNullOrWhiteSpace(value))
				{
					RetryStrategy retryStrategy;
					if (this.retryStrategies.TryGetValue(value, out retryStrategy))
					{
						this.defaultRetryStrategyName = value;
						this.defaultStrategy = retryStrategy;
					}
					else
						throw new ArgumentOutOfRangeException(nameof(value), string.Format((IFormatProvider)CultureInfo.CurrentCulture, "RetryStrategyNotFound", new object[1]
						{
							(object) value
						}));
				}
				else
					this.defaultRetryStrategyName = (string)null;
			}
		}

		public virtual RetryPolicy<T> GetRetryPolicy<T>() where T : ITransientErrorDetectionStrategy, new()
		{
			return new RetryPolicy<T>(this.GetRetryStrategy());
		}

		public virtual RetryPolicy<T> GetRetryPolicy<T>(string retryStrategyName) where T : ITransientErrorDetectionStrategy, new()
		{
			return new RetryPolicy<T>(this.GetRetryStrategy(retryStrategyName));
		}

		public virtual RetryStrategy GetRetryStrategy()
		{
			return this.defaultStrategy;
		}

		public virtual RetryStrategy GetRetryStrategy(string retryStrategyName)
		{
			RetryStrategy retryStrategy;
			if (!this.retryStrategies.TryGetValue(retryStrategyName, out retryStrategy))
				throw new ArgumentOutOfRangeException(string.Format((IFormatProvider)CultureInfo.CurrentCulture, "RetryStrategyNotFound", new object[1]
				{
					(object) retryStrategyName
				}));
			return retryStrategy;
		}

		public virtual RetryStrategy GetDefaultRetryStrategy(string technology)
		{
			RetryStrategy defaultStrategy;
			if (!this.defaultRetryStrategiesMap.TryGetValue(technology, out defaultStrategy))
				defaultStrategy = this.defaultStrategy;
			if (defaultStrategy == null)
				throw new ArgumentOutOfRangeException(string.Format((IFormatProvider)CultureInfo.CurrentCulture, "DefaultRetryStrategyNotFound", new object[1]
				{
					(object) technology
				}));
			return defaultStrategy;
		}
	}
}