using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
    public class PersonChangedMessageSender :IMessageSender
    {
        private readonly IEnumerable<Type> _triggerInterfaces = new List<Type>
		                                                        	{
		                                                        		typeof (IPerson),
		                                                        		typeof (IPersonWriteProtectionInfo),
		                                                        	};
		private readonly IEnumerable<Type> _otherTriggerInterfaces = new List<Type>
		                                                        	{
		                                                        		typeof (ITeam),
		                                                        		typeof (ISite),
		                                                        	};

    	private readonly ISendDenormalizeNotification _sendDenormalizeNotification;
    	private readonly ISaveToDenormalizationQueue _saveToDenormalizationQueue;

		public PersonChangedMessageSender(ISendDenormalizeNotification sendDenormalizeNotification, ISaveToDenormalizationQueue saveToDenormalizationQueue)
		{
			_sendDenormalizeNotification = sendDenormalizeNotification;
			_saveToDenormalizationQueue = saveToDenormalizationQueue;
		}

	    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider",
		    MessageId = "System.String.Format(System.String,System.Object)"),
	     System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods",
		     MessageId = "0")]
	    public void Execute(IRunSql runSql, IEnumerable<IRootChangeInfo> modifiedRoots)
	    {
		    var atLeastOneMessage = false;
		    var rootChangeInfos = modifiedRoots as IList<IRootChangeInfo> ?? modifiedRoots.ToList();
		    var affectedInterfaces = from r in rootChangeInfos
		                             let t = r.Root.GetType()
		                             where _triggerInterfaces.Any(ti => ti.IsAssignableFrom(t))
		                             select (IAggregateRoot) r.Root;

		    foreach (var personList in affectedInterfaces.Batch(25))
		    {
			    var idsAsString = personList.Select(p => p.Id.GetValueOrDefault()).ToArray();
			    var message = new PersonChangedMessage();
			    message.SetPersonIdCollection(idsAsString);
			    _saveToDenormalizationQueue.Execute(message, runSql);
			    atLeastOneMessage = true;
		    }
			
			if (rootChangeInfos.Select(r => new {r, t = r.Root.GetType()}).Count(@t1 => _otherTriggerInterfaces.Any(ti => ti.IsAssignableFrom(@t1.t))) > 0)
			{
				var message = new PersonChangedMessage {SerializedPeople = Guid.Empty.ToString()};
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