using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Common.EntityBaseTypes
{
	public class Events
	{
		private IList<Func<IPopEventsContext, IEvent>> _events = new List<Func<IPopEventsContext, IEvent>>();
		private Guid? _commandId;

		public void NotifyCommandId(Guid commandId)
		{
			_commandId = commandId;
		}

		public void AddEvent(Func<IEvent> @event)
		{
			_events.Add(x => @event.Invoke());
		}

		public void AddEvent(Func<IPopEventsContext, IEvent> @event)
		{
			_events.Add(@event);
		}

		public void AddEvent(IEvent @event)
		{
			_events.Add(x => @event);
		}

		public IEnumerable<IEvent> PopAllEvents(IPopEventsContext context)
		{
			context = context ?? new FromServiceLocators();
			var allEvents = _events.Select(e => e(context)).ToArray();
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

	public interface IPopEventsContext
	{
		INow Now { get; }
	}

	public class FromServiceLocators : IPopEventsContext
	{
		public INow Now => ServiceLocator_DONTUSE.Now;
	}

	public class Injected : IPopEventsContext
	{
		public INow Now { get; }

		public Injected(INow now)
		{
			Now = now;
		}
	}
}