using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Remoting.Messaging;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Logging;
using Teleopti.Logging.Core;
using Teleopti.Messaging.Client;
using Teleopti.Messaging.Events;
using Teleopti.Messaging.Management.BindingLists;
using Teleopti.Messaging.Management.Controllers;

namespace Teleopti.Messaging.Management.Model
{
    public delegate LogbookEntryBindingList CreateBindingListHandler();
    public delegate List<String> StringListHandler();
    public class EventLogEntryModel
    {
        private EventLogEntryController _controller;
        private IMessageBroker _messageBroker;
        private LogbookEntryBindingList _bindingList;
        private List<string> _users;
        private string _connectionString;

        public EventLogEntryModel(string connectionString)
        {
            _connectionString = connectionString;
        }

        public EventLogEntryController Controller
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
                _messageBroker.RegisterEventSubscription(null,Guid.Empty, OnEvent, Guid.Empty, typeof(ILogEntry));
                _messageBroker.ExceptionHandler += new EventHandler<UnhandledExceptionEventArgs>(OnException);
                BaseLogger.Instance.WriteLine(EventLogEntryType.SuccessAudit, typeof(ILogEntry), "Successfully instantiated the Message Broker.");
            }
        }

        private void OnEvent(object sender, EventMessageArgs e)
        {
            _bindingList.LogbookEntryList.Add(new LogbookEntry(e.Message));
            ((List<ILogbookEntry>)_bindingList.LogbookEntryList).Sort();
        }

        private void OnException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception exc = (Exception)e.ExceptionObject;
            BaseLogger.Instance.WriteLine(EventLogEntryType.Error, GetType(), exc.Message + exc.StackTrace);
        }

        private void GuardBroker()
        {
            if (Broker == null)
                MessageBrokerStart();
        }

        public void CreateUserStringListAsync(StringListHandler handler)
        {
            GuardBroker();
            AsyncCallback cb = new AsyncCallback(UsersStringListTarget);
            if (handler != null)
                handler.BeginInvoke(cb, null);
        }

        private void UsersStringListTarget(IAsyncResult ar)
        {
            try
            {
                AsyncResult state = (AsyncResult)ar;
                StringListHandler asyncDelegate = (StringListHandler) state.AsyncDelegate;
                _users = asyncDelegate.EndInvoke(ar);
                _controller.SetAutoCompleteSourceUsers(_users);
            }
            catch (Exception exc)
            {
                Debug.WriteLine(exc);
            }            
        }

        public void CreateBindingListAsync(CreateBindingListHandler handler)
        {
            GuardBroker();
            AsyncCallback cb = new AsyncCallback(BindingListTarget);
            if (handler != null)
                handler.BeginInvoke(cb, null);
            
        }

        private void BindingListTarget(IAsyncResult ar)
        {
            try
            {
                AsyncResult state = (AsyncResult)ar;
                CreateBindingListHandler asyncDelegate = (CreateBindingListHandler)state.AsyncDelegate;
                _bindingList = asyncDelegate.EndInvoke(ar);
                _controller.SetBindingList(_bindingList);
            }
            catch (Exception exc)
            {
                Debug.WriteLine(exc);
            }
        }

        public LogbookEntryBindingList CreateBindingList()
        {
            List<ILogbookEntry> logBook = new List<ILogbookEntry>(Broker.RetrieveLogbookEntries());
            logBook.Sort();
            _bindingList = new LogbookEntryBindingList(logBook);
            _bindingList.AllowNew = false;
            _bindingList.AllowRemove = false;
            _bindingList.AllowEdit = false;
            _bindingList.RaiseListChangedEvents = true;        
            return _bindingList;
        }

    }
}
