using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;

namespace Teleopti.Ccc.WebTest.Areas.Start.Controllers
{
    public class FakeAuthenticator : IIdentityLogon
	{
		private readonly AuthenticatorResult auth;

		public FakeAuthenticator()
		{
			auth = new AuthenticatorResult {Person = PersonFactory.CreatePerson()};
		}

        public AuthenticatorResult LogonIdentityUser()
        {
            return auth;
        }
    }
}