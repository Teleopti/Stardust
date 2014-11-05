using System;
using System.Linq;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Controllers
{
	public class ReportsController : Controller
	{

		public ReportsController(IPermissionProvider permissionProvider, ITeamRepository teamRepository, IPersonRepository personRepository, INow date, IAgentStateReader agentStateReader, IUserTimeZone userTimeZone, ICommonAgentNameProvider commonAgentNameProvider)
		{

		}

		[UnitOfWorkAction, HttpGet]
		public JsonResult GetReports()
		{


			return Json(new
			{
				Name = "report1",
				Url = "http://localhost:52858/mytime"
			}, JsonRequestBehavior.AllowGet);
		}

	}

}