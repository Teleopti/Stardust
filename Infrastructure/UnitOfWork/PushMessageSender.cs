using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class PushMessageSender : IPersistCallback
	{
		private readonly ISendPushMessageWhenRootAlteredService _sendPushMessageWhenRootAlteredService;
		private readonly ICurrentUnitOfWork _unitOfWork;

		public PushMessageSender(ICurrentUnitOfWork unitOfWork)
		{
			_sendPushMessageWhenRootAlteredService = new SendPushMessageWhenRootAlteredService();
			_unitOfWork = unitOfWork;
		}

		public void AdditionalFlush(IEnumerable<IRootChangeInfo> modifiedRoots)
		{
			_sendPushMessageWhenRootAlteredService.SendPushMessages(
				modifiedRoots,
				new PushMessagePersister(
					new PushMessageRepository(_unitOfWork.Current()),
					new PushMessageDialogueRepository(_unitOfWork.Current()),
					new CreatePushMessageDialoguesService()
					));
		}

		public void AfterFlush(IEnumerable<IRootChangeInfo> modifiedRoots)
		{
		}
	}
}