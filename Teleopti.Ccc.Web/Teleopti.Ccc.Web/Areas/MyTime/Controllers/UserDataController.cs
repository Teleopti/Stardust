using System.Web.Mvc;
using Teleopti.Ccc.Infrastructure.Web;
using Teleopti.Ccc.Web.Areas.MyTime.Models.MessageBroker;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.MyTime.Controllers
{
	public class UserDataController : Controller
	{
		private readonly IUserDataFactory _userDataFactory;
		private readonly ICurrentHttpContext _context;

		public UserDataController(IUserDataFactory userDataFactory, ICurrentHttpContext context)
		{
			_userDataFactory = userDataFactory;
			_context = context;
		}

		[UnitOfWorkAction]
		[HttpGet]
		public JsonResult FetchUserData()
		{
			return Json(_userDataFactory.CreateViewModel(_context.Current().Request), JsonRequestBehavior.AllowGet);
		}
	}
}