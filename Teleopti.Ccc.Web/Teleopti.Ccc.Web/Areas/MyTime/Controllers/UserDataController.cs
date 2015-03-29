using System;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Web.Areas.MyTime.Models.MessageBroker;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;

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
		public JsonResult FetchUserData(string date)
		{
			var datetime = date==null?new DateTime(): date.Utc(); //"2015-03-29 08:00"
			return Json(_userDataFactory.CreateViewModel(datetime), JsonRequestBehavior.AllowGet);
		}
	}
}