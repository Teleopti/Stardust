using System;
using Teleopti.Ccc.Sdk.ServiceBus.Messages;

namespace Teleopti.Ccc.ServiceBus.ErrorViewer
{
    public interface IIncomingMessageHandler
    {
        event Action<CustomErrorMessage> MessageArrived;
        void HandleIncomingMessage(CustomErrorMessage customErrorMessage);
    }

    public class IncomingMessageHandler : IIncomingMessageHandler
    {
        public event Action<CustomErrorMessage> MessageArrived;

        public void HandleIncomingMessage(CustomErrorMessage customErrorMessage)
        {
            var messageArrived = MessageArrived;
            if (messageArrived!=null)
            {
                messageArrived.Invoke(customErrorMessage);
            }
        }
    }
}
