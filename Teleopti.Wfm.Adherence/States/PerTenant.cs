using System;
using System.Collections.Concurrent;
using Teleopti.Ccc.Domain.Common;

namespace Teleopti.Wfm.Adherence.States
{
	public class PerTenant<T>
	{
		private readonly Func<T> _factory;
		private readonly ICurrentDataSource _dataSource;
		private readonly ConcurrentDictionary<string, T> _cache = new ConcurrentDictionary<string, T>();

		public PerTenant(ICurrentDataSource dataSource)
		{
			_dataSource = dataSource;
		}

		public PerTenant(ICurrentDataSource dataSource, Func<T> factory)
		{
			_dataSource = dataSource;
			_factory = factory;
		}

		public void Set(T value)
		{
			_cache.AddOrUpdate(_dataSource.CurrentName(), value, (key, existing) => value);
		}

		public T Value
		{
			get
			{
				T ret;
				if (_cache.TryGetValue(_dataSource.CurrentName(), out ret))
					return ret;
				if (_factory == null) 
					return ret;
				ret = _factory.Invoke();
				Set(ret);
				return ret;
			}
		}
	}
}