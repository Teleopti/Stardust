using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service
{
	[TestFixture]
	[RtaTest]
	[Toggle(Toggles.RTA_NeutralAdherence_30930)]
	public class PersonAdherenceScheduleRemovedTest
	{
		public FakeRtaDatabase Database;
		public MutableNow Now;
		public FakeEventPublisher Publisher;
		public Domain.ApplicationLayer.Rta.Service.Rta Target;

		[Test]
		public void ShouldPublishInAdherenceOnCurrentTimeWhenShiftIsRemoved()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithUser("usercode", personId)
				.WithSchedule(personId, phone, "2016-02-23 08:00", "2016-02-23 10:00")
				.WithRule("loggedout", phone, -1, Adherence.Out)
				.WithRule("loggedout", null, 0, Adherence.In)
				;

			Now.Is("2016-02-23 08:05");
			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "loggedout"
			});

			Database.ClearSchedule(personId);
			Target.ReloadSchedulesOnNextCheckForActivityChanges(Database.TenantName(), personId);
			Target.CheckForActivityChanges(Database.TenantName());

			Publisher.PublishedEvents.OfType<PersonInAdherenceEvent>().Single().Timestamp.Should().Be("2016-02-23 08:05".Utc());
		}
	}
}