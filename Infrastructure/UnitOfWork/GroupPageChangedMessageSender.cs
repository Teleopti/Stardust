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
	public class GroupPageChangedMessageSender : IMessageSender
	{
		private readonly IServiceBusSender _serviceBusSender;

		private readonly IEnumerable<Type> _triggerInterfaces = new List<Type>
		                                                        	{
		                                                        		typeof (IGroupPage)
		                                                        	};

		public GroupPageChangedMessageSender(IServiceBusSender serviceBusSender)
		{
			_serviceBusSender = serviceBusSender;
		}

		public void Execute(IEnumerable<IRootChangeInfo> modifiedRoots)
		{
			if (!_serviceBusSender.EnsureBus()) return;

			var affectedInterfaces = from r in modifiedRoots
			                         from i in r.Root.GetType().GetInterfaces()
			                         select i;
	        if (affectedInterfaces.Any(t => _triggerInterfaces.Contains(t)))
			{
                //get the group page ids
				var groupPage = modifiedRoots.Select(r => r.Root).OfType<IGroupPage>();
				foreach (var groupPageList in groupPage.Batch(25))
				{
					var idsAsString = (from p in groupPageList select p.Id.GetValueOrDefault()).ToArray();
					var message = new GroupPageChangedMessage();
					message.SetGroupPageIdCollection(idsAsString);
					_serviceBusSender.Send(message);
				}
			}
		}
	}
}