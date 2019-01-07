using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Wfm.Adherence.States;
using Teleopti.Wfm.Adherence.States.Events;

namespace Teleopti.Wfm.Adherence.Test.States.Unit.Service
{
	[TestFixture]
	[RtaTest]
	public class RuleMappingTest
	{
		public FakeDatabase Database;
		public Rta Target;
		public MutableNow Now;
		public FakeEventPublisher EventPublisher;
		
		[Test]
		public void ShouldMapAlarmWithoutStateGroup()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, phone, "2015-03-12 8:00", "2015-03-12 9:00")
				.WithMappedRule(null, phone, 0, Adherence.Configuration.Adherence.Out)
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
				.WithAgent("usercode", personId)
				.WithSchedule(personId, phone, "2015-03-12 8:00", "2015-03-12 9:00")
				.WithMappedRule("phone", phone, (Guid?) null)
				;
			Now.Is("2015-03-12 08:05");

			Target.ProcessState(new StateForTest
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
				.WithAgent("usercode", personId)
				.WithSchedule(personId, phone, "2015-05-11 08:00", "2015-05-11 09:00")
				.WithMappedRule("AUX1 " + platform1, phone, -1, Adherence.Configuration.Adherence.Out)
				.WithMappedRule("AUX1 " + platform2, phone, 0, Adherence.Configuration.Adherence.In)
				;
			Now.Is("2015-05-11 08:00");

			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "AUX1 " + platform2
			});

			EventPublisher.PublishedEvents.OfType<PersonInAdherenceEvent>().Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldExcludeMapsWithStateGroupsWithoutStateCode()
		{
			var person = Guid.NewGuid();
			Database
				.WithAgent("usercode", person)
				.WithMapWithStateGroupWithoutStateCodes()
				.WithMappedRule(null, null, 0, Adherence.Configuration.Adherence.In)
				;

			Target.CheckForActivityChanges(Database.TenantName());

			EventPublisher.PublishedEvents.OfType<PersonInAdherenceEvent>().Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldHandleMultipleStateCodesInStateGroup()
		{
			var phone = Guid.NewGuid();
			Database
				.WithAgent("usercode")
				.WithStateGroup(phone, "phone")
				.WithStateCode("Phone")
				.WithStateCode("Ready")
				;

			Assert.DoesNotThrow(() =>
			{
				Target.ProcessState(new StateForTest
				{
					StateCode = "Ready",
					UserCode = "usercode"
				});
			});
		}
	}
}