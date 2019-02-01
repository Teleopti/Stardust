using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Common.EntityBaseTypes
{
	public abstract class AggregateRoot : Entity,
		IAggregateRoot,
		IChangeInfo,
		IEventsRoot
	{
		private IPerson _updatedBy;
		private DateTime? _updatedOn;
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

		protected void CloneEvents(AggregateRoot clone)
		{
			clone._events = _events.Clone();
		}

		void IEventsRoot.CloneEventsAfterMerge(AggregateRoot clone)
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
			return _events.PopAllEvents(context ?? new FromServiceLocators());
		}

		public virtual bool HasEvents()
		{
			return _events.HasEvents();
		}

		public virtual IPerson UpdatedBy => _updatedBy;

		public virtual DateTime? UpdatedOn
		{
			get => _updatedOn;
			set => _updatedOn = value;
		}

		public override void ClearId()
		{
			base.ClearId();
			_updatedBy = null;
			_updatedOn = null;
		}
	}
}