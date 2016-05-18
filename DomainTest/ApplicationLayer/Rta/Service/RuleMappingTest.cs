using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service
{
	[TestFixture]
	[RtaTest]
	public class RuleMappingTest
	{
		public FakeRtaDatabase Database;
		public Domain.ApplicationLayer.Rta.Service.Rta Target;
		public MutableNow Now;
		public FakeEventPublisher EventPublisher;
		
		[Test]
		public void ShouldMapAlarmWithoutStateGroup()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithUser("usercode", personId)
				.WithSchedule(personId, phone, "2015-03-12 8:00", "2015-03-12 9:00")
				.WithRule(null, phone, 0, Adherence.Out)
				;
			Now.Is("2015-03-12 08:05");

			Target.CheckForActivityChanges(Database.TenantName(), personId);

			EventPublisher.PublishedEvents.OfType<PersonOutOfAdherenceEvent>().Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldMapDefaultAdherenceWhenNoAlarm()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithUser("usercode", personId)
				.WithSchedule(personId, phone, "2015-03-12 8:00", "2015-03-12 9:00")
				.WithRule("phone", phone, (Guid?) null)
				;
			Now.Is("2015-03-12 08:05");

			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});

			EventPublisher.PublishedEvents.OfType<PersonNeutralAdherenceEvent>().Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldMapAlarmBasedOnPlatformTypeOfStateCode()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			var platform1 = Guid.NewGuid();
			var platform2 = Guid.NewGuid();
			Database
				.WithUser("usercode", personId)
				.WithSchedule(personId, phone, "2015-05-11 08:00", "2015-05-11 09:00")
				.WithRule("AUX1", platform1, phone, -1, Adherence.Out)
				.WithRule("AUX1", platform2, phone, 0, Adherence.In)
				;
			Now.Is("2015-05-11 08:00");

			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "AUX1",
				PlatformTypeId = platform2.ToString()
			});

			EventPublisher.PublishedEvents.OfType<PersonInAdherenceEvent>().Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldExcludeMapsWithStateGroupsWithoutStateCode()
		{
			var person = Guid.NewGuid();
			Database
				.WithUser("usercode", person)
				.WithMapWithStateGroupWithoutStateCodes()
				.WithRule(null, null, 0, Adherence.In)
				;

			Target.CheckForActivityChanges(Database.TenantName());

			EventPublisher.PublishedEvents.OfType<PersonInAdherenceEvent>().Should().Have.Count.EqualTo(1);
		}
	}
}