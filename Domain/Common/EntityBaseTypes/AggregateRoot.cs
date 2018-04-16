﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
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

		protected void AddEvent(Func<IEvent> @event)
		{
			_events.AddEvent(@event);
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

	public abstract class VersionedAggregateRootWithBusinessUnit : VersionedAggregateRoot,
		IBelongsToBusinessUnit
	{
		private IBusinessUnit _businessUnit;

		public virtual IBusinessUnit BusinessUnit
		{
			get => _businessUnit ?? (_businessUnit = ServiceLocatorForEntity.CurrentBusinessUnit.Current());
			protected set => _businessUnit = value;
		}
	}

	public abstract class NonversionedAggregateRootWithBusinessUnit : AggregateRoot,
		IBelongsToBusinessUnit
	{
		private IBusinessUnit _businessUnit;

		public virtual IBusinessUnit BusinessUnit
		{
			get => _businessUnit ?? (_businessUnit = ServiceLocatorForEntity.CurrentBusinessUnit.Current());
			protected set => _businessUnit = value;
		}
	}

	public abstract class VersionedAggregateRoot : AggregateRoot,
		IVersioned
	{
		private int? _version;

		public virtual int? Version => _version;

		public virtual void SetVersion(int version)
		{
			_version = version;
		}
	}

	public abstract class AggregateRoot : Entity,
		IAggregateRoot,
		IChangeInfo,
		IEventsRoot
	{
		private IPerson _updatedBy;
		private DateTime? _updatedOn;
		private static readonly LocalizedUpdateInfo _localizedUpdateInfo = new LocalizedUpdateInfo();

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

	public interface IEventsRoot
	{
		void CloneEventsAfterMerge(AggregateRoot clone);
	}

	public class Events
	{
		private IList<Func<IEvent>> _events = new List<Func<IEvent>>();
		private Guid? _commandId;

		public void NotifyCommandId(Guid commandId)
		{
			_commandId = commandId;
		}

		public void AddEvent(Func<IEvent> @event)
		{
			_events.Add(@event);
		}

		public void AddEvent(IEvent @event)
		{
			_events.Add(() => @event);
		}

		public IEnumerable<IEvent> PopAllEvents()
		{
			var allEvents = _events.Select(e => e()).ToArray();
			_events.Clear();
			if (_commandId.HasValue)
				allEvents.ForEach(e =>
				{
					if (e is ICommandIdentifier trackableE)
					{
						trackableE.CommandId = _commandId.Value;
					}
				});
			return allEvents;
		}

		public Events Clone()
		{
			return new Events
			{
				_events = _events.ToList()
			};
		}

		public bool HasEvents()
		{
			return _events.Any();
		}
	}

	public static class EventsExtensions
	{
		public static IEnumerable<IEvent> KeepLastOfType<T>(this IEnumerable<IEvent> events) where T : IEvent
		{
			return events.Aggregate(new List<IEvent>(), (result, e) =>
			{
				if (result.OfType<T>().Any())
					result.Remove(result.OfType<T>().Single());
				result.Add(e);
				return result;
			});
		}
	}
}