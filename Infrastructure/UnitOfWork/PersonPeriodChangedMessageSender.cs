﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
    public class PersonPeriodChangedMessageSender :IMessageSender
    {
		private readonly IServiceBusEventPopulatingPublisher _serviceBusSender;

	    private readonly IEnumerable<Type> _triggerInterfaces = new List<Type>
		                                                        	{
		                                                        		typeof (ITeam),
		                                                        		typeof (ISite),
		                                                        		typeof (IContract),
		                                                        		typeof (IContractSchedule),
		                                                        		typeof (IPartTimePercentage),
		                                                        		typeof (IRuleSetBag),
		                                                        		typeof (ISkill),
		                                                        		typeof (IPerson)
		                                                        	};

		public PersonPeriodChangedMessageSender(IServiceBusEventPopulatingPublisher serviceBusSender)
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
				var notPerson = (from p in modifiedRoots where !(p.Root is IPerson) select p.Root).ToList();
				foreach (var notpersonList in notPerson.Batch(25))
				{
					var idsAsString = (from p in notpersonList select ((IAggregateRoot)p).Id.GetValueOrDefault()).ToArray();
                    
                    var message = new PersonPeriodChangedMessage();
					message.SetPersonIdCollection(idsAsString);
                    _serviceBusSender.Publish(message);
				}
            }
        }
    }
}