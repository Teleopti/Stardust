using System;
using System.Threading.Tasks;
using Owin;
using Teleopti.Ccc.Web.Core.Startup.Booter;
using log4net;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Core.Startup.VerifyLicense
{
	[TaskPriority(7)]
	public class VerifyLicenseTask : IBootstrapperTask, ILicenseFeedback
	{
		private readonly ILicenseVerifierFactory _licenseVerifierFactory;
		private readonly Lazy<IDataSourceForTenant> _dataSourceForTenant;
		private readonly ILog _logger;

		public VerifyLicenseTask(ILicenseVerifierFactory licenseVerifierFactory, 
								Lazy<IDataSourceForTenant> dataSourceForTenant,
								ILog logger)
		{
			_licenseVerifierFactory = licenseVerifierFactory;
			_dataSourceForTenant = dataSourceForTenant;
			_logger = logger;
		}

		public Task Execute(IAppBuilder application)
		{
			_dataSourceForTenant.Value.DoOnAllTenants_AvoidUsingThis(tenant =>
			{
				var unitOfWorkFactory = tenant.Application;
				var licenseVerifier = _licenseVerifierFactory.Create(this, unitOfWorkFactory);
				var licenseService = licenseVerifier.LoadAndVerifyLicense();
				if (licenseService != null)
				{
					LicenseProvider.ProvideLicenseActivator(tenant.DataSourceName, licenseService);
				}
			});
			return null;
		}

		public void Warning(string warning)
		{
			Warning(warning, "");
		}

	    public void Warning(string warning, string caption)
	    {
            _logger.Warn(warning);
	    }

	    public void Error(string error)
		{
			_logger.Error(error);
		}
	}
}