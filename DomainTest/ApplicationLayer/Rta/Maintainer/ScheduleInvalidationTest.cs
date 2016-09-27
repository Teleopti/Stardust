using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using ExternalLogon = Teleopti.Ccc.Domain.ApplicationLayer.Events.ExternalLogon;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Maintainer
{
	[TestFixture]
	[RtaTest]
	public class ScheduleInvalidationTest
	{
		public FakeRtaDatabase Database;
		public AgentStateMaintainer Target;
		public MutableNow Now;
		public FakeAgentStatePersister Persister;

		[Test]
		public void ShouldPrepare()
		{
			var person = Guid.NewGuid();

			Target.Handle(new PersonAssociationChangedEvent
			{
				PersonId = person,
				TeamId = Guid.NewGuid(),
				ExternalLogons = new[] { new ExternalLogon() }
			});

			Persister.Get(person).Should().Not.Be.Null();
		}

		[Test]
		public void ShouldInvalidate()
		{
			Now.Is("2016-09-27 15:00");
			var person = Guid.NewGuid();
			Persister.Has(new AgentStateFound
			{
				PersonId = person,
				Schedule = new[] { new ScheduledActivity() }
			});

			Target.Handle(new ScheduleChangedEvent
			{
				PersonId = person,
				StartDateTime = "2016-09-27 08:00".Utc(),
				EndDateTime = "2016-09-27 17:00".Utc()
			});

			Persister.Get(person).Schedule.Should().Be.Null();
		}

		[Test]
		public void ShouldInvalidateOnChangesPastToToday()
		{
			Now.Is("2016-09-27 15:00");
			var person = Guid.NewGuid();
			Persister.Has(new AgentStateFound
			{
				PersonId = person,
				Schedule = new[] { new ScheduledActivity() }
			});

			Target.Handle(new ScheduleChangedEvent
			{
				PersonId = person,
				StartDateTime = "2016-09-01 08:00".Utc(),
				EndDateTime = "2016-09-27 17:00".Utc()
			});

			Persister.Get(person).Schedule.Should().Be.Null();
		}

		[Test]
		public void ShouldInvalidateOnChangesTodayToFuture()
		{
			Now.Is("2016-09-27 15:00");
			var person = Guid.NewGuid();
			Persister.Has(new AgentStateFound
			{
				PersonId = person,
				Schedule = new[] { new ScheduledActivity() }
			});

			Target.Handle(new ScheduleChangedEvent
			{
				PersonId = person,
				StartDateTime = "2016-09-27 08:00".Utc(),
				EndDateTime = "2016-10-31 17:00".Utc()
			});

			Persister.Get(person).Schedule.Should().Be.Null();
		}

		[Test]
		public void ShouldInvalidateOnChangesPastToFuture()
		{
			Now.Is("2016-09-27 15:00");
			var person = Guid.NewGuid();
			Persister.Has(new AgentStateFound
			{
				PersonId = person,
				Schedule = new[] { new ScheduledActivity() }
			});

			Target.Handle(new ScheduleChangedEvent
			{
				PersonId = person,
				StartDateTime = "2016-09-01 08:00".Utc(),
				EndDateTime = "2016-10-31 17:00".Utc()
			});

			Persister.Get(person).Schedule.Should().Be.Null();
		}

		[Test]
		public void ShouldNotInvalidateOnChangesPast()
		{
			Now.Is("2016-09-27 15:00");
			var person = Guid.NewGuid();
			Persister.Has(new AgentStateFound
			{
				PersonId = person,
				Schedule = new[] { new ScheduledActivity() }
			});

			Target.Handle(new ScheduleChangedEvent
			{
				PersonId = person,
				StartDateTime = "2016-09-01 08:00".Utc(),
				EndDateTime = "2016-09-24 17:00".Utc()
			});

			Persister.Get(person).Schedule.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldNotInvalidateOnChangesInTheFuture()
		{
			Now.Is("2016-09-27 15:00");
			var person = Guid.NewGuid();
			Persister.Has(new AgentStateFound
			{
				PersonId = person,
				Schedule = new[] { new ScheduledActivity() }
			});

			Target.Handle(new ScheduleChangedEvent
			{
				PersonId = person,
				StartDateTime = "2016-10-02 08:00".Utc(),
				EndDateTime = "2016-10-31 17:00".Utc()
			});

			Persister.Get(person).Schedule.Should().Not.Be.Null();
		}

	}
}