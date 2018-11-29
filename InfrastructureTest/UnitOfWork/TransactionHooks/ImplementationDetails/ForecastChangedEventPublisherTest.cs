using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork.TransactionHooks.ImplementationDetails
{
	[TestFixture]
	public class ForecastChangedEventPublisherTest
	{
		[Test]
		public void ShouldSetSkillDayIdOnMessageWhenForecastedDayChanged()
		{
			var skillDay = SkillDayFactory.CreateSkillDay(DateOnly.Today).WithId();
			IRootChangeInfo rootChangeInfo = new RootChangeInfo(skillDay, DomainUpdateType.Update);
			var publisher = new eventPopulatingPublisherProbe();
			var target = new ForecastChangedEventPublisher(publisher);
			target.AfterCompletion(new[] { rootChangeInfo });

			publisher.PublishedEvent.SkillDayIds.Single().Should().Be(skillDay.Id);
		}

		[Test]
		public void ShouldSetSkillDayIdsOnMessageWhenMultipleForecastedDaysChanged()
		{
			var skillDay1 = SkillDayFactory.CreateSkillDay(DateOnly.Today).WithId();
			var skillDay2 = SkillDayFactory.CreateSkillDay(DateOnly.Today).WithId();
			IRootChangeInfo rootChangeInfo1 = new RootChangeInfo(skillDay1, DomainUpdateType.Update);
			IRootChangeInfo rootChangeInfo2 = new RootChangeInfo(skillDay2, DomainUpdateType.Update);
			var publisher = new eventPopulatingPublisherProbe();
			var target = new ForecastChangedEventPublisher(publisher);
			target.AfterCompletion(new[] { rootChangeInfo1, rootChangeInfo2 });

			publisher.PublishedEvent.SkillDayIds.First().Should().Be(skillDay1.Id);
			publisher.PublishedEvent.SkillDayIds.Last().Should().Be(skillDay2.Id);
		}

		[Test]
		public void ShouldNotPublishEventIfOtherRootThanSkillDayIsChanged()
		{
			var incorrectRootTypeChanged = PersonFactory.CreatePersonWithId();
			IRootChangeInfo rootChangeInfo = new RootChangeInfo(incorrectRootTypeChanged, DomainUpdateType.Update);
			var publisher = new eventPopulatingPublisherProbe();
			var target = new ForecastChangedEventPublisher(publisher);
			target.AfterCompletion(new[] { rootChangeInfo});

			publisher.PublishedEvent.Should().Be.Null();
		}

		private class eventPopulatingPublisherProbe : IEventPopulatingPublisher
		{
			public ForecastChangedEvent PublishedEvent;

			public void Publish(params IEvent[] events)
			{
				PublishedEvent = (ForecastChangedEvent)events.Single();
			}
		}
	}
}