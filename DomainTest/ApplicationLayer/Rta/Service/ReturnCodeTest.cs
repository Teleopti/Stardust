using System.Collections.ObjectModel;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
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
		public void ShouldThrowIfNoSourceId()
		{
			var state = new ExternalUserStateForTest();
			Database
				.WithDataFromState(state);
			state.SourceId = string.Empty;

			Assert.Throws<InvalidSourceException>(() => Target.SaveState(state));
		}

		[Test]
		public void ShouldThrowIfNoPlatformId()
		{
			var state = new ExternalUserStateForTest();
			Database
				.WithDataFromState(state);
			state.PlatformTypeId = string.Empty;

			Assert.Throws<InvalidPlatformException>(() => Target.SaveState(state));
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

		[Test]
		public void ShouldThrowIfTooManyExternalUserStatesInBatch()
		{
			const int tooManyStates = 200;
			var externalStates = new Collection<ExternalUserStateForTest>();
			for (var i = 0; i < tooManyStates; i++)
				externalStates.Add(new ExternalUserStateForTest());
			var state = new ExternalUserStateForTest();
			Database
				.WithDataFromState(state);

			Assert.Throws(typeof(BatchTooBigException), () => Target.SaveStateBatch(externalStates));
		}

	}
}