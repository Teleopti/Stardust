using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Message.DataProvider
{
	public class PushMessageProvider : IPushMessageProvider
	{
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IPushMessageDialogueRepository _repository;

		public PushMessageProvider(ILoggedOnUser loggedOnUser, IPushMessageDialogueRepository repository)
		{
			_loggedOnUser = loggedOnUser;
			_repository = repository;
		}

		public int UnreadMessageCount => _repository.CountUnread(_loggedOnUser.CurrentUser());

		public IList<IPushMessageDialogue> GetMessages(Paging paging)
		{
			return new List<IPushMessageDialogue>(_repository.FindUnreadMessages(paging, _loggedOnUser.CurrentUser()));
		}

		public IPushMessageDialogue GetMessage(Guid messageId)
		{
			return _repository.Get(messageId);
		}
	}
}