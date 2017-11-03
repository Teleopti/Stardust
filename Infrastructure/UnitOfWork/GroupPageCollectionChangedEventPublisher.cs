using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class GroupPageCollectionChangedEventPublisher : ITransactionHook
	{
		private readonly IEventPopulatingPublisher _eventsPublisher;

		private readonly IEnumerable<Type> _triggerInterfaces = new List<Type>
		{
			typeof (IGroupPage)
		};

		public GroupPageCollectionChangedEventPublisher(IEventPopulatingPublisher eventsPublisher)
		{
			_eventsPublisher = eventsPublisher;
		}

		public void AfterCompletion(IEnumerable<IRootChangeInfo> modifiedRoots)
		{
			var allRoots = modifiedRoots.ToList();

			var affectedInterfaces = from r in allRoots
				from i in r.Root.GetType().GetInterfaces()
				select i;
			if (!affectedInterfaces.Any(t => _triggerInterfaces.Contains(t))) return;

			//get the group page ids
			var groupPage = allRoots.Select(r => r.Root).OfType<IGroupPage>();
			foreach (var groupPageList in groupPage.Batch(25))
			{
				var idsAsString = (from p in groupPageList select p.Id.GetValueOrDefault()).ToArray();
				var message = new GroupPageCollectionChangedEvent();
				message.SetGroupPageIdCollection(idsAsString);
				_eventsPublisher.Publish(message);
			}
		}
		
	}
}