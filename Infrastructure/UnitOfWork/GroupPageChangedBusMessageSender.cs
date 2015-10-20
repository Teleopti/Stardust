using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class GroupPageChangedBusMessageSender : IPersistCallback
	{
		private readonly IMessagePopulatingServiceBusSender _serviceBusSender;

		private readonly IEnumerable<Type> _triggerInterfaces = new List<Type>
		{
			typeof (IGroupPage)
		};

		public GroupPageChangedBusMessageSender(IMessagePopulatingServiceBusSender serviceBusSender)
		{
			_serviceBusSender = serviceBusSender;
		}

		public void AfterFlush(IEnumerable<IRootChangeInfo> modifiedRoots)
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
				var message = new GroupPageChangedMessage();
				message.SetGroupPageIdCollection(idsAsString);
				_serviceBusSender.Send(message, false);
			}
		}
	}
}