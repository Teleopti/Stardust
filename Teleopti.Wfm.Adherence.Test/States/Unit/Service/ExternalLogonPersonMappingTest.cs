using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Wfm.Adherence.Domain.Events;
using Teleopti.Wfm.Adherence.Domain.Service;

namespace Teleopti.Wfm.Adherence.Test.States.Unit.Service
{
	[TestFixture]
	[RtaTest]
	public class ExternalLogonPersonMappingTest
	{
		public FakeDatabase Database;
		public Rta Target;
		public FakeEventPublisher Publisher;
		public MutableNow Now;

		[Test]
		public void ShouldProcess2PersonsWithSameExternalLogon()
		{
			var person1 = Guid.NewGuid();
			var person2 = Guid.NewGuid();
			Database
				.WithAgent("usercode", person1)
				.WithAgent("usercode", person2);

			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});

			Publisher.PublishedEvents.OfType<PersonNeutralAdherenceEvent>().Where(x => x.PersonId == person1).Should().Have.Count.EqualTo(1);
			Publisher.PublishedEvents.OfType<PersonNeutralAdherenceEvent>().Where(x => x.PersonId == person2).Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldProcessSamePersonWith2ExternalLogon()
		{
			var phone = Guid.NewGuid();
			var person = Guid.NewGuid();
			Database
				.WithAgent("usercode1", person)
				.WithAgent("usercode2", person)
				.WithMappedRule("phone", phone, 0, Adherence.Configuration.Adherence.In)
				.WithMappedRule("loggedOut", phone, -1, Adherence.Configuration.Adherence.Out)
				.WithSchedule(person, phone, "2016-08-29 10:00", "2016-08-29 11:00")
				;

			Now.Is("2016-08-29 09:55");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode1",
				StateCode = "loggedOut"
			});
			Now.Is("2016-08-29 10:05");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode2",
				StateCode = "phone"
			});
			Now.Is("2016-08-29 10:30");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode1",
				StateCode = "loggedOut"
			});

			Publisher.PublishedEvents.OfType<PersonInAdherenceEvent>()
				.Where(x => x.PersonId == person && x.Timestamp == "2016-08-29 10:05".Utc())
				.Should().Have.Count.EqualTo(1);
			Publisher.PublishedEvents.OfType<PersonOutOfAdherenceEvent>()
				.Where(x => x.PersonId == person && x.Timestamp == "2016-08-29 10:30".Utc())
				.Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldProcessOverlappingExternalLogons()
		{
			var phone = Guid.NewGuid();
			var person1 = Guid.NewGuid();
			var person2 = Guid.NewGuid();
			Database
				.WithAgent("usercode1", person1)
				.WithAgent("usercode2", person1)
				.WithAgent("usercode2", person2)
				.WithAgent("usercode3", person2)
				.WithMappedRule("phone", phone, 0, Adherence.Configuration.Adherence.In)
				.WithMappedRule("loggedOut", phone, -1, Adherence.Configuration.Adherence.Out)
				.WithSchedule(person1, phone, "2016-08-29 10:00", "2016-08-29 11:00")
				.WithSchedule(person2, phone, "2016-08-29 10:00", "2016-08-29 11:00")
				;

			Now.Is("2016-08-29 09:55");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode1",
				StateCode = "loggedOut"
			});
			Now.Is("2016-08-29 10:05");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode2",
				StateCode = "phone"
			});
			Now.Is("2016-08-29 10:45");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode3",
				StateCode = "loggedOut"
			});

			Publisher.PublishedEvents.OfType<PersonInAdherenceEvent>()
				.Where(x => x.PersonId == person1 && x.Timestamp == "2016-08-29 10:05".Utc())
				.Should().Have.Count.EqualTo(1);
			Publisher.PublishedEvents.OfType<PersonOutOfAdherenceEvent>()
				.Where(x => x.PersonId == person1 && x.Timestamp == "2016-08-29 10:45".Utc())
				.Should().Have.Count.EqualTo(0);
			Publisher.PublishedEvents.OfType<PersonOutOfAdherenceEvent>()
				.Where(x => x.PersonId == person2 && x.Timestamp == "2016-08-29 10:45".Utc())
				.Should().Have.Count.EqualTo(1);
		}
	}
}