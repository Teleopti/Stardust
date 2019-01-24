using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	[EnabledBy(Toggles.WFM_AnalyticsForecastUpdater_80798)]
	public class SkillDayChangedEventBuilder :
		IHandleEvents,
		IRunInSync
	{
		private readonly IEventPopulatingPublisher _eventPublisher;

		public SkillDayChangedEventBuilder(IEventPopulatingPublisher eventPublisher)
		{
			_eventPublisher = eventPublisher;
		}

		public void Subscribe(SubscriptionRegistrator registrator)
		{
			registrator.SubscribeTo<SkillDayChangedEvent>();
		}

		[UnitOfWork]
		public virtual void Handle(IEnumerable<IEvent> events)
		{
			var skillDayIds = events.Select(x => ((dynamic)x).SkillDayId as Guid?).ToArray();
	
			_eventPublisher.Publish(new ForecastChangedEvent
			{
				SkillDayIds = skillDayIds.Select(skillDay => skillDay.Value).ToArray()
			});
		}
	}
}