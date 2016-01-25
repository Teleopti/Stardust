using System.Collections.ObjectModel;
using NUnit.Framework;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service
{
	[RtaTest]
	[TestFixture]
	public class ExceptionTest
	{
		public FakeRtaDatabase Database;
		public Domain.ApplicationLayer.Rta.Service.Rta Target;

		[Test]
		public void ShouldThrowIfNoSourceId()
		{
			Assert.Throws<InvalidSourceException>(() =>
				Target.SaveState(new ExternalUserStateForTest
				{
					SourceId = string.Empty
				})
				);
		}

		[Test]
		public void ShouldThrowIfNoPlatformId()
		{
			Assert.Throws<InvalidPlatformException>(() =>
				Target.SaveState(new ExternalUserStateForTest
				{
					PlatformTypeId = string.Empty
				})
				);
		}

		[Test]
		public void ShouldThrowIfNoPerson()
		{
			Assert.Throws<InvalidUserCodeException>(() =>
				Target.SaveState(new ExternalUserStateForTest
				{
					UserCode = "unknown"
				})
				);
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