using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Message.DataProvider
{
	public interface IPushMessageProvider
	{
		int UnreadMessageCount { get; }
		IList<IPushMessageDialogue> GetMessages(Paging paging);
		IPushMessageDialogue GetMessage(Guid messageId);
	}
}