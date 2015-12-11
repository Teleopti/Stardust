
using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service.StartTime
{
	[RtaTest]
	[TestFixture]
	public class StateStartTimeTest
	{
		public FakeRtaDatabase Database;
		public MutableNow Now;
		public Domain.ApplicationLayer.Rta.Service.Rta Target;

		[Test]
		public void ShouldHaveStateStartTimeWhenANewStateArrived()
		{
			var personId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			Database
				.WithUser("usercode", personId, businessUnitId);

			Now.Is("2015-12-10 8:00");
			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});

			Database.PersistedReadModel
				.StateStartTime.Should().Be("2015-12-10 8:00".Utc());
		}

		[Test]
		public void ShouldNotChangeStateStartTimeWhenStateDoesNotChange()
		{
			var personId = Guid.NewGuid();
			Database
				.WithDefaultsFromState(new ExternalUserStateForTest())
				.WithUser("usercode", personId)
				.WithRule("stateone", Guid.NewGuid());

			Now.Is("2015-12-10 8:00");
			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "stateone"
			});
			Now.Is("2015-12-10 8:30");
			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "stateone"
			});

			Database.PersistedReadModel
				.StateStartTime.Should().Be("2015-12-10 8:00".Utc());
		}

		[Test]
		public void ShouldUpdateStateStartTimeWhenStateChanges()
		{
			var personId = Guid.NewGuid();
			Database
				.WithDefaultsFromState(new ExternalUserStateForTest())
				.WithUser("usercode", personId)
				.WithRule("stateone", Guid.NewGuid())
				.WithRule("statetwo", Guid.NewGuid());

			Now.Is("2015-12-10 8:00");
			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "stateone"
			});
			Now.Is("2015-12-10 8:30");
			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statetwo"
			});


			Database.PersistedReadModel
				.StateStartTime.Should().Be("2015-12-10 8:30".Utc());
		}
	}
}
