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
	    private readonly IShiftCategoryProvider _shiftCategoryProvider;

		public TeamScheduleDataController(IActivityProvider teamScheduleDataProvider, IScheduleValidationProvider validationProvider, IShiftCategoryProvider shiftCategoryProvider)
		{
			_teamScheduleDataProvider = teamScheduleDataProvider;
			_validationProvider = validationProvider;
			_shiftCategoryProvider = shiftCategoryProvider;
		}

	    [UnitOfWork, HttpGet, Route("api/TeamScheduleData/FetchActivities")]
	    public virtual IList<ActivityViewModel> FetchActivities()
	    {
		    return _teamScheduleDataProvider.GetAll();
	    }

		[UnitOfWork, HttpPost, Route("api/TeamScheduleData/FetchRuleValidationResult")]
		public virtual IList<BusinessRuleValidationResult> FetchRuleValidationResult([FromBody]FetchRuleValidationResultFormData input)
		{
			return _validationProvider.GetBusinessRuleValidationResults(input);
		}

	    [UnitOfWork, HttpGet, Route("api/TeamScheduleData/FetchShiftCategories")]
	    public virtual IList<ShiftCategoryViewModel> FetchShiftCategories()
	    {
		    return _shiftCategoryProvider.GetAll();
	    }
	}
}