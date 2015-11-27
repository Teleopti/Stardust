using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.PerformanceTool.Controllers
{
	public class PerformanceToolConfigurationController : ApiController
	{
		private readonly IAbsenceRepository _absenceRepository;
		private readonly ILoggedOnUser _loggedOnUser;

		public PerformanceToolConfigurationController(IAbsenceRepository absenceRepository, ILoggedOnUser loggedOnUser)
		{
			_absenceRepository = absenceRepository;
			_loggedOnUser = loggedOnUser;
		}

		[UnitOfWork, HttpGet, Route("api/PerformanceTool/Configuration/AnAbsenceId")]
		public virtual IHttpActionResult GetAAbsenceId()
		{
			var absence = _absenceRepository.LoadAll().First();
			return Ok(absence.Id.GetValueOrDefault());
		}

		[UnitOfWork, HttpGet, Route("api/PerformanceTool/Configuration/APersonId")]
		public virtual IHttpActionResult GetAPersonId()
		{
			return Ok(_loggedOnUser.CurrentUser().Id.GetValueOrDefault());
		}
	}
}