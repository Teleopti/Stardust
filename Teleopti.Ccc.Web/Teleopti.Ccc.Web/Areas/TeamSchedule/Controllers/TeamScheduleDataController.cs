using System.Collections.Generic;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Core.DataProvider;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Models;
using Teleopti.Ccc.Web.Core.Data;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Controllers
{
    public class TeamScheduleDataController : ApiController
    {
	    private readonly IActivityProvider _teamScheduleDataProvider;
	    private readonly IScheduleValidationProvider _validationProvider;

		public TeamScheduleDataController(IActivityProvider teamScheduleDataProvider, IScheduleValidationProvider validationProvider)
		{
			_teamScheduleDataProvider = teamScheduleDataProvider;
			_validationProvider = validationProvider;
		}

	    [UnitOfWork, HttpGet, Route("api/TeamScheduleData/FetchActivities")]
	    public virtual IList<ActivityViewModel> FetchActivities()
	    {
		    return _teamScheduleDataProvider.GetAll();
	    }

		[UnitOfWork, HttpPost, Route("api/TeamScheduleData/FetchRuleValidationResult")]
		public virtual IList<BusinessRuleValidationResult> FetchRuleValidationResult(FetchRuleValidationResultFormData input)
		{
			return _validationProvider.GetBusineeRuleValidationResults(input);
		}
	}
}