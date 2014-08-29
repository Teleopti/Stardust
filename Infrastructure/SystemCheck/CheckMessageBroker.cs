using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Interfaces.MessageBroker.Client.Composite;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.Infrastructure.SystemCheck
{
    public class CheckMessageBroker : ISystemCheck
    {
        private readonly IMessageBrokerComposite _messageBroker;

        public CheckMessageBroker(IMessageBrokerComposite messageBroker)
        {
            _messageBroker = messageBroker;
        }

        public bool IsRunningOk()
        {
            return string.IsNullOrEmpty(_messageBroker.ServerUrl) ||
                   _messageBroker.IsAlive;
        }

        public string WarningText
        {
            get { return UserTexts.Resources.CheckMessageBrokerWarning; }
        }
    }
}
