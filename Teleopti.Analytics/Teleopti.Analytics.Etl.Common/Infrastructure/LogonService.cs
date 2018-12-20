using log4net;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;

namespace Teleopti.Analytics.Etl.Common.Infrastructure
{
	internal class LogOnService : ILicenseFeedback
	{
		private readonly ILogOnOff _logOnOff;
		private readonly IAvailableBusinessUnitsProvider _availableBusinessUnitsProvider;
		private readonly ILog _logger = LogManager.GetLogger(typeof(LogOnService));

		internal LogOnService(ILogOnOff logOnOff, IAvailableBusinessUnitsProvider availableBusinessUnitsProvider)
		{
			_logOnOff = logOnOff;
			_availableBusinessUnitsProvider = availableBusinessUnitsProvider;
		}

		internal bool LogOn(DataSourceContainer selectedDataSource, IBusinessUnit businessUnit)
		{
			_availableBusinessUnitsProvider.LoadHierarchyInformation(selectedDataSource.DataSource, businessUnit);

			_logOnOff.LogOnWithoutPermissions(selectedDataSource.DataSource, selectedDataSource.User, businessUnit);

			var unitOfWorkFactory = selectedDataSource.DataSource.Application;
			var licenseVerifier = new LicenseVerifier(this, unitOfWorkFactory, new LicenseRepository(new FromFactory(() => unitOfWorkFactory)));
			using (var uow = unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				var licenseService = licenseVerifier.LoadAndVerifyLicense();
				if (licenseService == null)
					return false;

				LicenseProvider.ProvideLicenseActivator(unitOfWorkFactory.Name, licenseService);


				var roleToPrincipalCommand =
					new RoleToPrincipalCommand(
						new ClaimSetForApplicationRole(
							new ApplicationFunctionsForRole(
								new LicensedFunctionsProvider(new DefinedRaptorApplicationFunctionFactory()),
								new ApplicationFunctionRepository(new ThisUnitOfWork(uow))
								)
							)
						);

				roleToPrincipalCommand.Execute(TeleoptiPrincipalForLegacy.CurrentPrincipal, new PersonRepository(new ThisUnitOfWork(uow)), unitOfWorkFactory.Name);
			}
			return true;
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
