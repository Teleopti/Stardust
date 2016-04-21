using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class PreferenceChangedEventPublisher : ITransactionHook
	{
		private readonly IEventPopulatingPublisher _eventsPublisher;
		private readonly ICurrentBusinessUnit _businessUnit;
		private readonly INow _now;

		private readonly IEnumerable<Type> _triggerInterfaces = new List<Type>
		{
			typeof (IPreferenceDay),
		};

		public PreferenceChangedEventPublisher(IEventPopulatingPublisher eventsPublisher, ICurrentBusinessUnit businessUnit, INow now)
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

			var id = affectedInterfaces.Select(p => p.Id.GetValueOrDefault()).SingleOrDefault();
			if (id == Guid.Empty)
				return;
			var message = new PreferenceChangedEvent
			{
				LogOnBusinessUnitId = bu.Id.GetValueOrDefault(Guid.Empty),
				PreferenceDayId = id,
				Timestamp = _now.UtcDateTime()
			};
			_eventsPublisher.Publish(message);
		}
	}
}