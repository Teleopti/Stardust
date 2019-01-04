using System;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.WebPlans)]
	public class PlanningGroupSettingsController : ApiController
	{
		private readonly IPlanningGroupSettingsModelPersister _planningGroupSettingsModelPersister;

		public PlanningGroupSettingsController(IPlanningGroupSettingsModelPersister planningGroupSettingsModelPersister)
		{
			_planningGroupSettingsModelPersister = planningGroupSettingsModelPersister;
		}

		[UnitOfWork, HttpDelete, Route("api/resourceplanner/plangroupsetting/{id}")] 
		public virtual IHttpActionResult Delete(Guid id)
		{
			_planningGroupSettingsModelPersister.Delete(id);
			return Ok();
		}
	}
}