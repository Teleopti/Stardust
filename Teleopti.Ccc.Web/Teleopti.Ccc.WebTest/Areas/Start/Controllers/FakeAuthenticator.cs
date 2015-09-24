using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;

namespace Teleopti.Ccc.WebTest.Areas.Start.Controllers
{
    public class FakeAuthenticator : IIdentityLogon
    {
        public AuthenticatorResult LogonIdentityUser()
        {
            var auth = new AuthenticatorResult();
            auth.Person = PersonFactory.CreatePerson();
            return auth;
        }
    }
}