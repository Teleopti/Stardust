using NUnit.Framework;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Web.Areas.Rta;

namespace Teleopti.Ccc.WebTest.Areas.Rta
{
	[TestFixture]
	public class AuthenticationKeyTest
	{
		[Test]
		public void ShouldThrowIfAuthenticationKeyIsIncorrect()
		{
			var state = new ExternalUserStateForTest { AuthenticationKey = "something" };
			var target = RtaForTest.MakeBasedOnState(state);

			state.AuthenticationKey += " else";

			Assert.Throws(typeof(InvalidAuthenticationKeyException), () => target.SaveState(state));
		}

		[Test]
		public void ShouldAcceptIfThirdAndFourthLetterOfAuthenticationKeyIsCorrupted_BecauseOfEncodingIssuesWithThe3rdLetterOfTheDefaultKeyAndWeAreNotAllowedToChangeTheDefault()
		{
			var target = RtaForTest.Make();
			var state = new ExternalUserStateForTest { AuthenticationKey = Domain.ApplicationLayer.Rta.Rta.DefaultAuthenticationKey };
			state.AuthenticationKey = state.AuthenticationKey.Remove(2, 2).Insert(2, "_");

			target.SaveState(state);
		}
	}
}