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
		private readonly ICurrentUnitOfWork _unitOfWork;

		public PushMessageSender(ICurrentUnitOfWork unitOfWork)
		{
			_sendPushMessageWhenRootAlteredService = new SendPushMessageWhenRootAlteredService();
			_unitOfWork = unitOfWork;
		}

		public void AdditionalFlush(IEnumerable<IRootChangeInfo> modifiedRoots)
		{

			// this is only because there are nested uows somewhere
			// and the outer persist will not be able to get current uow
			// but in those cases this is always empty
			if (!modifiedRoots.Any())
				return;

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