using System;
using System.Threading.Tasks;
using Owin;
using Teleopti.Ccc.Web.Core.Startup.Booter;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Core.Startup.VerifyLicense
{
	[TaskPriority(7)]
	public class VerifyLicenseTask : IBootstrapperTask
	{
		private readonly ISetLicenseActivator _setLicenseActivator;
		private readonly Lazy<IDataSourceForTenant> _dataSourceForTenant;

		public VerifyLicenseTask(ISetLicenseActivator setLicenseActivator, 
								Lazy<IDataSourceForTenant> dataSourceForTenant)
		{
			_setLicenseActivator = setLicenseActivator;
			_dataSourceForTenant = dataSourceForTenant;
		}

		public Task Execute(IAppBuilder application)
		{
			_dataSourceForTenant.Value.DoOnAllTenants_AvoidUsingThis(tenant =>
			{
				_setLicenseActivator.Execute(tenant);
			});
			return null;
		}
	}
}