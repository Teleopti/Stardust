using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.MultiTenancy;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class TenantsInitializedInRta : IAllTenantNames
	{
		private readonly IList<string> _initialized = new List<string>();

		public bool IsInitialized(string tenant)
		{
			return _initialized.Contains(tenant);
		}

		public void Initialized(string tenant)
		{
			_initialized.Add(tenant);
		}

		public void ForAllTenants(Action<string> action)
		{
			_initialized.ForEach(action);
		}

		public void Forget()
		{
			_initialized.Clear();
        }

		public IEnumerable<string> Tenants()
		{
			return _initialized.ToArray();
		}
	}
}