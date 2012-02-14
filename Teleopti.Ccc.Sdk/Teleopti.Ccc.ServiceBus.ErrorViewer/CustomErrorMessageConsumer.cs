using Rhino.ServiceBus;
using Teleopti.Ccc.Sdk.ServiceBus.Messages;

namespace Teleopti.Ccc.ServiceBus.ErrorViewer
{
    public class CustomErrorMessageConsumer : ConsumerOf<CustomErrorMessage>
    {
        private readonly IIncomingMessageHandler _incomingMessageHandler;

        public CustomErrorMessageConsumer(IIncomingMessageHandler incomingMessageHandler)
        {
            _incomingMessageHandler = incomingMessageHandler;
        }

        public void Consume(CustomErrorMessage message)
        {
            _incomingMessageHandler.HandleIncomingMessage(message);
        }
    }
}
