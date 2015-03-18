using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service;
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
	public class AdherenceDetailsTest
	{
		public FakeRtaDatabase Database;
		public IStateStreamSynchronizer Target;
		public FakeAdherenceDetailsReadModelPersister Persister;
		public MutableNow Now;
		public IRta Rta;

		[Test]
		public void ShouldInitializeAdherenceDetails()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Now.Is("2015-01-08 12:00");
			Database
				.WithUser("user", personId)
				.WithSchedule(personId, phone, "2015-01-08 11:00", "2015-01-08 13:00")
				.WithAlarm("phone", phone, 0);
			Rta.SaveState(new ExternalUserStateForTest
			{
				UserCode = "user", 
				StateCode = "phone"
			});

			Target.Initialize("Teleopti WFM");

			Persister.Get(personId, new DateOnly("2015-01-08 12:00".Utc())).Model.Activities.Single().StartTime
				.Should().Be("2015-01-08 11:00".Utc());
		}

		[Test]
		public void ShouldNotReinitializeAdherenceDetails()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Now.Is("2015-01-08 12:00");
			Database
				.WithUser("user", personId)
				.WithSchedule(personId, phone, "2015-01-08 11:00", "2015-01-08 13:00")
				.WithAlarm("phone", phone, 0);
			Rta.SaveState(new ExternalUserStateForTest
			{
				UserCode = "user", 
				StateCode = "phone"
			});
			Persister.Add(new AdherenceDetailsReadModel
			{
				PersonId = personId,
				Date = "2015-01-08".Utc(),
				Model = null
			});

			Target.Initialize("Teleopti WFM");

			Persister.Get(personId, new DateOnly("2015-01-08 12:00".Utc())).Model.Should().Be.Null();
		}

	}
}