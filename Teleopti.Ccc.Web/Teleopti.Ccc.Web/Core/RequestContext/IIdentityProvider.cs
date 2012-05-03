using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Web.Core.RequestContext
{
	public interface IIdentityProvider
	{
		TeleoptiIdentity Current();
	}
}