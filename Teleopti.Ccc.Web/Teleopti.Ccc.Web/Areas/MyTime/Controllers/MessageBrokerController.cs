using System.Web.Mvc;
using Teleopti.Ccc.Web.Areas.MyTime.Models.MessageBroker;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.MyTime.Controllers
{
	public class MessageBrokerController : Controller
	{
		private readonly IUserDataFactory _userDataFactory;

		public MessageBrokerController(IUserDataFactory userDataFactory)
		{
			_userDataFactory = userDataFactory;
		}

		[UnitOfWorkAction]
		[HttpGet]
		public JsonResult FetchUserData()
		{
			return Json(_userDataFactory.CreateViewModel(), JsonRequestBehavior.AllowGet);
		}
	}
}