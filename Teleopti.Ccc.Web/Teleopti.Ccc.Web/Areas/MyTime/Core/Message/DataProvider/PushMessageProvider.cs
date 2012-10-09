using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Message.DataProvider
{
	public class PushMessageProvider : IPushMessageProvider
	{
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IPushMessageRepository _repository;

		public PushMessageProvider(ILoggedOnUser loggedOnUser, IPushMessageRepository repository)
		{
			_loggedOnUser = loggedOnUser;
			_repository = repository;
		}

		public int UnreadMessageCount
		{
			get { return _repository.CountUnread(_loggedOnUser.CurrentUser()); }
		}

		public IList<IPushMessageDialogue> GetMessages(Paging paging)
		{
			return new List<IPushMessageDialogue>(_repository.FindUnreadMessage(paging, _loggedOnUser.CurrentUser()));
		}
	}
}