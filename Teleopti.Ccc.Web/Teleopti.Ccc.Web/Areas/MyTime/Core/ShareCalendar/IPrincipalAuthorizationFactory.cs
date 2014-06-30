using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.ShareCalendar
{
    public interface IPrincipalAuthorizationFactory
    {
        IPrincipalAuthorization FromPrincipal(ITeleoptiPrincipal principal);
    }
}