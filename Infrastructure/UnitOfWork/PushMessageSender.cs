using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class PushMessageSender : IPersistCallback
	{
		private readonly ISendPushMessageWhenRootAlteredService _sendPushMessageWhenRootAlteredService;

		public PushMessageSender()
		{
			_sendPushMessageWhenRootAlteredService = new SendPushMessageWhenRootAlteredService();
		}

		public void AdditionalFlush(IUnitOfWork unitOfWork, IEnumerable<IRootChangeInfo> modifiedRoots)
		{
			_sendPushMessageWhenRootAlteredService.SendPushMessages(
				modifiedRoots,
				new PushMessagePersister(
					new PushMessageRepository(unitOfWork),
					new PushMessageDialogueRepository(unitOfWork),
					new CreatePushMessageDialoguesService()
					));
		}

		public void AfterFlush(IEnumerable<IRootChangeInfo> modifiedRoots)
		{
		}
	}
}