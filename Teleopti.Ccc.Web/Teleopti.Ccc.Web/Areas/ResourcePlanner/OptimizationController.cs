using System;
using System.Web.Http;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.WebSchedules)]
	public class OptimizationController : ApiController
	{
		private readonly ScheduleOptimization _scheduleOptimization;
		private readonly IntradayOptimizationFromWeb _intradayOptimizationFromWeb;

		public OptimizationController(ScheduleOptimization scheduleOptimization, IntradayOptimizationFromWeb intradayOptimizationFromWeb)
		{
			_scheduleOptimization = scheduleOptimization;
			_intradayOptimizationFromWeb = intradayOptimizationFromWeb;
		}

		[HttpPost, Route("api/ResourcePlanner/optimize/FixedStaff/{id}")]
		public virtual IHttpActionResult FixedStaff(Guid id)
		{
			return Ok(_scheduleOptimization.Execute(id));
		}

		[HttpPost, Route("api/ResourcePlanner/optimize/intraday/{id}")]
		public void OptimizeIntraday(Guid id)
		{
			_intradayOptimizationFromWeb.Execute(id);
		}
	}
}