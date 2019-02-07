using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Common.EntityBaseTypes
{
	public abstract class AggregateRoot_Events : 
		AggregateRoot, 
		IAggregateRoot,
		IPublishEvents,
		ICloneEventsAfterMerge
	{
		private Events _events = new Events();

		public virtual void NotifyCommandId(Guid commandId)
		{
			_events.NotifyCommandId(commandId);
		}

		public virtual void NotifyDelete()
		{
		}

		public virtual void NotifyTransactionComplete(DomainUpdateType operation)
		{
		}

		protected void CloneEvents(AggregateRoot_Events clone)
		{
			if (clone == null)
				return;
			clone._events = _events.Clone();
		}

		void ICloneEventsAfterMerge.CloneEventsAfterMerge(AggregateRoot_Events clone)
		{
			CloneEvents(clone);
		}

		protected void AddEvent(Func<IEvent> @event)
		{
			_events.AddEvent(@event);
		}

		protected void AddEvent(Func<IPopEventsContext, IEvent> @event)
		{
			_events.AddEvent(@event);
		}
		
		protected void AddEvent(IEvent @event)
		{
			_events.AddEvent(@event);
		}

		public virtual IEnumerable<IEvent> PopAllEvents(IPopEventsContext context)
		{
			return _events.PopAllEvents(context);
		}

		public virtual bool HasEvents()
		{
			return _events.HasEvents();
		}

	}
}