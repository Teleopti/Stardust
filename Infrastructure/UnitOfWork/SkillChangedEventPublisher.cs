using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class SkillChangedEventPublisher : ITransactionHook
	{
		private readonly IEventPopulatingPublisher _eventsPublisher;
		private readonly ICurrentBusinessUnit _businessUnit;

		private readonly IEnumerable<Type> _triggerInterfaces = new List<Type>
		{
			typeof (ISkill),
		};

		public SkillChangedEventPublisher(IEventPopulatingPublisher eventsPublisher, ICurrentBusinessUnit businessUnit)
		{
			_eventsPublisher = eventsPublisher;
			_businessUnit = businessUnit;
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
			var message = new SkillChangedEvent {SkillId = id};
			_eventsPublisher.Publish(message);
		}
	}
}