using System.Web.Mvc;
using Teleopti.Ccc.Web.Areas.MyTime.Models.MessageBroker;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.MyTime.Controllers
{
	public class UserDataController : Controller
	{
		private readonly IUserDataFactory _userDataFactory;

		public UserDataController(IUserDataFactory userDataFactory)
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