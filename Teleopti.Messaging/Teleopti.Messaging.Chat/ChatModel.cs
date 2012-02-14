using System;
using System.Text;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Messaging.Chat
{
    public class ChatModel
    {
        public void SendMessage(IMessageBroker broker, string message)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(message);
            broker.SendEventMessage(Guid.Empty, Guid.Empty, typeof(IChat), DomainUpdateType.NotApplicable, bytes);
        }
    }


}
