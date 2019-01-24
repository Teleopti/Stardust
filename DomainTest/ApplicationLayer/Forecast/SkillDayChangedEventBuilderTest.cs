using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Forecast
{
	[DomainTest]
	public class SkillDayChangedEventBuilderTest
	{
		public SkillDayChangedEventBuilder Target;
		public FakeEventPublisher EventPublisher;

		[Test]
		public void ShouldPublishBundledSkillDayChangedEvents()
		{
			var skillDayId1 = Guid.NewGuid();
			var skillDayId2 = Guid.NewGuid();
			var events = new List<IEvent>(){ new SkillDayChangedEvent() { SkillDayId = skillDayId1 } , new SkillDayChangedEvent() { SkillDayId = skillDayId2 } };
			Target.Handle(events);
			var eventPublished = EventPublisher.PublishedEvents.Single(x => x.GetType() == typeof(ForecastChangedEvent)) as ForecastChangedEvent;
			eventPublished.SkillDayIds.First().Should().Be.EqualTo(skillDayId1);
			eventPublished.SkillDayIds.Last().Should().Be.EqualTo(skillDayId2);
		}
	}
}
