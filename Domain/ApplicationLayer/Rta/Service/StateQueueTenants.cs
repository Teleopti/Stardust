using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class StateQueueTenants
	{
		private readonly INow _now;
		private readonly ICurrentDataSource _dataSource;
		private readonly object _lock = new object();
		private readonly IList<TenantInfo> _tenants = new List<TenantInfo>();

		public StateQueueTenants(INow now, ICurrentDataSource dataSource)
		{
			_now = now;
			_dataSource = dataSource;
		}

		internal class TenantInfo
		{
			public string Name { get; set; }
			public DateTime ProcessQueueUntil { get; set; }
		}

		public void Poke()
		{
			lock (_lock)
			{
				var tenant = _dataSource.CurrentName();
				var info = _tenants.SingleOrDefault(x => x.Name == tenant);
				if (info == null)
				{
					info = new TenantInfo { Name = tenant };
					_tenants.Add(info);
				}
				info.ProcessQueueUntil = _now.UtcDateTime().AddMinutes(1);
			}
		}

		public IEnumerable<string> ActiveTenants()
		{
			lock (_lock)
			{
				var toRemove = _tenants.Where(x => x.ProcessQueueUntil < _now.UtcDateTime()).ToList();
				toRemove.ForEach(x => _tenants.Remove(x));
				return _tenants.Select(x => x.Name).ToArray();
			}
		}

	}
}