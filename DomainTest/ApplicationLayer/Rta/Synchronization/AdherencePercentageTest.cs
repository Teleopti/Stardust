using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Synchronization
{
	[RtaTest]
	[Toggle(Toggles.RTA_SeeAdherenceDetailsForOneAgent_31285)]
	[Toggle(Toggles.RTA_SeePercentageAdherenceForOneAgent_30783)]
	[Toggle(Toggles.RTA_NoBroker_31237)]
	[Toggle(Toggles.RTA_EventStreamInitialization_31237)]
	[TestFixture]
	public class AdherencePercentageTest
	{
		public FakeRtaDatabase Database;
		public IStateStreamSynchronizer Target;
		public IRta Rta;
		public FakeAdherencePercentageReadModelPersister Persister;
		public MutableNow Now;

		[Test]
		public void ShouldInitializeAdherencePercentage()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Now.Is("2015-01-08 11:00");
			Database
				.WithSchedule(personId, phone, "2015-01-08 11:00", "2015-01-08 13:00")
				.WithAlarm("phone", phone, 0)
				.WithUser("user", personId)
				;
			Rta.SaveState(new ExternalUserStateForTest
			{
				UserCode = "user",
				StateCode = "phone"
			});
			Now.Is("2015-01-08 12:00");

			Target.Initialize();

			Persister.Get(new DateOnly("2015-01-08".Utc()), personId).Should().Not.Be.Null();
		}

		[Test]
		// This happens when RTA_EventStreamInitialization_31237 is turned on and agents are not scheduled
		// Resulting in crash at startup when Updater tries to fetch readmodel with DateTime.MinValue
		public void ShouldNotInitializeWhenNoOngoingShift()
		{
			var personId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			Now.Is("2015-04-10 8:00");
			Database
				.WithSchedule(personId, Guid.NewGuid(), "2015-04-10 8:00", "2015-04-10 17:00")
				.WithUser("user", personId, businessUnitId, null, null);
			Rta.CheckForActivityChange(personId, businessUnitId);
			Persister.Clear();
			Now.Is("2015-04-11 8:00");

			Target.Initialize();

			Persister.PersistedModels.Should().Be.Empty();
		}
	}
}