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

			foreach (var affectedInterface in affectedInterfaces)
			{
				if (!affectedInterface.Id.HasValue || affectedInterface.Id.GetValueOrDefault() == Guid.Empty || !(affectedInterface is IPreferenceDay))
					continue;
				var preferenceDay = affectedInterface as IPreferenceDay;
				var message = new PreferenceChangedEvent
				{
					PreferenceDayId = preferenceDay.Id.GetValueOrDefault(),
					PersonId = preferenceDay.Person.Id.GetValueOrDefault(),
					RestrictionDate = preferenceDay.RestrictionDate.Date,
					LogOnBusinessUnitId = bu.Id.GetValueOrDefault(Guid.Empty),
					Timestamp = _now.UtcDateTime()
				};
				_eventsPublisher.Publish(message);
			}
		}
	}
}