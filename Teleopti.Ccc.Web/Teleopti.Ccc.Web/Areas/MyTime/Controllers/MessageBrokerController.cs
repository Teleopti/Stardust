using System.Web.Mvc;
using Teleopti.Ccc.Web.Areas.MyTime.Models.MessageBroker;

namespace Teleopti.Ccc.Web.Areas.MyTime.Controllers
{
	public class MessageBrokerController : Controller
	{
		private readonly IUserDataFactory _userDataFactory;

		public MessageBrokerController(IUserDataFactory userDataFactory)
		{
			_userDataFactory = userDataFactory;
		}

		[HttpGet]
		public JsonResult FetchUserData()
		{
			return Json(_userDataFactory.CreateViewModel(), JsonRequestBehavior.AllowGet);
		}
	}
}