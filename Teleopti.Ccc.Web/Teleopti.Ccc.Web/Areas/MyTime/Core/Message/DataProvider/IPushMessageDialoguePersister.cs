using System;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Message;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Message.DataProvider
{
	public interface IPushMessageDialoguePersister
	{
		MessageViewModel Persist(Guid messageId);

		MessageViewModel PersistMessage(ConfirmMessageViewModel confirmMessage);
	}
}