using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Core.RequestContext
{
	public interface ILoggedOnUser
	{
		IPerson CurrentUser();
		ITeam MyTeam(DateOnly date);
	}
}