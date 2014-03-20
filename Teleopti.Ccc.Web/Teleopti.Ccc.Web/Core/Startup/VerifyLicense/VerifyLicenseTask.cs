using System;
using System.Threading.Tasks;
using Teleopti.Ccc.Web.Core.Startup.Booter;
using log4net;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Core.Startup.VerifyLicense
{
	[TaskPriority(7)]
	public class VerifyLicenseTask : IBootstrapperTask, ILicenseFeedback
	{
		private readonly ILicenseVerifierFactory _licenseVerifierFactory;
		private readonly Lazy<IApplicationData> _applicationData;
		private readonly ILog _logger;

		public VerifyLicenseTask(ILicenseVerifierFactory licenseVerifierFactory, 
								Lazy<IApplicationData> applicationData,
								ILog logger)
		{
			_licenseVerifierFactory = licenseVerifierFactory;
			_applicationData = applicationData;
			_logger = logger;
		}

		public Task Execute()
		{
			foreach (var dataSource in _applicationData.Value.RegisteredDataSourceCollection)
			{
				var unitOfWorkFactory = dataSource.Application;
				var licenseVerifier = _licenseVerifierFactory.Create(this,
																	 unitOfWorkFactory);
				var licenseService = licenseVerifier.LoadAndVerifyLicense();
				LicenseProvider.ProvideLicenseActivator(licenseService);
			}
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
			throw new PermissionException(error);
		}
	}
}