using System;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.WebPlans)]
	public class PlanningGroupController : ApiController 
	{
		private readonly IPlanningGroupModelPersister _planningGroupModelPersister;
		private readonly IFetchPlanningGroupModel _fetchPlanningGroupModel;

		public PlanningGroupController(IPlanningGroupModelPersister planningGroupModelPersister, IFetchPlanningGroupModel fetchPlanningGroupModel)
		{
			_planningGroupModelPersister = planningGroupModelPersister;
			_fetchPlanningGroupModel = fetchPlanningGroupModel;
		}

		[UnitOfWork, HttpPost, Route("api/resourceplanner/planninggroup")]
		public virtual IHttpActionResult Save(PlanningGroupModel model)
		{
			_planningGroupModelPersister.Persist(model);
			return Ok();
		}

		[UnitOfWork, HttpGet, Route("api/resourceplanner/planninggroup")]
		public virtual IHttpActionResult List()
		{
			return Ok(_fetchPlanningGroupModel.FetchAll());
		}

		[UnitOfWork, HttpGet, Route("api/resourceplanner/planninggroup/{id}")]
		public virtual IHttpActionResult Get(Guid id)
		{
			return Ok(_fetchPlanningGroupModel.Fetch(id));
		}

		[UnitOfWork, HttpDelete, Route("api/resourceplanner/planninggroup/{id}")]
		public virtual IHttpActionResult DeletePlanningGroup(Guid id)
		{
			_planningGroupModelPersister.Delete(id);
			return Ok();
		}
	}
}