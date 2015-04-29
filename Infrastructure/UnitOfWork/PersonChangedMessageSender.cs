using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class PersonChangedMessageSender : IMessageSender
	{
		private readonly IEventPopulatingPublisher _eventsPublisher;
		private readonly ICurrentBusinessUnit _businessUnit;

		private readonly IEnumerable<Type> _triggerInterfaces = new List<Type>
		                                                        	{
		                                                        		typeof (IPerson),
		                                                        	};

		public PersonChangedMessageSender(IEventPopulatingPublisher eventsPublisher, ICurrentBusinessUnit businessUnit)
		{
			_eventsPublisher = eventsPublisher;
			_businessUnit = businessUnit;
		}

		public void Execute(IEnumerable<IRootChangeInfo> modifiedRoots)
		{
			// we are signed in with a transient BU and cant publish events
			var bu = _businessUnit.Current();
			if (bu == null || !bu.Id.HasValue)
				return;

			var affectedInterfaces = from r in modifiedRoots
									 let t = r.Root.GetType()
									 where _triggerInterfaces.Any(ti => ti.IsAssignableFrom(t))
									 select (IAggregateRoot)r.Root;

			foreach (var personList in affectedInterfaces.Batch(25))
			{
				var idsAsString = personList.Select(p => p.Id.GetValueOrDefault()).ToArray();
				var message = new PersonCollectionChangedEvent();
				message.SetPersonIdCollection(idsAsString);
				_eventsPublisher.Publish(message);
			}
		}
	}
}