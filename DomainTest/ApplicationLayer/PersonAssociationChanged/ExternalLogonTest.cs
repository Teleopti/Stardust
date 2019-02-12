using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.PersonAssociationChanged
{
	[DomainTest]
	[AddDatasourceId]
	public class ExternalLogonTest
	{
		public PersonAssociationChangedEventPublisher Target;
		public FakeEventPublisher Publisher;
		public MutableNow Now;
		public FakeDatabase Data;

		[Test]
		public void ShouldPublishWhenExternalLogonsChange()
		{
			Now.Is("2016-08-26 00:00");
			var person = Guid.NewGuid();
			var user1 = RandomName.Make("user");
			var user2 = RandomName.Make("user");
			Data
				.WithAgent(person, "pierre")
				.WithPeriod("2016-08-26")
				.WithExternalLogon(user1)
				.WithExternalLogon(user2);

			Target.Handle(new TenantHourTickEvent());

			var @event = Publisher.PublishedEvents.OfType<PersonAssociationChangedEvent>().Single(x => x.PersonId == person);
			@event.ExternalLogons.Select(x => x.UserCode).Should().Contain(user1);
			@event.ExternalLogons.Select(x => x.UserCode).Should().Contain(user2);
		}

		[Test]
		public void ShouldPublishWithDataSourceId()
		{
			Now.Is("2016-08-26 00:00");
			var person = Guid.NewGuid();
			Data
				.WithAgent(person, "pierre")
				.WithPeriod("2016-08-26")
				.WithDataSource(123, "source2")
				.WithExternalLogon("usercode");

			Target.Handle(new TenantHourTickEvent());

			var @event = Publisher.PublishedEvents.OfType<PersonAssociationChangedEvent>().Single(x => x.PersonId == person);
			@event.ExternalLogons.Single().DataSourceId.Should().Be(123);
		}

		[Test]
		public void ShouldNotPublishWhenExternalLogonsDoesNotChange()
		{
			Data
				.WithAgent("pierre")
				.WithPeriod("2016-08-25")
				.WithExternalLogon("user1")
				.WithExternalLogon("user2")
				.WithPeriod("2016-08-26")
				.WithExternalLogon("user2")
				.WithExternalLogon("user1");
			Now.Is("2016-08-26 00:00");
			Target.Handle(new TenantHourTickEvent());
			Publisher.Clear();

			Target.Handle(new TenantHourTickEvent());

			Publisher.PublishedEvents.Should().Be.Empty();
		}

		[Test]
		public void ShouldPublishWhenTerminalDateChanged()
		{
			var person = Guid.NewGuid();
			Data
				.WithAgent(person, "pierre", "2016-08-25")
				.WithDataSource(7, "7")
				.WithExternalLogon("usercode");
			Now.Is("2016-08-25 00:00");

			Target.Handle(new PersonTerminalDateChangedEvent
			{
				PersonId = person,
				PreviousTerminationDate = "2016-08-30".Utc(),
				TerminationDate = "2016-08-25".Utc()
			});

			var @event = Publisher.PublishedEvents.OfType<PersonAssociationChangedEvent>().Single();
			@event.ExternalLogons.Last().DataSourceId.Should().Be(7);
			@event.ExternalLogons.Last().UserCode.Should().Be("usercode");
		}

		[Test]
		public void ShouldPublishWhenTeamChanged()
		{
			var person = Guid.NewGuid();
			Data
				.WithAgent(person, "pierre")
				.WithDataSource(7, "7")
				.WithExternalLogon("usercode");
			Now.Is("2016-08-26 00:00");

			Target.Handle(new PersonTeamChangedEvent
			{
				PersonId = person,
				ExternalLogons = new[] {new ExternalLogon {DataSourceId = 7, UserCode = "usercode"}}
			});

			var @event = Publisher.PublishedEvents.OfType<PersonAssociationChangedEvent>().Single();
			@event.ExternalLogons.Last().DataSourceId.Should().Be(7);
			@event.ExternalLogons.Last().UserCode.Should().Be("usercode");
		}


		[Test]
		public void ShouldPublishWhenPersonPeriodChanged()
		{
			var person = Guid.NewGuid();
			Data
				.WithAgent(person, "pierre")
				.WithDataSource(7, "7")
				.WithExternalLogon("usercode");
			Now.Is("2016-08-26 00:00");

			Target.Handle(new PersonPeriodChangedEvent
			{
				PersonId = person,
				ExternalLogons = new[] {new ExternalLogon {DataSourceId = 7, UserCode = "usercode"}}
			});

			var @event = Publisher.PublishedEvents.OfType<PersonAssociationChangedEvent>().Single();
			@event.ExternalLogons.Last().DataSourceId.Should().Be(7);
			@event.ExternalLogons.Last().UserCode.Should().Be("usercode");
		}
	}
}