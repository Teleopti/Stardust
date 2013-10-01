using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Linq;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Main
{
	public interface ILoginInitializer
	{
		bool InitializeApplication();
	}

	public class LoginInitializer : ILoginInitializer, ILicenseFeedback
	{
		private readonly IDataSourceContainer _dataSource;
		private readonly IRoleToPrincipalCommand _roleToPrincipalCommand;

		public LoginInitializer(IDataSourceContainer dataSource, IRoleToPrincipalCommand roleToPrincipalCommand)
		{
			_dataSource = dataSource;
			_roleToPrincipalCommand = roleToPrincipalCommand;
		}

		public bool InitializeApplication()
		{
			return setupCulture() &&
			       initializeLicense() &&
			       checkRaptorApplicationFunctions();
		}

		private bool setupCulture()
		{
			if (TeleoptiPrincipal.Current.Regional == null) return false;

			Thread.CurrentThread.CurrentCulture =
				TeleoptiPrincipal.Current.Regional.Culture;
			Thread.CurrentThread.CurrentUICulture =
				TeleoptiPrincipal.Current.Regional.UICulture;
			return true;
		}

		private bool initializeLicense()
		{
			var unitofWorkFactory = _dataSource.DataSource.Application;
			var verifier = new LicenseVerifier(this, unitofWorkFactory, new LicenseRepository(unitofWorkFactory));
			var licenseService = verifier.LoadAndVerifyLicense();
			if (licenseService == null) return false;
			LicenseProvider.ProvideLicenseActivator(licenseService);
			return checkStatusOfLicense(licenseService);
		}

		private bool checkStatusOfLicense(ILicenseService licenseService)
		{
			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var rep = new LicenseStatusRepository(UnitOfWorkFactory.Current);
				// if something goes wrong here the document is corrupt, handle that in some way ??
				var status = rep.LoadAll().First();
				var licenseStatus = new LicenseStatusXml(XDocument.Parse(status.XmlString));
				if (licenseStatus.StatusOk && licenseStatus.AlmostTooMany)
				{
					Warning(getAlmostTooManyAgentsWarning(licenseStatus.NumberOfActiveAgents, licenseService));
				}
				if (!licenseStatus.StatusOk && licenseStatus.DaysLeft > 0)
				{
					Warning(getLicenseIsOverUsedWarning(licenseService, licenseStatus));
				}
				if (!licenseStatus.StatusOk && licenseStatus.DaysLeft < 1)
				{
					Error(getTooManyAgentsExplanation(licenseService, UnitOfWorkFactory.Current.Name,
					                                  licenseStatus.NumberOfActiveAgents));
					return false;
				}
			}
			return true;
		}

		private static string getAlmostTooManyAgentsWarning(int numberOfActiveAgents, ILicenseService licenseService)
		{
			return licenseService.LicenseType.Equals(LicenseType.Agent)
					   ? String.Format(CultureInfo.CurrentCulture, Resources.YouHaveAlmostTooManyActiveAgents,
									   numberOfActiveAgents, licenseService.MaxActiveAgents)
					   : String.Format(CultureInfo.CurrentCulture, Resources.YouHaveAlmostTooManySeats,
									   licenseService.MaxSeats);
		}

		private static string getLicenseIsOverUsedWarning(ILicenseService licenseService, ILicenseStatusXml licenseStatus)
		{
			var maxLicensed = licenseService.LicenseType.Equals(LicenseType.Agent)
				                  ? licenseService.MaxActiveAgents
				                  : licenseService.MaxSeats;

			return String.Format(CultureInfo.CurrentCulture, Resources.TooManyAgentsIsUsedWarning,
			                     licenseStatus.NumberOfActiveAgents, maxLicensed, licenseStatus.DaysLeft);
		}

		private static string getTooManyAgentsExplanation(ILicenseService licenseService, string dataSourceName,
		                                                  int numberOfActiveAgents)
		{
			return licenseService.LicenseType == LicenseType.Agent
				       ? dataSourceName + "\r\n" +
				         String.Format(CultureInfo.CurrentCulture, Resources.YouHaveTooManyActiveAgents,
				                       numberOfActiveAgents, licenseService.MaxActiveAgents)
				       : dataSourceName + "\r\n" + String.Format(CultureInfo.CurrentCulture, Resources.YouHaveTooManySeats,
				                                                 licenseService.MaxSeats);
		}

		private bool checkRaptorApplicationFunctions()
		{
			var repositoryFactory = new RepositoryFactory();
			var raptorSynchronizer = new RaptorApplicationFunctionsSynchronizer(repositoryFactory, UnitOfWorkFactory.Current);

			var result = raptorSynchronizer.CheckRaptorApplicationFunctions();

			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				_roleToPrincipalCommand.Execute(TeleoptiPrincipal.Current, uow, repositoryFactory.CreatePersonRepository(uow));
			}

			if (result.Result)
				return true;

			var message =
				buildApplicationFunctionWarningMessage(result);
			var answer = MessageBox.Show(
				message,
				Resources.VerifyingPermissionsTreeDots,
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Warning,
				MessageBoxDefaultButton.Button1,
				0);

			return answer == DialogResult.Yes;
		}

		private static string buildApplicationFunctionWarningMessage(CheckRaptorApplicationFunctionsResult result)
		{
			var message = string.Empty;

			if (result.AddedFunctions.Any())
			{
				var p = result.AddedFunctions.Select(f => f.FunctionPath);
				var appFunctionsText = string.Join(", ", p);

				message = "The following Application Function(s) has been added recently in code but not found in the database: "
				          + appFunctionsText
				          + "\nYou can continue, but the program might be unstable. Continue?";
				return message;
			}

			if (result.DeletedFunctions.Any())
			{
				var p = result.DeletedFunctions.Select(f => f.FunctionPath);
				var appFunctionsText = string.Join(", ", p);

				message = "The following Application Function(s) has been removed recently from code but still exists in the database: "
				          + appFunctionsText
				          + "\nYou can continue, but the program might be unstable. Continue?";
			}

			return message;
		}
		
		public void Warning(string warning)
		{
			//TODO
			throw new NotImplementedException();
		}

		public void Error(string error)
		{
			//TODO
			throw new NotImplementedException();
		}
	}
}
