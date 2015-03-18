using NUnit.Framework;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service
{
	[RtaTest]
	[TestFixture]
	public class AuthenticationKeyTest
	{
		public FakeRtaDatabase Database;
		public IRta Target;

		[Test]
		public void ShouldThrowIfAuthenticationKeyIsIncorrect()
		{
			var state = new ExternalUserStateForTest { AuthenticationKey = "something" };
			Database.WithDataFromState(state);

			state.AuthenticationKey += " else";

			Assert.Throws(typeof(InvalidAuthenticationKeyException), () => Target.SaveState(state));
		}

		[Test]
		public void ShouldAcceptIfThirdAndFourthLetterOfAuthenticationKeyIsCorrupted_BecauseOfEncodingIssuesWithThe3rdLetterOfTheDefaultKeyAndWeAreNotAllowedToChangeTheDefault()
		{
			var state = new ExternalUserStateForTest { AuthenticationKey = Domain.ApplicationLayer.Rta.Service.Rta.DefaultAuthenticationKey };
			state.AuthenticationKey = state.AuthenticationKey.Remove(2, 2).Insert(2, "_");

			Target.SaveState(state);
		}
	}
}