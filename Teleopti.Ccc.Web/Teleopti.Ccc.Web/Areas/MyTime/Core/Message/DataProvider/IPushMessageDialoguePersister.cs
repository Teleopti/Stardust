using Teleopti.Ccc.Web.Areas.MyTime.Models.Message;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Message.DataProvider
{
	public interface IPushMessageDialoguePersister
	{
		MessageViewModel PersistMessage(ConfirmMessageViewModel confirmMessage);
	}
}