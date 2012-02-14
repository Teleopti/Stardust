using System;
using System.Runtime.Remoting;
using System.Threading;
using Teleopti.Ccc.AgentPortalCode.Common;
using Teleopti.Ccc.AgentPortalCode.Foundation.StateHandlers;
using Teleopti.Ccc.AgentPortalCode.Helper;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.AgentPortal.AgentScheduleMessenger
{
    public class MessageBrokerHandler : IMessageBrokerModule
    {
        private readonly ScheduleMessengerScreen _scheduleMessengerScreen;
        private readonly PushMessageController _pushMessageController;
        private readonly Guid _moduleId = Guid.NewGuid();
        
        public MessageBrokerHandler(ScheduleMessengerScreen scheduleMessengerScreen, PushMessageController pushMessageController)
        {
            _scheduleMessengerScreen = scheduleMessengerScreen;
            _pushMessageController = pushMessageController;
        }

        public Guid ModuleId
        {
            get { return _moduleId; }
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
            ThreadPool.QueueUserWorkItem(RegisterForMessageBrokerEvents);
        }

        public void UnregisterMessageBrokerSubscriptions()
        {
            if (StateHolder.Instance.MessageBroker != null &&
                StateHolder.Instance.MessageBroker.IsInitialized)
            {
                try
                {
                    StateHolder.Instance.MessageBroker.UnregisterEventSubscription(OnEventMessageHandler);
                    StateHolder.Instance.MessageBroker.UnregisterEventSubscription(OnEventMessageHandler);
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

        private void RegisterForMessageBrokerEvents(object state)
        {
            if (StateHolder.Instance.MessageBroker != null &&
                StateHolder.Instance.MessageBroker.IsInitialized)
            {
                try
                {
                	var referenceId = new Guid(StateHolder.Instance.State.SessionScopeData.LoggedOnPerson.Id);
                	var workflowControlSetId = Guid.Empty;
					if (StateHolder.Instance.State.SessionScopeData.LoggedOnPerson.WorkflowControlSet!=null)
					{
						workflowControlSetId = new Guid(StateHolder.Instance.State.SessionScopeData.LoggedOnPerson.WorkflowControlSet.Id);
					}
					StateHolder.Instance.MessageBroker.RegisterEventSubscription(OnEventMessageHandler, referenceId, typeof(IPerson), typeof(IPersistableScheduleData));
					StateHolder.Instance.MessageBroker.RegisterEventSubscription(OnEventMessageHandler, referenceId, typeof(IPerson), typeof(IMeetingChangedEntity));
					StateHolder.Instance.MessageBroker.RegisterEventSubscription(OnEventMessageHandler, referenceId, typeof(IPerson), typeof(IPushMessageDialogue));
					StateHolder.Instance.MessageBroker.RegisterEventSubscription(OnEventMessageHandler, workflowControlSetId, typeof(IWorkflowControlSet));
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
			if (typeof(IWorkflowControlSet).IsAssignableFrom(eventMessage.Message.InterfaceType) &&
				scheduleWasNotPublishedBefore())
			{
				return true;
			}

        	if (eventMessage.Message.ReferenceObjectId ==
				 new Guid(StateHolder.Instance.State.SessionScopeData.LoggedOnPerson.Id))
            {
                if (typeof(IPersistableScheduleData).IsAssignableFrom(eventMessage.Message.InterfaceType) ||
					typeof(IMeetingChangedEntity).IsAssignableFrom(eventMessage.Message.InterfaceType))
                {
                    if (eventMessage.Message.DomainUpdateType != DomainUpdateType.NotApplicable)
                    {
                        var instance = AgentScheduleStateHolder.Instance();
                        DateTimePeriod asmPeriod = new DateTimePeriod(instance.ScheduleMessengerPeriod.UtcStartTime, instance.ScheduleMessengerPeriod.UtcEndTime);
                        DateTimePeriod messagePeriod = new DateTimePeriod(DateTime.SpecifyKind(eventMessage.Message.EventStartDate, DateTimeKind.Utc), DateTime.SpecifyKind(eventMessage.Message.EventEndDate, DateTimeKind.Utc));
                        return asmPeriod.Intersect(messagePeriod);
                    }
                }
            }
            return false;
        }

		private static bool scheduleWasNotPublishedBefore()
		{
			return StateHolder.Instance.State.SessionScopeData.LoggedOnPerson.WorkflowControlSet.SchedulesPublishedToDate == null ||
			       StateHolder.Instance.State.SessionScopeData.LoggedOnPerson.WorkflowControlSet.SchedulesPublishedToDate <
			       DateTime.Today;
		}
    }
}
