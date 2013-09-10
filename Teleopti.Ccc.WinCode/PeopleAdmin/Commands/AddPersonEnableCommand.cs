using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.PeopleAdmin.Commands
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
    public interface IAddPersonEnableCommand : ICanExecute
	{
	}

	public class AddPersonEnableCommand : IAddPersonEnableCommand
	{
		public bool CanExecute()
		{
            return PrincipalAuthorization.Instance().IsPermitted(DefinedRaptorApplicationFunctionPaths.AllowPersonModifications);
		}
	}
}