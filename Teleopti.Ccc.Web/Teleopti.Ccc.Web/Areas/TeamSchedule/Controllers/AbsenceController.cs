using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Models;

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
		public virtual IEnumerable<AbsenceViewModel> GetAvailableAbsences()
	    {
		    var result = _absenceRepository.LoadAllSortByName()
			    .Where(x => !((Absence) x).IsDeleted)
			    .Select(convertToAbsenceViewModel);

		    return result;
	    }

		[UnitOfWork, HttpGet, Route("api/Absence/GetRequestableAbsences")]
	    public virtual IEnumerable<AbsenceViewModel> GetRequestableAbsences()
	    {
		    var result = _absenceRepository.LoadRequestableAbsence()
			    .Where(x => !((Absence) x).IsDeleted)
			    .Select(convertToAbsenceViewModel);
		    return result;
	    }

	    private static AbsenceViewModel convertToAbsenceViewModel(IAbsence absence)
	    {
		    return new AbsenceViewModel
		    {
			    Id = absence.Id.ToString(),
			    Name = absence.Description.Name,
			    ShortName = absence.Description.ShortName
		    };
	    }
    }
}