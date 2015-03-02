using System.Collections.Generic;
using log4net;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.TransformerInfrastructure
{
	internal class LogOnService : ILicenseFeedback
	{
		private readonly ILogOnOff _logOnOff;
		private readonly ILog _logger = LogManager.GetLogger(typeof(LogOnService));

		internal LogOnService(ILogOnOff logOnOff)
		{
			_logOnOff = logOnOff;
		}

		internal bool LogOn(DataSourceContainer selectedDataSource, IBusinessUnit businessUnit)
		{
			selectedDataSource.AvailableBusinessUnitProvider.LoadHierarchyInformation(businessUnit);

			_logOnOff.LogOn(selectedDataSource.DataSource, selectedDataSource.User, businessUnit);

			var unitOfWorkFactory = selectedDataSource.DataSource.Application;
			var licenseVerifier = new LicenseVerifier(this, unitOfWorkFactory, new LicenseRepository(unitOfWorkFactory));
			var licenseService = licenseVerifier.LoadAndVerifyLicense();
			if (licenseService == null)
				return false;

			LicenseProvider.ProvideLicenseActivator(unitOfWorkFactory.Name, licenseService);

			var roleToPrincipalCommand =
				new RoleToPrincipalCommand(
					new RoleToClaimSetTransformer(
						new FunctionsForRoleProvider(
							new LicensedFunctionsProvider(new DefinedRaptorApplicationFunctionFactory()),
							new ExternalFunctionsProvider(new RepositoryFactory())
							)
						)
					);
			using (var uow = unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				roleToPrincipalCommand.Execute(TeleoptiPrincipal.CurrentPrincipal, unitOfWorkFactory, new PersonRepository(uow));
			}
			return true;
		}

		internal void LogOff()
		{
			_logOnOff.LogOff();
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
