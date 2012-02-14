using Teleopti.Ccc.Web.Areas.Start.Models.Authentication;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Start.Core.Authentication.ViewModelFactory
{
	public interface IAuthenticationViewModelFactory
	{
		SignInViewModel CreateSignInViewModel();
		SignInWindowsViewModel CreateSignInWindowsViewModel(SignInWindowsModel model);
		SignInApplicationViewModel CreateSignInApplicationViewModel(SignInApplicationModel model);
		SignInBusinessUnitViewModel CreateBusinessUnitViewModel(IDataSource dataSource, IPerson person);
	}
}