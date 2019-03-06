using System;
using System.Linq;
using Teleopti.Analytics.Etl.Common;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Infrastructure.Licensing;

namespace Teleopti.Wfm.Administration.Core
{
	public class LicenseDataFactoryWrapper : ILicenseDataFactoryWrapper
	{
		private readonly ITenants _tenants;
		private readonly IInitializeLicenseServiceForTenant _licenseInitializer;

		public LicenseDataFactoryWrapper(ITenants tenants, IInitializeLicenseServiceForTenant licenseInitializer)
		{
			_tenants = tenants;
			_licenseInitializer = licenseInitializer;
		}

		public ILicenseActivator GetLicenseActivator(string tenantName)
		{
			var tenant = _tenants.LoadedTenants().SingleOrDefault(t =>
				string.Compare(t.Name, tenantName, StringComparison.InvariantCultureIgnoreCase) == 0);
			if (tenant == null)
			{
				return null;
			}

			_licenseInitializer.TryInitialize(tenant.DataSource);
			return DefinedLicenseDataFactory.GetLicenseActivator(tenantName);
		}
	}
}