using System.Collections.Generic;
using System.ComponentModel;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Messaging.Management.BindingLists
{
    public class HeartbeatBindingList : BindingList<IEventHeartbeat>
    {

        public HeartbeatBindingList(IList<IEventHeartbeat> beats) : base(beats)
        {
            RaiseListChangedEvents = true;
        }

        public IList<IEventHeartbeat> HeartbeatList
        {
            get { return Items; }
        }

    }
}