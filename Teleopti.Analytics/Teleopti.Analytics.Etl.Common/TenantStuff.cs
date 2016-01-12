using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer.Job;

namespace Teleopti.Analytics.Etl.Common
{
	public class Tenants
	{
		private readonly JobHelper _jobHelper;

		public Tenants(JobHelper jobHelper)
		{
			_jobHelper = jobHelper;
		}

		public IEnumerable<ITenantName> CurrentTenants()
		{
			_jobHelper.RefreshTenantList();
			return _jobHelper.TenantCollection;
		}
	}
}