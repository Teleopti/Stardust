using System;
using System.Collections.Generic;
using System.Diagnostics;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Messaging.Client;
using Teleopti.Messaging.Management.BindingLists;
using Teleopti.Messaging.Management.Controllers;

namespace Teleopti.Messaging.Management.Model
{
    public class MulticastAddressEditModel
    {
        private IMessageBroker _messageBroker;
        private AddressBindingList _bindingList;
        private MulticastAddressEditController _controller;
        private string _connectionString;

        public MulticastAddressEditModel(string connectionString)
        {
            _connectionString = connectionString;
        }

        public MulticastAddressEditController Controller
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

        public AddressBindingList CreateBindingList()
        {
            GuardBroker();

            IList<IMessageInformation> addresses = new List<IMessageInformation>(Broker.RetrieveAddresses());
            _bindingList = new AddressBindingList(addresses);
            _bindingList.AllowNew = true;
            _bindingList.AllowRemove = true;
            _bindingList.AllowEdit = true;
            _bindingList.RaiseListChangedEvents = true;
            
            return _bindingList;

        }

        public void SaveConfigurations()
        {
            IMessageBroker instance = MessageBrokerImplementation.GetInstance();
            instance.UpdateAddresses(_bindingList.Addresses);
        }

        private void OnException(object sender, UnhandledExceptionEventArgs e)
        {
            Debug.WriteLine(e.ExceptionObject);
        }

        public void DeleteConfigurationItem(IMessageInformation addressInfo)
        {

            GuardBroker();

            Broker.DeleteAddressItem(addressInfo);

        }

        private void GuardBroker()
        {
            if (Broker == null)
                MessageBrokerStart();
        }

        public void RemoveListItem(IMessageInformation addressInfo)
        {
            _bindingList.Remove(addressInfo);    
        }

    }

}
