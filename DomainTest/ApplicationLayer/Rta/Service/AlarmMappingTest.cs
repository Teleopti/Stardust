using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service
{
	[TestFixture]
	[RtaTest]
	[Toggle(Toggles.RTA_NeutralAdherence_30930)]
	[Toggle(Toggles.RTA_SeeAdherenceDetailsForOneAgent_31285)]
	public class AlarmMappingTest
	{
		public FakeRtaDatabase Database;
		public IRta Target;
		public MutableNow Now;
		public FakeEventPublisher EventPublisher;

		[Test]
		public void ShouldMapWithNoStateGroup()
		{
			var businessUnitId = Guid.NewGuid();
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithBusinessUnit(businessUnitId)
				.WithUser("usercode", personId)
				.WithSchedule(personId, phone, "2015-03-12 8:00", "2015-03-12 9:00")
				.WithAlarm(null, phone, 0, Adherence.Out)
				;
			Now.Is("2015-03-12 08:05");

			Target.CheckForActivityChange(personId, businessUnitId);

			EventPublisher.PublishedEvents.OfType<PersonOutOfAdherenceEvent>().Should().Have.Count.EqualTo(1);
		}


		[Test]
		public void ShouldMapWithAlarmWithoutAlarm()
		{
			var businessUnitId = Guid.NewGuid();
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithBusinessUnit(businessUnitId)
				.WithUser("usercode", personId)
				.WithSchedule(personId, phone, "2015-03-12 8:00", "2015-03-12 9:00")
				.WithAlarm("phone", phone, (Guid?) null)
				;
			Now.Is("2015-03-12 08:05");

			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});

			EventPublisher.PublishedEvents.OfType<PersonNeutralAdherenceEvent>().Should().Have.Count.EqualTo(1);
		}

	}
}