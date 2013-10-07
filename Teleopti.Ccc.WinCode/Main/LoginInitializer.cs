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
        bool InitializeApplication(IDataSourceContainer dataSourceContainer);
	}

	public class LoginInitializer : ILoginInitializer
	{
		private readonly IRoleToPrincipalCommand _roleToPrincipalCommand;
	    private readonly ILogonLicenseChecker _licenseChecker;
		private readonly RaptorApplicationFunctionsSynchronizer _raptorSynchronizer;

	    public LoginInitializer(IRoleToPrincipalCommand roleToPrincipalCommand, ILogonLicenseChecker licenseChecker, RaptorApplicationFunctionsSynchronizer raptorSynchronizer)
		{
		    _roleToPrincipalCommand = roleToPrincipalCommand;
		    _licenseChecker = licenseChecker;
		    _raptorSynchronizer = raptorSynchronizer;
		}

	    public bool InitializeApplication(IDataSourceContainer dataSourceContainer)
		{
			return setupCulture() &&
                   _licenseChecker.HasValidLicense(dataSourceContainer) &&
			       checkRaptorApplicationFunctions();
		}

		private static bool setupCulture()
		{
			if (TeleoptiPrincipal.Current.Regional == null) return false;

			Thread.CurrentThread.CurrentCulture =
				TeleoptiPrincipal.Current.Regional.Culture;
			Thread.CurrentThread.CurrentUICulture =
				TeleoptiPrincipal.Current.Regional.UICulture;
			return true;
		}

		private bool checkRaptorApplicationFunctions()
		{
			var repositoryFactory = new RepositoryFactory();
			;
			raptorSynchronizer = new RaptorApplicationFunctionsSynchronizer(repositoryFactory, UnitOfWorkFactory.Current);

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
		
		
	}
}
