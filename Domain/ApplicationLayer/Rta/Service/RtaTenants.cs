using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class RtaTenants
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
		
	}
}