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
	public class PersonChangedMessageSender : IMessageSender
	{
		private readonly IMessagePopulatingServiceBusSender _serviceBusSender;

		private readonly IEnumerable<Type> _triggerInterfaces = new List<Type>
		                                                        	{
		                                                        		typeof (IPerson),
		                                                        	};

		public PersonChangedMessageSender(IMessagePopulatingServiceBusSender serviceBusSender)
		{
			_serviceBusSender = serviceBusSender;
		}

		public void Execute(IEnumerable<IRootChangeInfo> modifiedRoots)
		{
			var affectedInterfaces = from r in modifiedRoots
									 let t = r.Root.GetType()
									 where _triggerInterfaces.Any(ti => ti.IsAssignableFrom(t))
									 select (IAggregateRoot)r.Root;

			foreach (var personList in affectedInterfaces.Batch(25))
			{
				var idsAsString = personList.Select(p => p.Id.GetValueOrDefault()).ToArray();
				var message = new PersonChangedMessage();
				message.SetPersonIdCollection(idsAsString);
				_serviceBusSender.Send(message, false);
			}
		}
	}
}