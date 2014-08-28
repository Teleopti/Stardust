using System;
using System.Runtime.Remoting;
using Teleopti.Ccc.AgentPortalCode.Foundation.StateHandlers;
using Teleopti.Ccc.AgentPortalCode.Helper;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.AgentPortal.AgentScheduleMessenger
{
    public class MessageBrokerHandler : IInitiatorIdentifier
    {
        private readonly ScheduleMessengerScreen _scheduleMessengerScreen;
        private readonly PushMessageController _pushMessageController;
        private readonly Guid _instanceId = Guid.NewGuid();
        
        public MessageBrokerHandler(ScheduleMessengerScreen scheduleMessengerScreen, PushMessageController pushMessageController)
        {
            _scheduleMessengerScreen = scheduleMessengerScreen;
            _pushMessageController = pushMessageController;
        }

        public Guid InitiatorId
        {
            get { return _instanceId; }
        }

        public event EventHandler ScheduleChanged;

        /// <summary>
        /// Called when [event message handler].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-08-22
        /// Henrik 2009-07-15: BW-warning, This class really needs refactoring!
        /// </remarks>
        private void OnEventMessageHandler(object sender, EventMessageArgs e)
        {
            if (_scheduleMessengerScreen.InvokeRequired)
            {
                _scheduleMessengerScreen.BeginInvoke(new EventHandler<EventMessageArgs>(OnEventMessageHandler), sender, e);
            }
            else
            {
                //Let the controller handle it instead. Move MB-code to the controller...
                _pushMessageController.MessageChanged(e);

                if (CheckScheduleMessage(e))
                {
                    //Reload Current Schedule
                	var handler = ScheduleChanged;
                    if (handler!=null)
                    {
                        handler.Invoke(this,EventArgs.Empty);
                    }
                }
            }
        }

        public void StartMessageBrokerListener()
        {
            RegisterForMessageBrokerEvents();
        }

        public void UnregisterMessageBrokerSubscriptions()
        {
            if (StateHolder.Instance.MessageBroker != null &&
                StateHolder.Instance.MessageBroker.IsAlive)
            {
                try
                {
                    StateHolder.Instance.MessageBroker.UnregisterEventSubscription(OnEventMessageHandler);
                    StateHolder.Instance.MessageBroker.UnregisterEventSubscription(OnEventMessageHandler);
                }
                catch (RemotingException exp)
                {
                    // TODO: how should we handle MB exceptions ? 
                    Console.WriteLine(exp.Message);
                }
            }
        }

        private void RegisterForMessageBrokerEvents()
        {
            if (StateHolder.Instance.MessageBroker != null &&
                StateHolder.Instance.MessageBroker.IsAlive)
            {
                try
                {
                	var referenceId = StateHolder.Instance.State.SessionScopeData.LoggedOnPerson.Id.GetValueOrDefault();
                	var businessUnitId = StateHolder.Instance.State.SessionScopeData.BusinessUnit.Id.GetValueOrDefault();
                	var datasource = StateHolder.Instance.State.SessionScopeData.DataSource.Name;
					StateHolder.Instance.MessageBroker.RegisterEventSubscription(datasource,businessUnitId, OnEventMessageHandler, referenceId, typeof(IPerson), typeof(IScheduleChangedInDefaultScenario));
					StateHolder.Instance.MessageBroker.RegisterEventSubscription(datasource,businessUnitId, OnEventMessageHandler, referenceId, typeof(IPerson), typeof(IPushMessageDialogue));
                }
                catch (RemotingException e)
                {
                    // TODO: how should we handle MB exceptions ? 
                    Console.WriteLine(e.Message);
                }
            }
        }

        //Checks if the message affects the schedule of the ASM
        private static bool CheckScheduleMessage(EventMessageArgs eventMessage)
        {
			if (eventMessage.Message.ReferenceObjectId == StateHolder.Instance.State.SessionScopeData.LoggedOnPerson.Id)
			{
				if (typeof (IScheduleChangedInDefaultScenario).IsAssignableFrom(eventMessage.Message.InterfaceType))
				{
					return eventMessage.Message.EventStartDate <= DateTime.Today.AddDays(1) &&
					       eventMessage.Message.EventEndDate >= DateTime.Today;
				}
			}

        	return false;
        }
    }
}
