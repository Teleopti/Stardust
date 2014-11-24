using System.Web;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.MessageBroker
{
	public interface IUserDataFactory
	{
		UserData CreateViewModel(HttpRequestBase context);
	}
}