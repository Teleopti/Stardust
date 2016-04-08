using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class MessageBrokerSender : IPersistCallback
	{
		private readonly Func<IMessageBrokerComposite> _messageBroker;
		private readonly ICurrentInitiatorIdentifier _initiatorIdentifier;

		public MessageBrokerSender(Func<IMessageBrokerComposite> messageBroker, ICurrentInitiatorIdentifier initiatorIdentifier)
		{
			_messageBroker = messageBroker;
			_initiatorIdentifier = initiatorIdentifier;
		}

		public void AdditionalFlush(IUnitOfWork unitOfWork, IEnumerable<IRootChangeInfo> modifiedRoots)
		{
		}

		public void AfterFlush(IEnumerable<IRootChangeInfo> modifiedRoots)
		{
		}

		public void AfterCommit(IEnumerable<IRootChangeInfo> modifiedRoots)
		{
			var identifier = _initiatorIdentifier.Current();
			var moduleId = identifier == null ? Guid.Empty : identifier.InitiatorId;
			new NotifyMessageBroker(_messageBroker()).Notify(moduleId, modifiedRoots);
		}
	}
}