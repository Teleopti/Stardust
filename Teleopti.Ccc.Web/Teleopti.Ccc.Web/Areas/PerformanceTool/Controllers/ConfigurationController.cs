using System.Linq;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.PerformanceTool.Controllers
{
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