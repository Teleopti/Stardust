using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Main
{
	public interface ILoginInitializer
	{
        bool InitializeApplication(IDataSourceContainer dataSourceContainer);
	}

	public class LoginInitializer : ILoginInitializer
	{
		private readonly IRoleToPrincipalCommand _roleToPrincipalCommand;
	    private readonly ILogonLicenseChecker _licenseChecker;
	    private readonly ILogonView _logonView;
	    private readonly IRaptorApplicationFunctionsSynchronizer _raptorSynchronizer;
	    private readonly ILogonMatrix _logonMatrix;

	    public LoginInitializer(IRoleToPrincipalCommand roleToPrincipalCommand,
		                        ILogonLicenseChecker licenseChecker, ILogonView logonView,
		                        IRaptorApplicationFunctionsSynchronizer raptorSynchronizer, ILogonMatrix logonMatrix)
		{
		    _roleToPrincipalCommand = roleToPrincipalCommand;
		    _licenseChecker = licenseChecker;
	        _logonView = logonView;
	        _raptorSynchronizer = raptorSynchronizer;
		    _logonMatrix = logonMatrix;
		}

	    public bool InitializeApplication(IDataSourceContainer dataSourceContainer)
		{
			var result =  setupCulture() &&
                   _licenseChecker.HasValidLicense(dataSourceContainer) &&
			       checkRaptorApplicationFunctions(dataSourceContainer) &&
                   authorize();
	        if (!result)
	            return false;
            syncMatrix(dataSourceContainer);
	        return true;
		}

		private static bool setupCulture()
		{
			if (TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional == null) return false;

			Thread.CurrentThread.CurrentCulture =
				TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.Culture;
			Thread.CurrentThread.CurrentUICulture =
				TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.UICulture;
			return true;
		}

		private bool checkRaptorApplicationFunctions(IDataSourceContainer dataSourceContainer)
		{
			var unitOfWorkFactory = dataSourceContainer.DataSource.Application;
			var repositoryFactory = new RepositoryFactory();
			var result = _raptorSynchronizer.CheckRaptorApplicationFunctions();

			using (var uow = unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				_roleToPrincipalCommand.Execute(TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal, repositoryFactory.CreatePersonRepository(uow), unitOfWorkFactory.Name);
			}

			if (result.Result)
				return true;

			var message =
				buildApplicationFunctionWarningMessage(result);
		    var answer = _logonView.ShowYesNoMessage(message, Resources.VerifyingPermissionsTreeDots,
		                                             MessageBoxDefaultButton.Button1);
			
			return answer == DialogResult.Yes;
		}

        private bool authorize()
        {
            if (!PrincipalAuthorization.Current().IsPermitted(
                DefinedRaptorApplicationFunctionPaths.OpenRaptorApplication))
            {
                _logonView.ShowErrorMessage(string.Concat(Resources.YouAreNotAuthorizedToRunTheApplication, "  "), Resources.AuthenticationFailed);
               return false;
            }
            return true;
        }

        private void syncMatrix(IDataSourceContainer dataSourceContainer)
        {
            _logonMatrix.SynchronizeAndLoadMatrixReports(dataSourceContainer.DataSource.Application);
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
