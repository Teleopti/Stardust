using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class DayOffTemplateChangedEventPublisher : ITransactionHook
	{
		private readonly IEventPopulatingPublisher _eventsPublisher;
		private readonly ICurrentBusinessUnit _businessUnit;
		private readonly INow _now;

		private readonly IEnumerable<Type> _triggerInterfaces = new List<Type>
		{
			typeof (IDayOffTemplate),
		};

		public DayOffTemplateChangedEventPublisher(IEventPopulatingPublisher eventsPublisher, ICurrentBusinessUnit businessUnit, INow now)
		{
			_eventsPublisher = eventsPublisher;
			_businessUnit = businessUnit;
			_now = now;
		}

		public void AfterCompletion(IEnumerable<IRootChangeInfo> modifiedRoots)
		{
			var bu = _businessUnit.Current();
			if (bu == null || !bu.Id.HasValue)
				return;

			var affectedInterfaces = from r in modifiedRoots
									 let t = r.Root.GetType()
									 where _triggerInterfaces.Any(ti => ti.IsAssignableFrom(t))
									 select (IAggregateRoot)r.Root;

			foreach (var affectedInterface in affectedInterfaces.Distinct())
			{
				if (!affectedInterface.Id.HasValue || affectedInterface.Id.GetValueOrDefault() == Guid.Empty || !(affectedInterface is IDayOffTemplate))
					continue;
				var dayOffTemplate = affectedInterface as IDayOffTemplate;
				var message = new DayOffTemplateChangedEvent
				{
					DayOffTemplateId = dayOffTemplate.Id.GetValueOrDefault(),
					DayOffName = dayOffTemplate.Description.Name,
					DayOffShortName = dayOffTemplate.Description.ShortName,
					DatasourceUpdateDate = dayOffTemplate.UpdatedOn ?? _now.UtcDateTime(),
					LogOnBusinessUnitId = bu.Id.GetValueOrDefault(Guid.Empty),
					Timestamp = _now.UtcDateTime()
				};
				_eventsPublisher.Publish(message);
			}
		}
	}
}