using System;
using System.Diagnostics.CodeAnalysis;

namespace Teleopti.Interfaces.MessageBroker.Events
{
    /// <summary>
    /// The arguments for an incoming Event Message.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    public class EventMessageArgs : EventArgs
    {
        private readonly IEventMessage _eventMessage;

        /// <summary>
        /// Constructor taking an IEventMessage
        /// </summary>
        /// <param name="message"></param>
        public EventMessageArgs(IEventMessage message)
        {
            _eventMessage = message;
        }

        /// <summary>
        /// The actual incoming Event Message.
        /// </summary>
        public IEventMessage Message
        {
            get { return _eventMessage; }
        }
    }
}