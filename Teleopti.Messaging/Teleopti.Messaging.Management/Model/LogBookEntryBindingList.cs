using System.Collections.Generic;
using System.ComponentModel;
using Teleopti.Interfaces.MessageBroker.Core;

namespace Teleopti.Messaging.Management.Model
{

    public class LogbookEntryBindingList : BindingList<ILogbookEntry>
    {
        public LogbookEntryBindingList(IList<ILogbookEntry> logBookEntries) : base(logBookEntries)
        {
            RaiseListChangedEvents = true;
        }

        public IList<ILogbookEntry> LogbookEntryList
        {
            get { return Items;  }
        }
    }

}
