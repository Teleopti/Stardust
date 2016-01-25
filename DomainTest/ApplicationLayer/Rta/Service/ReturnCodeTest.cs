using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service
{
	[RtaTest]
	[TestFixture]
	public class ReturnCodeTest
	{
		public FakeRtaDatabase Database;
		public Domain.ApplicationLayer.Rta.Service.Rta Target;

		[Test]
		public void ShouldProcessValidState()
		{
			var state = new ExternalUserStateForTest();
			Database
				.WithDataFromState(state);
			
			Target.SaveState(state)
				.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldProcessExternalUserStatesInBatch()
		{
			var state1 = new ExternalUserStateForTest();
			var state2 = new ExternalUserStateForTest();
			Database
				.WithDataFromState(state1);

			Target.SaveStateBatch(new[] { state1, state2 })
				.Should().Be.EqualTo(1);
		}

	}
}