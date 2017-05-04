using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Message.DataProvider;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakePushMessageProvider : IPushMessageProvider
	{
		int IPushMessageProvider.UnreadMessageCount => 3;

		IPushMessageDialogue IPushMessageProvider.GetMessage(Guid messageId)
		{
			throw new NotImplementedException();
		}

		IList<IPushMessageDialogue> IPushMessageProvider.GetMessages(Paging paging)
		{
			throw new NotImplementedException();
		}
	}
}
