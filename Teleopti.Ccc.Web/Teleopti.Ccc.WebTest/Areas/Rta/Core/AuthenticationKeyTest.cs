using System.ServiceModel;
using NUnit.Framework;
using Teleopti.Ccc.Web.Areas.Rta;

namespace Teleopti.Ccc.WebTest.Areas.Rta.Core
{
	[TestFixture]
	public class AuthenticationKeyTest
	{
		[Test]
		public void ShouldThrowIfAuthenticationKeyIsIncorrect()
		{
			var state = new ExternalUserStateForTest { AuthenticationKey = "something" };
			var target = new TeleoptiRtaServiceForTest(state);

			state.AuthenticationKey += " else";

			Assert.Throws(typeof(FaultException), () => target.SaveExternalUserState(state));
		}

		[Test]
		public void ShouldAcceptIfThirdAndFourthLetterOfAuthenticationKeyIsCorrupted_BecauseOfEncodingIssuesWithThe3rdLetterOfTheDefaultKeyAndWeAreNotAllowedToChangeTheDefault()
		{
			var target = new TeleoptiRtaServiceForTest();
			var state = new ExternalUserStateForTest { AuthenticationKey = TeleoptiRtaService.DefaultAuthenticationKey };
			state.AuthenticationKey = state.AuthenticationKey.Remove(2, 2).Insert(2, "_");

			target.SaveExternalUserState(state);
		}
	}
}