using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Optimization;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner
{
	public class DayOffSettingsController : ApiController
	{
		private readonly IFetchDayOffRulesModel _fetchDayOffRulesModel;

		public DayOffSettingsController(IFetchDayOffRulesModel fetchDayOffRulesModel)
		{
			_fetchDayOffRulesModel = fetchDayOffRulesModel;
		}

		[UnitOfWork, HttpGet, Route("api/resourceplanner/dayoffrules/default"), Authorize]
		public virtual IHttpActionResult GetDefaultSettings()
		{
			return Ok(_fetchDayOffRulesModel.FetchDefaultRules());
		}
	}
}