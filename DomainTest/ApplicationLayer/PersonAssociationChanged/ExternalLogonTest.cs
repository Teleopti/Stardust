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
	[TestFixture]
	[DomainTest]
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
			var user1 = RandomName.Make("user");
			var user2 = RandomName.Make("user");
			Data
				.WithAgent("pierre")
				.WithPeriod("2016-08-26")
				.WithExternalLogon(user1)
				.WithExternalLogon(user2);

			Target.Handle(new TenantHourTickEvent());

			var @event = Publisher.PublishedEvents.OfType<PersonAssociationChangedEvent>().Single();
			@event.ExternalLogons.Select(x => x.UserCode).Should().Contain(user1);
			@event.ExternalLogons.Select(x => x.UserCode).Should().Contain(user2);
		}

		[Test]
		public void ShouldPublishWithDataSourceId()
		{
			Now.Is("2016-08-26 00:00");
			Data
				.WithAgent("pierre")
				.WithPeriod("2016-08-26")
				.WithDataSource(123, "source2")
				.WithExternalLogon("usercode");

			Target.Handle(new TenantHourTickEvent());

			Publisher.PublishedEvents.OfType<PersonAssociationChangedEvent>().Single()
				.ExternalLogons.Single().DataSourceId
				.Should().Be(123);
		}

		[Test]
		public void ShouldNotPublishWhenExternalLogonsDoesNotChange()
		{
			Now.Is("2016-08-26 00:00");
			Data
				.WithAgent("pierre")
				.WithPeriod("2016-08-25")
				.WithExternalLogon("user1")
				.WithExternalLogon("user2")
				.WithPeriod("2016-08-26")
				.WithExternalLogon("user2")
				.WithExternalLogon("user1");

			Target.Handle(new TenantHourTickEvent());

			Publisher.PublishedEvents.Should().Be.Empty();
		}

		[Test]
		public void ShouldPublishWhenTerminalDateChanged()
		{
			Now.Is("2016-08-26 00:00");

			Target.Handle(new PersonTerminalDateChangedEvent
			{
				PreviousTerminationDate = "2016-08-30".Utc(),
				TerminationDate = "2016-08-25".Utc(),
				ExternalLogons = new[] {new ExternalLogon {DataSourceId = 7, UserCode = "usercode"}, }
			});

			var @event = Publisher.PublishedEvents.OfType<PersonAssociationChangedEvent>().Single();
			@event.ExternalLogons.Single().DataSourceId.Should().Be(7);
			@event.ExternalLogons.Single().UserCode.Should().Be("usercode");
		}

		[Test]
		public void ShouldPublishWhenTeamChanged()
		{
			Now.Is("2016-08-26 00:00");

			Target.Handle(new PersonTeamChangedEvent
			{
				ExternalLogons = new[] { new ExternalLogon { DataSourceId = 7, UserCode = "usercode" }, }
			});

			var @event = Publisher.PublishedEvents.OfType<PersonAssociationChangedEvent>().Single();
			@event.ExternalLogons.Single().DataSourceId.Should().Be(7);
			@event.ExternalLogons.Single().UserCode.Should().Be("usercode");
		}


		[Test]
		public void ShouldPublishWhenPersonPeriodChanged()
		{
			Now.Is("2016-08-26 00:00");

			Target.Handle(new PersonPeriodChangedEvent
			{
				ExternalLogons = new[] { new ExternalLogon { DataSourceId = 7, UserCode = "usercode" }, }
			});

			var @event = Publisher.PublishedEvents.OfType<PersonAssociationChangedEvent>().Single();
			@event.ExternalLogons.Single().DataSourceId.Should().Be(7);
			@event.ExternalLogons.Single().UserCode.Should().Be("usercode");
		}
	}
}