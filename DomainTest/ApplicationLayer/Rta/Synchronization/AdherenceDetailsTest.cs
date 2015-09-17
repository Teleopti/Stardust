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
	[Toggle(Toggles.RTA_NewEventHangfireRTA_34333)]
	[Toggle(Toggles.RTA_AdherenceDetails_34267)]
	[TestFixture]
	public class AdherenceDetailsTest
	{
		public FakeRtaDatabase Database;
		public FakeAdherenceDetailsReadModelPersister Persister;
		public MutableNow Now;
		public Domain.ApplicationLayer.Rta.Service.Rta Rta;
		public RtaTestAttribute Context;

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

			Context.SimulateRestartWith(Now, Database);
			Rta.SaveState(new ExternalUserStateForTest());

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

			Context.SimulateRestartWith(Now, Database);
			Rta.SaveState(new ExternalUserStateForTest());

			Persister.Get(personId, new DateOnly("2015-01-08 12:00".Utc())).Model.Should().Be.Null();
		}

	}
}