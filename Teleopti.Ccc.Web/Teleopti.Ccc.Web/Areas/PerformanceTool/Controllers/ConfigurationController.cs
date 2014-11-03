using System.Linq;
using System.Web.Mvc;
using Autofac.Extras.DynamicProxy2;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Aop;
using Teleopti.Ccc.Web.Core.Aop.Aspects;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.PerformanceTool.Controllers
{
	[Intercept(typeof(AspectInterceptor))]
	public class ConfigurationController : Controller
	{
		private readonly IAbsenceRepository _absenceRepository;
		private readonly ILoggedOnUser _loggedOnUser;

		public ConfigurationController(IAbsenceRepository absenceRepository, ILoggedOnUser loggedOnUser)
		{
			_absenceRepository = absenceRepository;
			_loggedOnUser = loggedOnUser;
		}

		[UnitOfWork, HttpGet]
		public virtual JsonResult GetAAbsenceId()
		{
			var absence = _absenceRepository.LoadAll().First();
			return Json(absence.Id.Value, JsonRequestBehavior.AllowGet);
		}
		
		[UnitOfWork, HttpGet]
		public virtual JsonResult GetAPersonId()
		{
			return Json(_loggedOnUser.CurrentUser().Id.Value, JsonRequestBehavior.AllowGet);
		}
	}
}