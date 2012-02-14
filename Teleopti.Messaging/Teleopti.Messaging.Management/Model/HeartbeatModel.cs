using System;
using System.Collections.Generic;
using System.Diagnostics;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Messaging.Client;
using Teleopti.Messaging.Management.BindingLists;
using Teleopti.Messaging.Management.Controllers;

namespace Teleopti.Messaging.Management.Model
{
    public class HeartbeatModel
    {
        private HeartbeatController _controller;
        private IMessageBroker _messageBroker;
        private HeartbeatBindingList _bindingList;
        private string _connectionString;

        public HeartbeatModel(string connectionString)
        {
            _connectionString = connectionString;
        }

        public HeartbeatController Controller
        {
            get { return _controller;  }
            set { _controller = value; }
        }

        public IMessageBroker Broker
        {
            get { return _messageBroker; }
        }

        public void MessageBrokerStart()
        {
            if (_messageBroker == null)
            {
                _messageBroker = MessageBrokerImplementation.GetInstance(_connectionString);
                _messageBroker.StartMessageBroker();
                _messageBroker.ExceptionHandler += new EventHandler<UnhandledExceptionEventArgs>(OnException);
            }
        }

        public HeartbeatBindingList CreateBindingList()
        {
            GuardBroker();

            IList<IEventHeartbeat> configurations = new List<IEventHeartbeat>(Broker.RetrieveHeartbeats());
            _bindingList = new HeartbeatBindingList(configurations);

            _bindingList.AllowNew = false;
            _bindingList.AllowRemove = false;
            _bindingList.AllowEdit = false;
            _bindingList.RaiseListChangedEvents = true;        

            return _bindingList;

        }

        private void OnException(object sender, UnhandledExceptionEventArgs e)
        {
            Debug.WriteLine(e.ExceptionObject);
        }

        private void GuardBroker()
        {
            if (Broker == null)
                MessageBrokerStart();
        }

    }
}
