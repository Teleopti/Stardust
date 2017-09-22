using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.MessageBroker.Client;

namespace Teleopti.Ccc.Infrastructure.SystemCheck
{
	public class CheckMessageBrokerMailBox : ISystemCheck
	{
		private readonly IMessageBrokerComposite _messageBrokerComposite;

		public CheckMessageBrokerMailBox(IMessageBrokerComposite messageBrokerComposite)
		{
			_messageBrokerComposite = messageBrokerComposite;
		}

		public bool IsRunningOk()
		{
			return _messageBrokerComposite.IsPollingAlive;
		}

		public string WarningText
		{
			get { return UserTexts.Resources.CouldNotGetMessagesFromMessageBroker; }
		}
	}
}