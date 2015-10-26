using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Optimization;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner
{
	public class DayOffSettingsController : ApiController
	{
		private readonly IFetchDayOffSettingsModel _fetchDayOffSettingsModel;

		public DayOffSettingsController(IFetchDayOffSettingsModel fetchDayOffSettingsModel)
		{
			_fetchDayOffSettingsModel = fetchDayOffSettingsModel;
		}

		[UnitOfWork, HttpGet, Route("api/resourceplanner/dayoffsettings"), Authorize]
		public virtual IHttpActionResult GetAllDayOffSettings()
		{
			return Ok(_fetchDayOffSettingsModel.FetchAll());
		}
	}
}