using System.Collections.Generic;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Core.DataProvider;
using Teleopti.Ccc.Web.Core.Data;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Controllers
{
    public class TeamScheduleDataController : ApiController
    {
	    private readonly IActivityProvider _teamScheduleDataProvider;

		public TeamScheduleDataController(IActivityProvider teamScheduleDataProvider)
	    {
		    _teamScheduleDataProvider = teamScheduleDataProvider;
	    }

	    [UnitOfWork, HttpGet, Route("api/TeamScheduleData/FetchActivities")]
	    public virtual IList<ActivityViewModel> FetchActivities()
	    {
		    return _teamScheduleDataProvider.GetAll();
	    }
	}
}