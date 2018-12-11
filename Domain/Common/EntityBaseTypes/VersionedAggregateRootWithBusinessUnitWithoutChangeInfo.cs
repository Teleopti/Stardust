using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Common.EntityBaseTypes
{
	public abstract class VersionedAggregateRootWithBusinessUnitWithoutChangeInfo : Entity,
		IVersioned,
		IAggregateRoot,
		IBelongsToBusinessUnit
	{
		private IBusinessUnit _businessUnit;

		private readonly Events _events = new Events();

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

		protected void AddEvent(IEvent @event)
		{
			_events.AddEvent(@event);
		}

		public virtual IEnumerable<IEvent> PopAllEvents()
		{
			return _events.PopAllEvents();
		}

		public virtual bool HasEvents()
		{
			return _events.HasEvents();
		}

		public virtual IBusinessUnit BusinessUnit
		{
			get => _businessUnit ?? (_businessUnit = ServiceLocatorForEntity.CurrentBusinessUnit.Current());
			protected set => _businessUnit = value;
		}

		private int? _version;

		public virtual int? Version => _version;

		public virtual void SetVersion(int version)
		{
			_version = version;
		}
	}
}