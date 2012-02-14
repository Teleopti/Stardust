using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Teleopti.Ccc.Win.PeopleAdmin.MessageBroker
{
 public interface IPeopleAdminMessageBroker
    {
        void RegisterForMessageBrokerEvents(Type type);
        void UnregisterForMessageBrokerEvents();
    }
}