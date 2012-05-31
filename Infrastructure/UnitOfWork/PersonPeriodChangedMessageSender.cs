
using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
    public class PersonPeriodChangedMessageSender :IMessageSender
    {
        private readonly IEnumerable<Type> _triggerInterfaces = new List<Type>
		                                                        	{
		                                                        		typeof (ITeam),
		                                                        		typeof (ISite),
		                                                        		typeof (IContract),
		                                                        		typeof (IContractSchedule),
		                                                        		typeof (IPartTimePercentage),
		                                                        		typeof (IRuleSetBag),
		                                                        		typeof (ISkill)
		                                                        	};

    	private readonly ISendDenormalizeNotification _sendDenormalizeNotification;
    	private readonly ISaveToDenormalizationQueue _saveToDenormalizationQueue;

        public PersonPeriodChangedMessageSender(ISendDenormalizeNotification sendDenormalizeNotification, ISaveToDenormalizationQueue saveToDenormalizationQueue)
		{
			_sendDenormalizeNotification = sendDenormalizeNotification;
			_saveToDenormalizationQueue = saveToDenormalizationQueue;
		}

    	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public void Execute(IRunSql runSql, IEnumerable<IRootChangeInfo> modifiedRoots)
        {
			var atLeastOneMessage = false;
            var affectedInterfaces = from r in modifiedRoots
                                     from i in r.Root.GetType().GetInterfaces()
                                     select i;

            if (affectedInterfaces.Any(t => _triggerInterfaces.Contains(t)))
            {
				var notPerson = (from p in modifiedRoots where !(p.Root is Person) select p.Root).ToList();
				foreach (var notpersonList in notPerson.Batch(400))
				{
					var idsAsString = (from p in notpersonList select ((IAggregateRoot)p).Id).ToArray();
                    Guid[] ids = idsAsString.Select(g => g ?? Guid.Empty).ToArray();
                    
                    var message = new PersonPeriodChangedMessage
                    {
                        Ids = ids,
                    };
                    _saveToDenormalizationQueue.Execute(message, runSql);
					atLeastOneMessage = true;
				}
				if (atLeastOneMessage)
				{
					_sendDenormalizeNotification.Notify();
				}
            }
        }
    }
}