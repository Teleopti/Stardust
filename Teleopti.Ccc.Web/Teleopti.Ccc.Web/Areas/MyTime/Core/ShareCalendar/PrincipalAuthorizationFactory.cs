using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.ShareCalendar
{
    public class PrincipalAuthorizationFactory : IPrincipalAuthorizationFactory
    {
        public IPrincipalAuthorization FromPrincipal(ITeleoptiPrincipal principal)
        {
            return new PrincipalAuthorization(new givenTeleoptiPrincipal(principal));
        }

        private class givenTeleoptiPrincipal : ICurrentTeleoptiPrincipal
        {
            private readonly ITeleoptiPrincipal _principal;

            public givenTeleoptiPrincipal(ITeleoptiPrincipal principal)
            {
                _principal = principal;
            }

            public ITeleoptiPrincipal Current()
            {
                return _principal;
            }
        }
    }
}