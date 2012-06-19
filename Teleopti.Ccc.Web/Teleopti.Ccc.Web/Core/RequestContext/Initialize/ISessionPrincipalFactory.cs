using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Web.Core.RequestContext.Initialize
{
	/// <summary>
	/// Will give correct principal object back
	/// </summary>
	public interface ISessionPrincipalFactory
	{
		ITeleoptiPrincipal Generate();
	}
}