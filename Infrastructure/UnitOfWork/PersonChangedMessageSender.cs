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
    public class PersonChangedMessageSender :IMessageSender
    {
		private readonly IServiceBusEventPublisher _serviceBusSender;

	    private readonly IEnumerable<Type> _triggerInterfaces = new List<Type>
		                                                        	{
		                                                        		typeof (IPerson),
		                                                        	};

		public PersonChangedMessageSender(IServiceBusEventPublisher serviceBusSender)
		{
			_serviceBusSender = serviceBusSender;
		}

	    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider"),
	     System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Execute(IEnumerable<IRootChangeInfo> modifiedRoots)
		{
			if (!_serviceBusSender.EnsureBus()) return;

		    var affectedInterfaces = from r in modifiedRoots
		                             let t = r.Root.GetType()
		                             where _triggerInterfaces.Any(ti => ti.IsAssignableFrom(t))
		                             select (IAggregateRoot) r.Root;

		    foreach (var personList in affectedInterfaces.Batch(25))
		    {
			    var idsAsString = personList.Select(p => p.Id.GetValueOrDefault()).ToArray();
			    var message = new PersonChangedMessage();
			    message.SetPersonIdCollection(idsAsString);
                    _serviceBusSender.Publish(message);
		    }
	    }
    }
}