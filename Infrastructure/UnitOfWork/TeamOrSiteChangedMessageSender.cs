using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class TeamOrSiteChangedMessageSender : IMessageSender
	{
		private readonly IMessagePopulatingServiceBusSender _serviceBusSender;

		private readonly IEnumerable<Type> _otherTriggerInterfaces = new List<Type>
			{
				typeof (ITeam),
				typeof (ISite),
			};

		public TeamOrSiteChangedMessageSender(IMessagePopulatingServiceBusSender serviceBusSender)
		{
			_serviceBusSender = serviceBusSender;
		}

		public void Execute(IEnumerable<IRootChangeInfo> modifiedRoots)
		{
			var affectedInterfaces = from r in modifiedRoots
			                         let t = r.Root.GetType()
			                         where _otherTriggerInterfaces.Any(ti => ti.IsAssignableFrom(t))
			                         select r.Root;

			if (!affectedInterfaces.Any()) return;

			var message = new PersonChangedMessage{SerializedPeople = Guid.Empty.ToString()};
			_serviceBusSender.Send(message, false);
		}
	}
}