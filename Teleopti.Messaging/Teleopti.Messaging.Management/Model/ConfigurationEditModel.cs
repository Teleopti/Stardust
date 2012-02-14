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
    public class ConfigurationEditModel
    {
        private ConfigurationEditController _controller;
        private IMessageBroker _messageBroker;
        private ConfigurationBindingList _bindingList;
        private string _connectionString;

        public ConfigurationEditModel(string connectionString)
        {
            _connectionString = connectionString;
        }

        public ConfigurationEditController Controller
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

        public ConfigurationBindingList CreateBindingList()
        {
            GuardBroker();

            IList<IConfigurationInfo> configurations = new List<IConfigurationInfo>(Broker.RetrieveConfigurations());
            _bindingList = new ConfigurationBindingList(configurations);
            _bindingList.AllowNew = true;
            _bindingList.AllowRemove = true;
            _bindingList.AllowEdit = true;
            _bindingList.RaiseListChangedEvents = true;
            
            return _bindingList;

        }

        public void SaveConfigurations()
        {
            IMessageBroker instance = MessageBrokerImplementation.GetInstance();
            instance.UpdateConfigurations(_bindingList.ConfigurationList);
        }

        private void OnException(object sender, UnhandledExceptionEventArgs e)
        {
            Debug.WriteLine(e.ExceptionObject);
        }

        public void DeleteConfigurationItem(IConfigurationInfo configurationInfo)
        {

            GuardBroker();

            Broker.DeleteConfigurationItem(configurationInfo);

        }

        private void GuardBroker()
        {
            if (Broker == null)
                MessageBrokerStart();
        }

        public void RemoveListItem(IConfigurationInfo configurationInfo)
        {
            _bindingList.Remove(configurationInfo);    
        }

    }
}
