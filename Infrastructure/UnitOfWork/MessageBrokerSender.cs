using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class MessageBrokerSender : ITransactionHook
	{
		private readonly Func<IMessageBrokerComposite> _messageBroker;
		private readonly ICurrentInitiatorIdentifier _initiatorIdentifier;

		public MessageBrokerSender(Func<IMessageBrokerComposite> messageBroker, ICurrentInitiatorIdentifier initiatorIdentifier)
		{
			_messageBroker = messageBroker;
			_initiatorIdentifier = initiatorIdentifier;
		}
		
		public void AfterCompletion(IEnumerable<IRootChangeInfo> modifiedRoots)
		{
			var identifier = _initiatorIdentifier.Current();
			var moduleId = identifier == null ? Guid.Empty : identifier.InitiatorId;
			new NotifyMessageBroker(_messageBroker()).Notify(moduleId, modifiedRoots);
		}
	}
}