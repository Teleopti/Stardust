using System.Collections.Generic;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Messaging.Management.Controllers;

namespace Teleopti.Messaging.Management.Model
{
    public class ManagementModel
    {
        private ManagementViewController _controller;

        public ManagementModel()
        {
        }

        public ManagementViewController Controller
        {
            get { return _controller;  }
            set { _controller = value; }
        }

        public void ExitApplication()
        {

        }

        public IList<IConfigurationInfo> ViewConfiguration()
        {
            return null;
        }

        public void EditConfiguration(IConfigurationInfo configurationInfo)
        {

        }


        public IList<IMessageInformation> ViewMulticastAddresses()
        {
            return null;
        }

        public IList<ILogEntry> ViewLog()
        {
            return null;
        }

        public IList<IEventUser> ViewUser()
        {
            return null;
        }

        public IList<IEventSubscriber> ViewSubscriber()
        {
            return null;
        }

    }
}
