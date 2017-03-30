using System.Collections.Concurrent;
using Teleopti.Ccc.Domain.Common;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class PerTenant<T>
	{
		private readonly ICurrentDataSource _dataSource;
		private ConcurrentDictionary<string, T> _cache = new ConcurrentDictionary<string, T>();

		public PerTenant(ICurrentDataSource dataSource)
		{
			_dataSource = dataSource;
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
				_cache.TryGetValue(_dataSource.CurrentName(), out ret);
				return ret;
			}
		}

		public void Clear()
		{
			_cache = new ConcurrentDictionary<string, T>();
		}
	}
}