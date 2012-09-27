﻿using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Portal
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

        public IList<IPushMessageDialogue> GetMessages()
        {
            throw new System.NotImplementedException();
        }
	}
}