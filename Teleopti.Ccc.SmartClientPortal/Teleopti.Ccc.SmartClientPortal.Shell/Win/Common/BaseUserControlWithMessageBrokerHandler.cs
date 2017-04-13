using System;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.MessageBroker.Legacy;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.WinCode.Common;

namespace Teleopti.Ccc.Win.Common
{
    public class BaseUserControlWithMessageBrokerHandler : BaseUserControl, ILocalized, IHelpContext
    {

        // Message handler changed.
        public event EventHandler<EventMessageArgs> OnEventMessageHandlerChanged;

        /// <summary>
        /// Handles the update from message broker.
        /// </summary>
        /// <param name="e">The e.</param>
        public virtual void HandleUpdateFromMessageBroker(EventMessageArgs e){}

        /// <summary>
        /// Handles the message broker.
        /// </summary>
        /// <param name="e">The e.</param>
        public void HandleMessageBroker(EventMessageArgs e)
        {
            HandleUpdateFromMessageBroker(e);
        }

        /// <summary>
        /// Registers for message broker events.
        /// </summary>
        /// <param name="type">The type.</param>
        public void RegisterForMessageBrokerEvents(Type type)
        {
            MessageBrokerInStateHolder.Instance.RegisterEventSubscription
                (OnEventMessageHandler, type);

        }

        public void UnregisterMessageBrokerEvent()
        {
			MessageBrokerInStateHolder.Instance.UnregisterSubscription(OnEventMessageHandler);
        }

        
        /// <summary>
        /// Called when [event message handler].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void OnEventMessageHandler(object sender, EventMessageArgs e)
        {
        	var handler = OnEventMessageHandlerChanged;
            if (handler!= null)
            {
                handler(sender, e);
            }
        }
    }
}
