using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Results;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Controllers
{
    public class AbsenceController : ApiController
    {
	    private readonly IAbsenceRepository _absenceRepository;

	    public AbsenceController(IAbsenceRepository absenceRepository)
	    {
		    _absenceRepository = absenceRepository;
	    }

	    [UnitOfWork, HttpGet, Route("api/Absence/GetAvailableAbsences")]
		public virtual JsonResult<IEnumerable<AbsenceViewModel>> GetAvailableAbsences()
	    {
		    var absences = _absenceRepository.LoadAllSortByName();

		    return Json(absences.Select(x => new AbsenceViewModel
		    {
			    Id = x.Id.GetValueOrDefault().ToString(),
			    Name = x.Description.Name,
			    ShortName = x.Description.ShortName
		    }));
	    }
    }

	public class AbsenceViewModel
	{
		public string Id { get; set; }
		public string Name { get; set; }
		public string ShortName { get; set; }
	}
}